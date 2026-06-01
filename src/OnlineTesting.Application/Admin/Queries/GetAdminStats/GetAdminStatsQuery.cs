using MediatR;

namespace OnlineTesting.Application.Admin.Queries.GetAdminStats;

public record GetAdminStatsQuery : IRequest<AdminStatsDto>;

public record AdminStatsDto(
    int TotalUsers,
    int ActiveSubscriptions,
    decimal TotalRevenueSom,
    int NewUsersToday,
    int NewUsersThisWeek,
    int TotalAttempts,
    int PaidOrders);
