using MediatR;
using OnlineTesting.Application.Tests.Admin.Bilets.Queries;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Queries.GetBiletById;

public record GetBiletByIdQuery(Guid Id) : IRequest<BiletDetailsDto>;