using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Tests.Admin.Questions.Commands.CreateQuestion;
using OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestion;
using OnlineTesting.Application.Tests.Admin.Questions.Commands.DeleteQuestionTranslation;
using OnlineTesting.Application.Tests.Admin.Questions.Commands.SetQuestionActive;
using OnlineTesting.Application.Tests.Admin.Questions.Commands.UpdateQuestion;
using OnlineTesting.Application.Tests.Admin.Questions.Commands.UpsertQuestionTranslation;
using OnlineTesting.Application.Tests.Admin.Questions.Queries.GetQuestionById;
using OnlineTesting.Application.Tests.Admin.Questions.Queries.GetQuestionsList;
using OnlineTesting.Application.Tests.Common;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/questions")]
[Authorize(Policy = Roles.Policies.ContentManagement)]
public class AdminQuestionsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminQuestionsController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<PagedResult<QuestionAdminListItemDto>> List(
        [FromQuery] Guid? topicId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _sender.Send(new GetQuestionsListQuery(topicId, search, page, pageSize), ct);

    [HttpGet("{id:guid}")]
    public Task<QuestionAdminDto> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetQuestionByIdQuery(id), ct);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQuestionCommand cmd,
        CancellationToken ct)
    {
        var res = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateQuestionBody body,
        CancellationToken ct)
    {
        await _sender.Send(new UpdateQuestionCommand(id, body.TopicId, body.ImageKey, body.Answers), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteQuestionCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new SetQuestionActiveCommand(id, true), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new SetQuestionActiveCommand(id, false), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/translations/{lang}")]
    public async Task<IActionResult> UpsertTranslation(
        Guid id,
        string lang,
        [FromBody] UpsertQuestionTranslationBody body,
        CancellationToken ct)
    {
        await _sender.Send(
            new UpsertQuestionTranslationCommand(id, lang, body.Text, body.Explanation), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/translations/{lang}")]
    public async Task<IActionResult> DeleteTranslation(Guid id, string lang, CancellationToken ct)
    {
        await _sender.Send(new DeleteQuestionTranslationCommand(id, lang), ct);
        return NoContent();
    }

    public record UpdateQuestionBody(Guid TopicId, string? ImageKey, List<AnswerInputDto> Answers);
    public record UpsertQuestionTranslationBody(string Text, string? Explanation);
}