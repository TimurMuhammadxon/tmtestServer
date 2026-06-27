using MediatR;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Teacher;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.CreateTestLink;

public class CreateTestLinkHandler : IRequestHandler<CreateTestLinkCommand, TestLinkDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateTestLinkHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TestLinkDto> Handle(CreateTestLinkCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = TestLink.Create(
            teacherId,
            request.Title,
            request.FlowType,
            request.BiletId,
            request.TopicIds,
            request.QuestionCount,
            request.GroupId,
            request.MaxAttempts,
            request.ExpiresAt,
            request.ShowExplanations);

        _db.TestLinks.Add(link);
        await _db.SaveChangesAsync(ct);

        return new TestLinkDto(
            link.Id, link.Title, link.Code, link.FlowType.ToString(),
            link.BiletId, link.TopicIds, link.QuestionCount,
            link.GroupId, link.MaxAttempts, link.ExpiresAt,
            link.IsActive, link.ShowExplanations, link.CreatedAt);
    }
}
