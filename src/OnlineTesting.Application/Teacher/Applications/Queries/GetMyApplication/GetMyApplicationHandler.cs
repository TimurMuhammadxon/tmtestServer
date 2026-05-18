using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.Applications.Queries.GetMyApplication;

public class GetMyApplicationHandler : IRequestHandler<GetMyApplicationQuery, TeacherApplicationDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMyApplicationHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TeacherApplicationDto?> Handle(GetMyApplicationQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var application = await _db.TeacherApplications
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.SubmittedAt)
            .FirstOrDefaultAsync(ct);

        if (application is null) return null;

        return new TeacherApplicationDto(
            application.Id,
            application.FullName,
            application.PhoneNumber,
            application.TelegramUsername,
            application.OrganizationName,
            application.ExperienceText,
            application.AdditionalNotes,
            application.Status.ToString(),
            application.SubmittedAt,
            application.ReviewedAt,
            application.RejectionReason);
    }
}
