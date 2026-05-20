using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.SetPlanPrice;

public class SetPlanPriceHandler : IRequestHandler<SetPlanPriceCommand>
{
    private readonly IApplicationDbContext _db;

    public SetPlanPriceHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetPlanPriceCommand request, CancellationToken ct)
    {
        var plan = await _db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, ct)
            ?? throw new NotFoundException($"Subscription plan '{request.PlanId}' not found.");

        plan.SetPrice(request.Price);
        await _db.SaveChangesAsync(ct);
    }
}
