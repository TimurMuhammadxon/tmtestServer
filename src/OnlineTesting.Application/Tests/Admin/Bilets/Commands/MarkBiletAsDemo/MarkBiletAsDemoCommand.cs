using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.MarkBiletAsDemo;

public record MarkBiletAsDemoCommand(Guid Id) : IRequest;