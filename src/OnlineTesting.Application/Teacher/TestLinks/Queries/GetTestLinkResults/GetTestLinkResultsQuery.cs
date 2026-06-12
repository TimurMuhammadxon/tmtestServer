using MediatR;

namespace OnlineTesting.Application.Teacher.TestLinks.Queries.GetTestLinkResults;

public record GetTestLinkResultsQuery(Guid TestLinkId) : IRequest<TestLinkResultsDto>;

public record TestLinkResultsDto(
    Guid TestLinkId,
    string Title,
    List<TestLinkResultItemDto> Results
);

public record TestLinkResultItemDto(
    Guid UserId,
    string? FirstName,
    string? LastName,
    Guid AttemptId,
    DateTime StartedAt,
    DateTime? FinishedAt,
    int? CorrectCount,
    int TotalQuestions,
    string Status
);
