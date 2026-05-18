using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.Groups.Commands.RemoveMember;

public class RemoveMemberHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RemoveMemberHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var group = await _db.Groups
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (group.TeacherId != teacherId)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        var member = await _db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == request.GroupId && m.UserId == request.UserId, ct)
            ?? throw new NotFoundException($"Member '{request.UserId}' not found in group.");

        _db.GroupMembers.Remove(member);
        await _db.SaveChangesAsync(ct);
    }
}
