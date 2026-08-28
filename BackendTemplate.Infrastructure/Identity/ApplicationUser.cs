using BackendTemplate.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackendTemplate.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
