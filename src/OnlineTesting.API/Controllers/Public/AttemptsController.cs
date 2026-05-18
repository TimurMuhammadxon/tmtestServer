using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Tests.Solutions.Attempts.Commands.FinishAttempt;
using OnlineTesting.Application.Tests.Solutions.Attempts.Commands.StartAttempt;
using OnlineTesting.Application.Tests.Solutions.Attempts.Commands.SubmitAnswer;
using OnlineTesting.Application.Tests.Solutions.Attempts.Queries;
using OnlineTesting.Application.Tests.Solutions.Attempts.Queries.GetAttempt;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("attempts")]
[Authorize]
public class AttemptsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AttemptsController(IMediator mediator) => _mediator = mediator;

    public record StartAttemptRequest(
        FlowType FlowType,
        Guid? BiletId,
        IReadOnlyList<Guid>? TopicIds,
        int? QuestionCount);

    public record SubmitAnswerRequest(Guid QuestionId, Guid AnswerId);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Start([FromBody] StartAttemptRequest body, CancellationToken ct)
    {
        var id = await _mediator.Send(
            new StartAttemptCommand(body.FlowType, body.BiletId, body.TopicIds, body.QuestionCount), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AttemptDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAttemptQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/answer")]
    [ProducesResponseType(typeof(SubmitAnswerResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Answer(Guid id, [FromBody] SubmitAnswerRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitAnswerCommand(id, body.QuestionId, body.AnswerId), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/finish")]
    [ProducesResponseType(typeof(FinishAttemptResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Finish(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new FinishAttemptCommand(id), ct);
        return Ok(result);
    }
}
