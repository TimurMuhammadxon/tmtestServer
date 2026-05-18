using MediatR;

namespace OnlineTesting.Application.Teacher.Applications.Queries.GetMyApplication;

public record GetMyApplicationQuery : IRequest<TeacherApplicationDto?>;

public record TeacherApplicationDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string? TelegramUsername,
    string? OrganizationName,
    string? ExperienceText,
    string? AdditionalNotes,
    string Status,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? RejectionReason);
