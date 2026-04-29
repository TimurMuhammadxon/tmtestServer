using Microsoft.EntityFrameworkCore;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ExternalLogin> ExternalLogins { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}