using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Teacher.Applications.Commands.SubmitApplication;
using OnlineTesting.Application.Teacher.Applications.Queries.GetMyApplication;
using OnlineTesting.Application.Teacher.Groups.Commands.JoinGroup;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Authorize]
public class TeacherApplicationsController : ControllerBase
{
    private readonly ISender _sender;
    public TeacherApplicationsController(ISender sender) => _sender = sender;

    [HttpPost("teacher-applications")]
    public async Task<IActionResult> Submit([FromBody] SubmitTeacherApplicationCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetMy), new { }, new { id });
    }

    [HttpGet("teacher-applications/my")]
    public Task<TeacherApplicationDto?> GetMy(CancellationToken ct)
        => _sender.Send(new GetMyApplicationQuery(), ct);

    [HttpPost("groups/join")]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupBody body, CancellationToken ct)
    {
        var result = await _sender.Send(new JoinGroupCommand(body.InviteCode), ct);
        return Ok(result);
    }

    public record JoinGroupBody(string InviteCode);
}
