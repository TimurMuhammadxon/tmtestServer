using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Tests.Admin.Topics.Commands.CreateTopic;
using OnlineTesting.Application.Tests.Admin.Topics.Commands.DeleteTopic;
using OnlineTesting.Application.Tests.Admin.Topics.Commands.DeleteTopicTranslation;
using OnlineTesting.Application.Tests.Admin.Topics.Commands.SetTopicActive;
using OnlineTesting.Application.Tests.Admin.Topics.Commands.UpdateTopic;
using OnlineTesting.Application.Tests.Admin.Topics.Commands.UpsertTopicTranslation;
using OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicById;
using OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicsList;
using OnlineTesting.Domain.Authorization;

namespace OnlineTesting.API.Controllers.Admin;

[ApiController]
[Route("admin/topics")]
[Authorize(Policy = Roles.Policies.ContentManagement)]
public class AdminTopicsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminTopicsController(ISender sender) => _sender = sender;

    [HttpGet]
    public Task<PagedResult<TopicAdminDto>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeTranslations = false,
        CancellationToken ct = default)
        => _sender.Send(new GetTopicsListQuery(page, pageSize, includeTranslations), ct);

    [HttpGet("{id:guid}")]
    public Task<TopicAdminDto> GetById(Guid id, CancellationToken ct)
        => _sender.Send(new GetTopicByIdQuery(id), ct);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTopicCommand cmd,
        CancellationToken ct)
    {
        var res = await _sender.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTopicBody body,
        CancellationToken ct)
    {
        await _sender.Send(new UpdateTopicCommand(id, body.Code, body.OrderIndex, body.IsDemo), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteTopicCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new SetTopicActiveCommand(id, true), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new SetTopicActiveCommand(id, false), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/translations/{lang}")]
    public async Task<IActionResult> UpsertTranslation(
        Guid id,
        string lang,
        [FromBody] UpsertTopicTranslationBody body,
        CancellationToken ct)
    {
        await _sender.Send(new UpsertTopicTranslationCommand(id, lang, body.Name), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/translations/{lang}")]
    public async Task<IActionResult> DeleteTranslation(Guid id, string lang, CancellationToken ct)
    {
        await _sender.Send(new DeleteTopicTranslationCommand(id, lang), ct);
        return NoContent();
    }

    public record UpdateTopicBody(string Code, int OrderIndex, bool IsDemo);
    public record UpsertTopicTranslationBody(string Name);
}