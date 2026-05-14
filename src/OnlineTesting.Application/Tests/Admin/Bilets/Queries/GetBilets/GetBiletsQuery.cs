using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Queries.GetBilets;

public record GetBiletsQuery(
    bool? IsActive,
    bool? IsDemo,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BiletListItemDto>>;