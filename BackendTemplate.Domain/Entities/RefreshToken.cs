using BackendTemplate.Domain.Common;

namespace BackendTemplate.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired && !IsUsed;

    public void Revoke(string ipAddress, string? reason = null, string? replacedBy = null)
    {
        IsRevoked = true;
        RevokedByIp = ipAddress;
        ReasonRevoked = reason ?? "Revoked without reason";
        ReplacedByToken = replacedBy;
    }
}
