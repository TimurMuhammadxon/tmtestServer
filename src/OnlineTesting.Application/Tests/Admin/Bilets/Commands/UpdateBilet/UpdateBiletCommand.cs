using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.UpdateBilet;

public record UpdateBiletCommand(
    Guid Id,
    IReadOnlyList<Guid> QuestionIds
) : IRequest;