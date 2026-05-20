using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Subscriptions.Admin.Commands.SetPlanPrice;
using OnlineTesting.Application.Subscriptions.Admin.Commands.TogglePlan;
using OnlineTesting.Application.Subscriptions.Queries.GetPlans;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/subscription-plans")]
[Authorize(Policy = Roles.Policies.OwnerAccess)]
public class AdminSubscriptionPlansController : ControllerBase
{
    private readonly ISender _sender;
    public AdminSubscriptionPlansController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<List<SubscriptionPlanDto>> GetAll(CancellationToken ct)
        => _sender.Send(new GetPlansQuery(AdminView: true), ct);

    [HttpPatch("{id:guid}/price")]
    public async Task<IActionResult> SetPrice(Guid id, [FromBody] SetPriceBody body, CancellationToken ct)
    {
        await _sender.Send(new SetPlanPriceCommand(id, body.Price), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, [FromBody] ToggleBody body, CancellationToken ct)
    {
        await _sender.Send(new TogglePlanCommand(id, body.IsActive), ct);
        return NoContent();
    }

    public record SetPriceBody(decimal Price);
    public record ToggleBody(bool IsActive);
}
