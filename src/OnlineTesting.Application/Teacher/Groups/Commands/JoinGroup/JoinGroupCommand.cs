using MediatR;

namespace OnlineTesting.Application.Teacher.Groups.Commands.JoinGroup;

public record JoinGroupCommand(string InviteCode) : IRequest<JoinGroupResult>;

public record JoinGroupResult(Guid GroupId, string GroupName);
