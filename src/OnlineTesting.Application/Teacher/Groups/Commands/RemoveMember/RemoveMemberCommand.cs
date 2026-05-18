using MediatR;

namespace OnlineTesting.Application.Teacher.Groups.Commands.RemoveMember;

public record RemoveMemberCommand(Guid GroupId, Guid UserId) : IRequest;
