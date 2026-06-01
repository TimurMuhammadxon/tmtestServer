using MediatR;

namespace OnlineTesting.Application.Teacher.Groups.Queries.GetGroupMemberStats;

public record GetGroupMemberStatsQuery(Guid GroupId) : IRequest<IReadOnlyList<GroupMemberStatsDto>>;

public record GroupMemberStatsDto(
    Guid UserId,
    string Email,
    string? DisplayName,
    DateTime JoinedAt,
    int TotalAttempts,
    int PassedAttempts,
    int? AvgAccuracyPercent);
