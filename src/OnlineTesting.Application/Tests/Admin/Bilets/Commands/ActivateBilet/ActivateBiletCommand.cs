using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.ActivateBilet;

public record ActivateBiletCommand(Guid Id) : IRequest;