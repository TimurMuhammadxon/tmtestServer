using MediatR;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Progress.Queries.GetAttemptHistory;

public record GetAttemptHistoryQuery(
    FlowType? FlowType,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<AttemptHistoryItemDto>>;

public record AttemptHistoryItemDto(
    Guid Id,
    string Flow,
    string Status,
    int? CorrectCount,
    int TotalQuestions,
    DateTime StartedAt,
    DateTime? FinishedAt);
