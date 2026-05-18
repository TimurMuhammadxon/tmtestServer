using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Progress.Queries.GetAttemptHistory;

public class GetAttemptHistoryHandler : IRequestHandler<GetAttemptHistoryQuery, PagedResult<AttemptHistoryItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetAttemptHistoryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<AttemptHistoryItemDto>> Handle(GetAttemptHistoryQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.Attempts.Where(a => a.UserId == userId);

        if (request.FlowType.HasValue)
            query = query.Where(a => a.Flow == request.FlowType.Value);

        var total = await query.CountAsync(ct);

        var rawItems = await query
            .OrderByDescending(a => a.StartedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new
            {
                a.Id, a.Flow, a.Status, a.CorrectCount, a.StartedAt, a.FinishedAt,
                TotalQuestions = _db.AttemptQuestions.Count(aq => aq.AttemptId == a.Id)
            })
            .ToListAsync(ct);

        var items = rawItems
            .Select(a => new AttemptHistoryItemDto(
                a.Id, a.Flow.ToString(), a.Status.ToString(),
                a.CorrectCount, a.TotalQuestions, a.StartedAt, a.FinishedAt))
            .ToList();

        return new PagedResult<AttemptHistoryItemDto>(items, page, size, total);
    }
}
