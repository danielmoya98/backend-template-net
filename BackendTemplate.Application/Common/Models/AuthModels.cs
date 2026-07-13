namespace BackendTemplate.Application.Common.Models;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record AuthResponse(string Id, string Email, string Token, string FirstName, string LastName);
