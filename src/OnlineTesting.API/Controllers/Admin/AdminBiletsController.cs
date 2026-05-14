using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Tests.Admin.Bilets.Commands.ActivateBilet;
using OnlineTesting.Application.Tests.Admin.Bilets.Commands.CreateBilet;
using OnlineTesting.Application.Tests.Admin.Bilets.Commands.DeactivateBilet;
using OnlineTesting.Application.Tests.Admin.Bilets.Commands.DeleteBilet;
using OnlineTesting.Application.Tests.Admin.Bilets.Commands.MarkBiletAsDemo;
using OnlineTesting.Application.Tests.Admin.Bilets.Commands.UnmarkBiletAsDemo;
using OnlineTesting.Application.Tests.Admin.Bilets.Commands.UpdateBilet;
using OnlineTesting.Application.Tests.Admin.Bilets.Queries;
using OnlineTesting.Application.Tests.Admin.Bilets.Queries.GetBiletById;
using OnlineTesting.Application.Tests.Admin.Bilets.Queries.GetBilets;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/bilets")]
[Authorize(Policy = "ContentManagement")]
public class AdminBiletsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminBiletsController(IMediator mediator) => _mediator = mediator;

    public record CreateBiletRequest(int Number, IReadOnlyList<Guid> QuestionIds, bool IsDemo);
    public record UpdateBiletRequest(IReadOnlyList<Guid> QuestionIds);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBiletRequest body, CancellationToken ct)
    {
        var id = await _mediator.Send(
            new CreateBiletCommand(body.Number, body.QuestionIds, body.IsDemo), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBiletRequest body, CancellationToken ct)
    {
        await _mediator.Send(new UpdateBiletCommand(id, body.QuestionIds), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteBiletCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateBiletCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateBiletCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/mark-demo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkDemo(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new MarkBiletAsDemoCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/unmark-demo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnmarkDemo(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new UnmarkBiletAsDemoCommand(id), ct);
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BiletListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] bool? isActive,
        [FromQuery] bool? isDemo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetBiletsQuery(isActive, isDemo, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BiletDetailsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBiletByIdQuery(id), ct);
        return Ok(result);
    }
}