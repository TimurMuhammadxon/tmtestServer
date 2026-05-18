using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.Groups.Commands.DeleteGroup;

public class DeleteGroupHandler : IRequestHandler<DeleteGroupCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteGroupHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteGroupCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var group = await _db.Groups
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, ct)
            ?? throw new NotFoundException($"Group '{request.GroupId}' not found.");

        if (group.TeacherId != teacherId)
            throw new NotFoundException($"Group '{request.GroupId}' not found.");

        _db.Groups.Remove(group);
        await _db.SaveChangesAsync(ct);
    }
}
