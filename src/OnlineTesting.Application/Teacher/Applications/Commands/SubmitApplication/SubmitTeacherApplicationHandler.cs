using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Teacher;
using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Teacher.Applications.Commands.SubmitApplication;

public class SubmitTeacherApplicationHandler : IRequestHandler<SubmitTeacherApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SubmitTeacherApplicationHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(SubmitTeacherApplicationCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException($"User '{userId}' not found.");

        if (user.Role == Role.Teacher || user.Role == Role.Admin
            || user.Role == Role.SuperAdmin || user.Role == Role.Owner)
            throw new ConflictException("You already have teacher or higher privileges.");

        var existing = await _db.TeacherApplications
            .FirstOrDefaultAsync(a => a.UserId == userId
                && a.Status == TeacherApplicationStatus.Pending, ct);

        if (existing is not null)
            throw new ConflictException("You already have a pending application.");

        var application = TeacherApplication.Submit(
            userId,
            request.FullName,
            request.PhoneNumber,
            request.TelegramUsername,
            request.OrganizationName,
            request.ExperienceText,
            request.AdditionalNotes);

        _db.TeacherApplications.Add(application);
        await _db.SaveChangesAsync(ct);

        return application.Id;
    }
}
