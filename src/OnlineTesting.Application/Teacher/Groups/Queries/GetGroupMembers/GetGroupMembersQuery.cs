using MediatR;

namespace OnlineTesting.Application.Teacher.Groups.Queries.GetGroupMembers;

public record GetGroupMembersQuery(Guid GroupId) : IRequest<List<GroupMemberDto>>;

public record GroupMemberDto(
    Guid UserId,
    string Email,
    DateTime JoinedAt);
