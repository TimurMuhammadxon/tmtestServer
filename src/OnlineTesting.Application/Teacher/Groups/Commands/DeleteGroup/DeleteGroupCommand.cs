using MediatR;

namespace OnlineTesting.Application.Teacher.Groups.Commands.DeleteGroup;

public record DeleteGroupCommand(Guid GroupId) : IRequest;
