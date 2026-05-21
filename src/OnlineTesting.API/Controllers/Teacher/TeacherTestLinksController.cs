using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Teacher.TestLinks.Commands.CreateTestLink;
using OnlineTesting.Application.Teacher.TestLinks.Commands.DeactivateTestLink;
using OnlineTesting.Application.Teacher.TestLinks.Queries.GetTestLinkResults;
using OnlineTesting.Application.Teacher.TestLinks.Queries.GetTestLinks;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Teacher;

[ApiController]
[Route("teacher/test-links")]
[Authorize(Policy = Roles.Policies.TeacherSubscriptionAccess)]
public class TeacherTestLinksController : ControllerBase
{
    private readonly ISender _sender;
    public TeacherTestLinksController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<List<TestLinkListItemDto>> List(CancellationToken ct)
        => _sender.Send(new GetTestLinksQuery(), ct);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestLinkCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetResults), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateTestLinkCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/results")]
    public Task<TestLinkResultsDto> GetResults(Guid id, CancellationToken ct)
        => _sender.Send(new GetTestLinkResultsQuery(id), ct);
}
