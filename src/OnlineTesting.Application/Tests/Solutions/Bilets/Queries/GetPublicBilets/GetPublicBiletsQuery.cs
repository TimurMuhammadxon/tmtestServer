using MediatR;
using OnlineTesting.Application.Tests.Solutions.Bilets.Queries;

namespace OnlineTesting.Application.Tests.Solutions.Bilets.Queries.GetPublicBilets;

public record GetPublicBiletsQuery() : IRequest<IReadOnlyList<PublicBiletListItemDto>>;