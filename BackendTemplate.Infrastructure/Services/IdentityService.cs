using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.Entities;
using BackendTemplate.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BackendTemplate.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IApplicationDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure(Error.Forbidden("Auth.AccountInactive", "This user account is currently deactivated."));
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
        var (accessToken, jwtId) = GenerateJwtToken(user, userRoles);
        var refreshToken = GenerateRefreshToken(user.Id, jwtId, ipAddress);

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            userRoles);

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<string>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<string>.Failure(Error.Conflict("Auth.EmailExists", "A user with this email address already exists."));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<string>.Failure(Error.Validation("Auth.RegistrationFailed", errors));
        }

        var roleToAssign = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;
        if (!await _roleManager.RoleExistsAsync(roleToAssign))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
        }

        await _userManager.AddToRoleAsync(user, roleToAssign);

        return Result<string>.Success(user.Id);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidAccessToken", "Invalid access token supplied."));
        }

        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var jwtId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(jwtId))
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.InvalidClaims", "Token claims are missing or malformed."));
        }

        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (storedRefreshToken == null)
        {
            return Result<AuthResponse>.Failure(Error.NotFound("Auth.TokenNotFound", "Refresh token was not found."));
        }

        if (storedRefreshToken.IsUsed)
        {
            // Possible token reuse attack — revoke all tokens for this user
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in userTokens)
            {
                token.Revoke(ipAddress ?? "unknown", "Attempted reuse of already used refresh token.");
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<AuthResponse>.Failure(Error.Forbidden("Auth.SecurityBreach", "Security alert: Attempted reuse of compromised refresh token."));
        }

        if (storedRefreshToken.IsRevoked || storedRefreshToken.IsExpired)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.TokenExpired", "Refresh token has expired or been revoked."));
        }

        if (storedRefreshToken.JwtId != jwtId || storedRefreshToken.UserId != userId)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.TokenMismatch", "Refresh token does not match access token details."));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Auth.UserNotFound", "Associated user account not found or is inactive."));
        }

        // Rotate token
        storedRefreshToken.IsUsed = true;

        var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
        var (newAccessToken, newJwtId) = GenerateJwtToken(user, userRoles);
        var newRefreshToken = GenerateRefreshToken(user.Id, newJwtId, ipAddress);

        storedRefreshToken.ReplacedByToken = newRefreshToken.Token;

        await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            newAccessToken,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt,
            userRoles);

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result> RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token == null)
        {
            return Result.Failure(Error.NotFound("Auth.TokenNotFound", "Refresh token not found."));
        }

        if (!token.IsActive)
        {
            return Result.Failure(Error.Conflict("Auth.TokenAlreadyInactive", "Token is already inactive or revoked."));
        }

        token.Revoke(ipAddress ?? "unknown", "Revoked manually by user request.");
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(Error.Validation("Auth.PasswordChangeFailed", errors));
        }

        return Result.Success();
    }

    private (string Token, string JwtId) GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"]!;
        var key = Encoding.UTF8.GetBytes(secret);
        var jwtId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, user.Id),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("roles", role));
        }

        var expirationMinutes = double.TryParse(jwtSettings["ExpirationMinutes"], out var minutes) ? minutes : 60;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), jwtId);
    }

    private RefreshToken GenerateRefreshToken(string userId, string jwtId, string? ipAddress)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationDays = double.TryParse(jwtSettings["RefreshTokenExpirationDays"], out var days) ? days : 7;

        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomBytes),
            JwtId = jwtId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedByIp = ipAddress ?? "unknown",
            IsRevoked = false,
            IsUsed = false
        };
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"]!;
        var key = Encoding.UTF8.GetBytes(secret);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false // Ignore expiry here so we can refresh
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
