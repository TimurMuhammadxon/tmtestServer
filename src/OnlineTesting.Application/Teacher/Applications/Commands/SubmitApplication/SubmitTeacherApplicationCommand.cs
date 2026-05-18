using MediatR;

namespace OnlineTesting.Application.Teacher.Applications.Commands.SubmitApplication;

public record SubmitTeacherApplicationCommand(
    string FullName,
    string PhoneNumber,
    string? TelegramUsername,
    string? OrganizationName,
    string? ExperienceText,
    string? AdditionalNotes) : IRequest<Guid>;
