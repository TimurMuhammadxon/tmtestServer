using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Subscriptions;

namespace OnlineTesting.Application.Payments.Admin.Queries.GetPaymentsList;

public class GetPaymentsListHandler : IRequestHandler<GetPaymentsListQuery, PagedResult<PaymentOrderAdminDto>>
{
    private readonly IApplicationDbContext _db;
    public GetPaymentsListHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<PaymentOrderAdminDto>> Handle(GetPaymentsListQuery request, CancellationToken ct)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var total = await _db.PaymentOrders.CountAsync(ct);

        var rows = await _db.PaymentOrders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(o => new
            {
                o.Id,
                o.UserId,
                o.PlanId,
                o.AmountTiyin,
                o.Status,
                o.CreatedAt,
                UserEmail = _db.Users
                    .Where(u => u.Id == o.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefault() ?? "—",
                Plan = _db.SubscriptionPlans
                    .Where(p => p.Id == o.PlanId)
                    .Select(p => new { p.Type, p.Duration })
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        var items = rows.Select(r => new PaymentOrderAdminDto(
            r.Id,
            r.UserEmail,
            r.Plan is null ? "—" : $"{r.Plan.Type} / {FormatDuration(r.Plan.Duration)}",
            r.AmountTiyin,
            r.Status.ToString(),
            r.CreatedAt)).ToList();

        return new PagedResult<PaymentOrderAdminDto>(items, page, size, total);
    }

    private static string FormatDuration(SubscriptionDuration d) => d switch
    {
        SubscriptionDuration.TwoWeeks    => "2 hafta",
        SubscriptionDuration.OneMonth    => "1 oy",
        SubscriptionDuration.TwoMonths   => "2 oy",
        SubscriptionDuration.ThreeMonths => "3 oy",
        _ => d.ToString(),
    };
}
