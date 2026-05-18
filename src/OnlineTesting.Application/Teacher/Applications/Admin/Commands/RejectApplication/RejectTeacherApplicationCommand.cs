using MediatR;

namespace OnlineTesting.Application.Teacher.Applications.Admin.Commands.RejectApplication;

public record RejectTeacherApplicationCommand(Guid ApplicationId, string? Reason) : IRequest;
