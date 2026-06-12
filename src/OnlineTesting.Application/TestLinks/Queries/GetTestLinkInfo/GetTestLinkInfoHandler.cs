using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.TestLinks.Queries.GetTestLinkInfo;

public class GetTestLinkInfoHandler : IRequestHandler<GetTestLinkInfoQuery, TestLinkInfoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTestLinkInfoHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TestLinkInfoDto> Handle(GetTestLinkInfoQuery request, CancellationToken ct)
    {
        var link = await _db.TestLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == request.Code.ToUpper() && t.IsActive, ct)
            ?? throw new NotFoundException("Invalid or expired link.");

        if (link.ExpiresAt <= DateTime.UtcNow)
            throw new ConflictException("This link has expired.");

        var userId = _currentUser.UserId;
        var attemptsUsed = userId.HasValue
            ? await _db.Attempts.CountAsync(a => a.TestLinkId == link.Id && a.UserId == userId.Value, ct)
            : 0;

        return new TestLinkInfoDto(
            link.Title,
            link.FlowType.ToString(),
            link.MaxAttempts,
            link.ExpiresAt,
            attemptsUsed,
            link.IsActive);
    }
}
