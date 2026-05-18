using MediatR;

namespace OnlineTesting.Application.Teacher.Applications.Admin.Commands.ApproveApplication;

public record ApproveTeacherApplicationCommand(Guid ApplicationId) : IRequest;
