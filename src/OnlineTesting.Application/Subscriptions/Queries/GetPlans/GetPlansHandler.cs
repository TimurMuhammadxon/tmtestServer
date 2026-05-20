using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Subscriptions.Queries.GetPlans;

public class GetPlansHandler : IRequestHandler<GetPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPlansHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<SubscriptionPlanDto>> Handle(GetPlansQuery request, CancellationToken ct)
    {
        var query = _db.SubscriptionPlans.AsNoTracking();

        if (!request.AdminView)
            query = query.Where(p => p.IsActive);

        return await query
            .OrderBy(p => p.Type)
            .ThenBy(p => p.Duration)
            .Select(p => new SubscriptionPlanDto(
                p.Id, p.Type.ToString(), p.Duration.ToString(), p.Price, p.IsActive))
            .ToListAsync(ct);
    }
}
