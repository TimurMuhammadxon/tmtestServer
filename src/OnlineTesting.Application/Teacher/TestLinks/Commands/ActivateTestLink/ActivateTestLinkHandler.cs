using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.ActivateTestLink;

public class ActivateTestLinkHandler : IRequestHandler<ActivateTestLinkCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ActivateTestLinkHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateTestLinkCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = await _db.TestLinks
            .FirstOrDefaultAsync(t => t.Id == request.TestLinkId, ct)
            ?? throw new NotFoundException($"Test link '{request.TestLinkId}' not found.");

        if (link.TeacherId != teacherId)
            throw new NotFoundException($"Test link '{request.TestLinkId}' not found.");

        link.Activate();
        await _db.SaveChangesAsync(ct);
    }
}
