using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.Groups.Queries.GetGroupMembers;

public class GetGroupMembersHandler : IRequestHandler<GetGroupMembersQuery, List<GroupMemberDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetGroupMembersHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<GroupMemberDto>> Handle(GetGroupMembersQuery request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var group = await _db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (group.TeacherId != teacherId)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var rows = await _db.GroupMembers
            .AsNoTracking()
            .Where(m => m.GroupId == request.GroupId)
            .OrderBy(m => m.JoinedAt)
            .Join(_db.Users, m => m.UserId, u => u.Id, (m, u) => new { m.UserId, u.Email, m.JoinedAt })
            .ToListAsync(ct);

        return rows.Select(r => new GroupMemberDto(r.UserId, r.Email, r.JoinedAt)).ToList();
    }
}
