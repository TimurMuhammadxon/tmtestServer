using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.DeactivateTestLink;

public class DeactivateTestLinkHandler : IRequestHandler<DeactivateTestLinkCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeactivateTestLinkHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateTestLinkCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = await _db.TestLinks
            .FirstOrDefaultAsync(t => t.Id == request.TestLinkId, ct)
            ?? throw new NotFoundException($"Test link '{request.TestLinkId}' not found.");

        if (link.TeacherId != teacherId)
            throw new NotFoundException($"Test link '{request.TestLinkId}' not found.");

        link.Deactivate();
        await _db.SaveChangesAsync(ct);
    }
}
