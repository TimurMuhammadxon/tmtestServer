using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.TestLinks.Queries.GetTestLinks;

public class GetTestLinksHandler : IRequestHandler<GetTestLinksQuery, List<TestLinkListItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTestLinksHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<TestLinkListItemDto>> Handle(GetTestLinksQuery request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var links = await _db.TestLinks
            .AsNoTracking()
            .Where(t => t.TeacherId == teacherId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var linkIds = links.Select(t => t.Id).ToList();

        var attemptCounts = await _db.Attempts
            .Where(a => a.TestLinkId != null && linkIds.Contains(a.TestLinkId.Value))
            .GroupBy(a => a.TestLinkId!.Value)
            .Select(g => new { LinkId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LinkId, x => x.Count, ct);

        return links.Select(t => new TestLinkListItemDto(
            t.Id, t.Title, t.Code, t.FlowType.ToString(),
            t.GroupId, t.MaxAttempts, t.ExpiresAt, t.IsActive, t.CreatedAt,
            attemptCounts.GetValueOrDefault(t.Id, 0)
        )).ToList();
    }
}
