using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Infrastructure.Persistence;

namespace OnlineTesting.Infrastructure.Subscriptions;

public class SubscriptionChecker : ISubscriptionChecker
{
    private readonly ApplicationDbContext _db;

    public SubscriptionChecker(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsActiveAsync(Guid userId, CancellationToken ct) =>
        _db.Subscriptions.AnyAsync(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow, ct);
}
