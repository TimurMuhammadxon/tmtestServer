using MediatR;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.Groups.Commands.CreateGroup;

public class CreateGroupHandler : IRequestHandler<CreateGroupCommand, CreateGroupResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateGroupHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateGroupResult> Handle(CreateGroupCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var group = Domain.Teacher.Group.Create(teacherId, request.Name);

        _db.Groups.Add(group);
        await _db.SaveChangesAsync(ct);

        return new CreateGroupResult(group.Id, group.InviteCode);
    }
}
