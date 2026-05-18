using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Teacher.Applications.Commands.SubmitApplication;
using OnlineTesting.Application.Teacher.Applications.Queries.GetMyApplication;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("teacher-applications")]
[Authorize]
public class TeacherApplicationsController : ControllerBase
{
    private readonly ISender _sender;
    public TeacherApplicationsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitTeacherApplicationCommand cmd, CancellationToken ct)
    {
        var id = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetMy), new { }, new { id });
    }

    [HttpGet("my")]
    public Task<TeacherApplicationDto?> GetMy(CancellationToken ct)
        => _sender.Send(new GetMyApplicationQuery(), ct);
}
