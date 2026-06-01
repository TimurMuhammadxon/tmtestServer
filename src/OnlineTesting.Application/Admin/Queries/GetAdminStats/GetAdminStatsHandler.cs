using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Payments;

namespace OnlineTesting.Application.Admin.Queries.GetAdminStats;

public class GetAdminStatsHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IApplicationDbContext _db;

    public GetAdminStatsHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AdminStatsDto> Handle(GetAdminStatsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = todayStart.AddDays(-7);

        var totalUsers = await _db.Users.CountAsync(ct);
        var activeSubscriptions = await _db.Subscriptions.CountAsync(s => s.ExpiresAt > now, ct);
        var newUsersToday = await _db.Users.CountAsync(u => u.CreatedAt >= todayStart, ct);
        var newUsersThisWeek = await _db.Users.CountAsync(u => u.CreatedAt >= weekStart, ct);
        var totalAttempts = await _db.Attempts.CountAsync(ct);

        var paidOrders = await _db.PaymentOrders.CountAsync(o => o.Status == PaymentOrderStatus.Paid, ct);
        var totalRevenueTiyin = await _db.PaymentOrders
            .Where(o => o.Status == PaymentOrderStatus.Paid)
            .SumAsync(o => (long)o.AmountTiyin, ct);

        return new AdminStatsDto(
            totalUsers,
            activeSubscriptions,
            totalRevenueTiyin / 100m,
            newUsersToday,
            newUsersThisWeek,
            totalAttempts,
            paidOrders);
    }
}
