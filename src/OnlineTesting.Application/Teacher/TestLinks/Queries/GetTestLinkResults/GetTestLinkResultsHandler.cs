using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.TestLinks.Queries.GetTestLinkResults;

public class GetTestLinkResultsHandler : IRequestHandler<GetTestLinkResultsQuery, TestLinkResultsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTestLinkResultsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TestLinkResultsDto> Handle(GetTestLinkResultsQuery request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = await _db.TestLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TestLinkId, ct)
            ?? throw new NotFoundException($"Test link '{request.TestLinkId}' not found.");

        if (link.TeacherId != teacherId)
            throw new NotFoundException($"Test link '{request.TestLinkId}' not found.");

        var results = await _db.Attempts
            .AsNoTracking()
            .Where(a => a.TestLinkId == request.TestLinkId)
            .OrderByDescending(a => a.StartedAt)
            .Join(_db.Users, a => a.UserId, u => u.Id, (a, u) => new { a, u.FirstName, u.LastName })
            .Select(x => new
            {
                x.a.Id,
                x.a.UserId,
                x.FirstName,
                x.LastName,
                x.a.Status,
                x.a.CorrectCount,
                x.a.StartedAt,
                x.a.FinishedAt,
                TotalQuestions = _db.AttemptQuestions.Count(aq => aq.AttemptId == x.a.Id)
            })
            .ToListAsync(ct);

        var resultDtos = results
            .Select(r => new TestLinkResultItemDto(
                r.UserId, r.FirstName, r.LastName, r.Id,
                r.StartedAt, r.FinishedAt,
                r.CorrectCount, r.TotalQuestions,
                r.Status.ToString()))
            .ToList();

        return new TestLinkResultsDto(link.Id, link.Title, resultDtos);
    }
}
