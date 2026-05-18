using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.Groups.Queries.GetGroups;

public class GetGroupsHandler : IRequestHandler<GetGroupsQuery, List<GroupDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetGroupsHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<GroupDto>> Handle(GetGroupsQuery request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        return await _db.Groups
            .AsNoTracking()
            .Where(g => g.TeacherId == teacherId)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GroupDto(
                g.Id,
                g.Name,
                g.InviteCode,
                g.IsActive,
                _db.GroupMembers.Count(m => m.GroupId == g.Id),
                g.CreatedAt))
            .ToListAsync(ct);
    }
}
