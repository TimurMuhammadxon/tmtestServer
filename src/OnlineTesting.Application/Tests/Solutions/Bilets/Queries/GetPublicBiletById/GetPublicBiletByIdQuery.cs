using MediatR;
using OnlineTesting.Application.Tests.Solutions.Bilets.Queries;

namespace OnlineTesting.Application.Tests.Solutions.Bilets.Queries.GetPublicBiletById;

public record GetPublicBiletByIdQuery(Guid Id) : IRequest<PublicBiletDetailsDto>;