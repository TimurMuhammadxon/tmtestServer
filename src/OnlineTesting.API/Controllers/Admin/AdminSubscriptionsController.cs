using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Subscriptions.Admin.Commands.GrantSubscription;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/users")]
[Authorize(Policy = Roles.Policies.OwnerAccess)]
public class AdminSubscriptionsController : ControllerBase
{
    private readonly ISender _sender;
    public AdminSubscriptionsController(ISender sender) => _sender = sender;

    [HttpPost("{userId:guid}/subscription")]
    public async Task<IActionResult> Grant(Guid userId, [FromBody] GrantBody body, CancellationToken ct)
    {
        var result = await _sender.Send(new GrantSubscriptionCommand(userId, body.PlanId), ct);
        return Ok(result);
    }

    public record GrantBody(Guid PlanId);
}
