using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Teacher.Groups.Queries.GetGroupMemberStats;

public class GetGroupMemberStatsHandler : IRequestHandler<GetGroupMemberStatsQuery, IReadOnlyList<GroupMemberStatsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetGroupMemberStatsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GroupMemberStatsDto>> Handle(GetGroupMemberStatsQuery request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("Not authenticated.");

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (group.TeacherId != teacherId)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var members = await _db.GroupMembers
            .Where(gm => gm.GroupId == request.GroupId)
            .Join(_db.Users, gm => gm.UserId, u => u.Id,
                (gm, u) => new { gm.UserId, u.Email, u.FirstName, u.LastName, gm.JoinedAt })
            .OrderBy(m => m.JoinedAt)
            .ToListAsync(ct);

        // Все попытки участников через тест-ссылки этой группы
        var groupTestLinkIds = await _db.TestLinks
            .Where(tl => tl.GroupId == request.GroupId)
            .Select(tl => tl.Id)
            .ToListAsync(ct);

        var memberIds = members.Select(m => m.UserId).ToList();

        var attempts = groupTestLinkIds.Count > 0
            ? await _db.Attempts
                .Where(a => memberIds.Contains(a.UserId)
                    && a.TestLinkId.HasValue
                    && groupTestLinkIds.Contains(a.TestLinkId!.Value)
                    && a.Status != AttemptStatus.InProgress)
                .Select(a => new
                {
                    a.UserId,
                    a.Status,
                    a.CorrectCount,
                    QuestionCount = _db.AttemptQuestions.Count(aq => aq.AttemptId == a.Id)
                })
                .ToListAsync(ct)
            : new List<object>().Select(_ => new
            {
                UserId = Guid.Empty,
                Status = AttemptStatus.Completed,
                CorrectCount = (int?)0,
                QuestionCount = 0
            }).ToList();

        var attemptsByUser = attempts
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return members.Select(m =>
        {
            var userAttempts = attemptsByUser.TryGetValue(m.UserId, out var list) ? list : new();
            var total = userAttempts.Count;
            var passed = userAttempts.Count(a => a.Status == AttemptStatus.Passed);
            int? avgAcc = null;
            if (total > 0)
            {
                var totalQ = userAttempts.Sum(a => a.QuestionCount);
                var totalC = userAttempts.Sum(a => a.CorrectCount ?? 0);
                avgAcc = totalQ > 0 ? (int)Math.Round(totalC * 100.0 / totalQ) : 0;
            }

            var displayName = string.IsNullOrWhiteSpace(m.FirstName)
                ? null
                : $"{m.FirstName} {m.LastName}".Trim();

            return new GroupMemberStatsDto(m.UserId, m.Email, displayName, m.JoinedAt, total, passed, avgAcc);
        }).ToList();
    }
}
