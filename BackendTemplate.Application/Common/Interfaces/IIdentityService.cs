using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;

namespace BackendTemplate.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<Result<string>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<Result> RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
