using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Admin.Queries.GetAdminStats;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/stats")]
[Authorize(Policy = Roles.Policies.ContentManagement)]
[Produces("application/json")]
public class AdminStatsController : ControllerBase
{
    private readonly ISender _mediator;
    public AdminStatsController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdminStatsQuery(), ct);
        return Ok(result);
    }
}
