using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Teacher;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Teacher.Applications.Admin.Commands.ApproveApplication;

public class ApproveTeacherApplicationHandler : IRequestHandler<ApproveTeacherApplicationCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ApproveTeacherApplicationHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveTeacherApplicationCommand request, CancellationToken ct)
    {
        var reviewerId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var application = await _db.TeacherApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct)
            ?? throw new NotFoundException($"Application '{request.ApplicationId}' not found.");

        if (application.Status != TeacherApplicationStatus.Pending)
            throw new ConflictException($"Application is already {application.Status}.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == application.UserId, ct)
            ?? throw new NotFoundException($"User '{application.UserId}' not found.");

        application.Approve(reviewerId);
        user.SetRole(Role.Teacher);

        await _db.SaveChangesAsync(ct);
    }
}
