using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.DeactivateBilet;

public record DeactivateBiletCommand(Guid Id) : IRequest;