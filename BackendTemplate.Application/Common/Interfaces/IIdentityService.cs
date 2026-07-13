using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;

namespace BackendTemplate.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<string>> RegisterAsync(RegisterRequest request);
}
