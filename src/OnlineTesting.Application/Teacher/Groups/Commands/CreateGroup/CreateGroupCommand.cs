using MediatR;

namespace OnlineTesting.Application.Teacher.Groups.Commands.CreateGroup;

public record CreateGroupCommand(string Name) : IRequest<CreateGroupResult>;

public record CreateGroupResult(Guid Id, string InviteCode);
