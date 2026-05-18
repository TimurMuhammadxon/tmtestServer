using MediatR;

namespace OnlineTesting.Application.Teacher.Groups.Queries.GetGroups;

public record GetGroupsQuery : IRequest<List<GroupDto>>;

public record GroupDto(
    Guid Id,
    string Name,
    string InviteCode,
    bool IsActive,
    int MemberCount,
    DateTime CreatedAt);
