namespace BackendTemplate.Application.Common.Models;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Role = null);

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);

public record RevokeTokenRequest(
    string RefreshToken);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public record AuthResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    List<string> Roles);
