using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Bilets.Commands.CreateBilet;

public record CreateBiletCommand(
    int Number,
    IReadOnlyList<Guid> QuestionIds,
    bool IsDemo
) : IRequest<Guid>;