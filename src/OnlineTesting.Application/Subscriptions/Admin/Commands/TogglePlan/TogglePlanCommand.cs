using MediatR;

namespace OnlineTesting.Application.Subscriptions.Admin.Commands.TogglePlan;

public record TogglePlanCommand(Guid PlanId, bool IsActive) : IRequest;
