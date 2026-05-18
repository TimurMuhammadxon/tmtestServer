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

        var userRole = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => (Role?)u.Role)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"User '{userId}' not found.");

        if (userRole == Role.Teacher || userRole == Role.Admin
            || userRole == Role.SuperAdmin || userRole == Role.Owner)
            throw new ConflictException("You already have teacher or higher privileges.");

        var application = TeacherApplication.Submit(
            userId,
            request.FullName,
            request.PhoneNumber,
            request.TelegramUsername,
            request.OrganizationName,
            request.ExperienceText,
            request.AdditionalNotes);

        try
        {
            _db.TeacherApplications.Add(application);
            await _db.SaveChangesAsync(ct);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new ConflictException("You already have a pending application.");
        }

        return application.Id;
    }
}
