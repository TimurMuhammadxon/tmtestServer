using MediatR;

namespace OnlineTesting.Application.Tests.Solutions.Queries.GetTopicsForStudent;

public record GetTopicsForStudentQuery(bool GuestMode) : IRequest<List<TopicStudentDto>>;

public record TopicStudentDto(
    Guid Id,
    string Code,
    int OrderIndex,
    bool IsDemo,
    string Name,
    string Language,
    bool IsFallback);