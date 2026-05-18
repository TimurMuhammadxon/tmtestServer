using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Application.Teacher.Groups.Commands.JoinGroup;

public class JoinGroupHandler : IRequestHandler<JoinGroupCommand, JoinGroupResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public JoinGroupHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<JoinGroupResult> Handle(JoinGroupCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var group = await _db.Groups
            .FirstOrDefaultAsync(g => g.InviteCode == request.InviteCode.ToUpper() && g.IsActive, ct)
            ?? throw new NotFoundException("Invalid or expired invite code.");

        if (group.TeacherId == userId)
            throw new ConflictException("Teacher cannot join their own group.");

        var alreadyMember = await _db.GroupMembers
            .AnyAsync(m => m.GroupId == group.Id && m.UserId == userId, ct);

        if (alreadyMember)
            throw new ConflictException("You are already a member of this group.");

        _db.GroupMembers.Add(GroupMember.Create(group.Id, userId));
        await _db.SaveChangesAsync(ct);

        return new JoinGroupResult(group.Id, group.Name);
    }
}
