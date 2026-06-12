using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Teacher.TestLinks.Commands.ActivateTestLink;
using OnlineTesting.Application.Teacher.TestLinks.Commands.CreateTestLink;
using OnlineTesting.Application.Teacher.TestLinks.Commands.DeactivateTestLink;
using OnlineTesting.Application.Teacher.TestLinks.Commands.DeleteTestLink;
using OnlineTesting.Application.Teacher.TestLinks.Commands.UpdateTestLink;
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
    public Task<PagedResult<TestLinkListItemDto>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _sender.Send(new GetTestLinksQuery(page, pageSize), ct);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestLinkCommand cmd, CancellationToken ct)
    {
        var result = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetResults), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestLinkCommand cmd, CancellationToken ct)
    {
        await _sender.Send(cmd with { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new ActivateTestLinkCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivateTestLinkCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteTestLinkCommand(id), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/results")]
    public Task<TestLinkResultsDto> GetResults(Guid id, CancellationToken ct)
        => _sender.Send(new GetTestLinkResultsQuery(id), ct);
}
