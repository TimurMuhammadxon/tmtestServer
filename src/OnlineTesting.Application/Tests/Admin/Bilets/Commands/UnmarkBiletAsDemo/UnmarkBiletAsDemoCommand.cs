using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.UnmarkBiletAsDemo;

public record UnmarkBiletAsDemoCommand(Guid Id) : IRequest;