using BackendTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Student> Students { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
