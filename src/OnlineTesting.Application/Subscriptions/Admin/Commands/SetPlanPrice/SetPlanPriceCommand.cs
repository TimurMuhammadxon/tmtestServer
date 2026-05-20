using MediatR;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.SetPlanPrice;

public record SetPlanPriceCommand(Guid PlanId, decimal Price) : IRequest;
