using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.TogglePlan;

public class TogglePlanHandler : IRequestHandler<TogglePlanCommand>
{
    private readonly IApplicationDbContext _db;

    public TogglePlanHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(TogglePlanCommand request, CancellationToken ct)
    {
        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, ct)
            ?? throw new NotFoundException($"Subscription plan '{request.PlanId}' not found.");

        if (request.IsActive) plan.Activate(); else plan.Deactivate();
        await _db.SaveChangesAsync(ct);
    }
}
