using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Application.Teacher.Applications.Admin.Commands.RejectApplication;

public class RejectTeacherApplicationHandler : IRequestHandler<RejectTeacherApplicationCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RejectTeacherApplicationHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RejectTeacherApplicationCommand request, CancellationToken ct)
    {
        var reviewerId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var application = await _db.TeacherApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct)
            ?? throw new NotFoundException($"Application '{request.ApplicationId}' not found.");

        if (application.Status != TeacherApplicationStatus.Pending)
            throw new ConflictException($"Application is already {application.Status}.");

        application.Reject(reviewerId, request.Reason);
        await _db.SaveChangesAsync(ct);
    }
}
