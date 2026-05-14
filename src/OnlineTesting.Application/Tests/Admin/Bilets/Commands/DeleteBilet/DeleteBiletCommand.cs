using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.DeleteBilet;

public record DeleteBiletCommand(Guid Id) : IRequest;