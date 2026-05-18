using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Teacher.Groups.Commands.JoinGroup;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("groups")]
[Authorize]
public class PublicGroupsController : ControllerBase
{
    private readonly ISender _sender;
    public PublicGroupsController(ISender sender) => _sender = sender;

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinGroupCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return Ok(result);
    }
}
