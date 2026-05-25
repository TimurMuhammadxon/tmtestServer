using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Subscriptions.Admin.Commands.GrantSubscription;
using OnlineTesting.Application.Users.Admin.Queries.GetUsersList;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/users")]
[Authorize(Policy = Roles.Policies.OwnerAccess)]
public class AdminUsersController : ControllerBase
{
    private readonly ISender _sender;
    public AdminUsersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetUsersListQuery(search, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{userId:guid}/subscription")]
    public async Task<IActionResult> Grant(Guid userId, [FromBody] GrantBody body, CancellationToken ct)
    {
        var result = await _sender.Send(new GrantSubscriptionCommand(userId, body.PlanId), ct);
        return Ok(result);
    }

    public record GrantBody(Guid PlanId);
}
