using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Progress.Queries.GetAttemptHistory;
using OnlineTesting.Application.Progress.Queries.GetDashboard;
using OnlineTesting.Application.Progress.Queries.GetErrorQuestionDetail;
using OnlineTesting.Application.Progress.Queries.GetErrorsAnalysis;
using OnlineTesting.Application.Progress.Queries.GetTopicsProgress;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.API.Controllers.Public;

[ApiController]
[Route("progress")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly ISender _sender;
    public ProgressController(ISender sender) => _sender = sender;

    [HttpGet("dashboard")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client)]
    public Task<DashboardDto> Dashboard(CancellationToken ct)
        => _sender.Send(new GetDashboardQuery(), ct);

    [HttpGet("topics")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    public Task<List<TopicProgressDto>> Topics(CancellationToken ct)
        => _sender.Send(new GetTopicsProgressQuery(), ct);

    [HttpGet("errors")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    public Task<List<ErrorAnalysisItemDto>> Errors(CancellationToken ct)
        => _sender.Send(new GetErrorsAnalysisQuery(), ct);

    [HttpGet("errors/{questionId:guid}")]
    public Task<ErrorQuestionDetailDto> ErrorDetail(Guid questionId, CancellationToken ct)
        => _sender.Send(new GetErrorQuestionDetailQuery(questionId), ct);

    [HttpGet("history")]
    public Task<PagedResult<AttemptHistoryItemDto>> History(
        [FromQuery] FlowType? flowType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => _sender.Send(new GetAttemptHistoryQuery(flowType, page, pageSize), ct);
}
