using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.DeleteTestLink;

public class DeleteTestLinkHandler : IRequestHandler<DeleteTestLinkCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteTestLinkHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteTestLinkCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = await _db.TestLinks
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Test link '{request.Id}' not found.");

        if (link.TeacherId != teacherId)
            throw new UnauthorizedException("Access denied.");

        _db.TestLinks.Remove(link);
        await _db.SaveChangesAsync(ct);
    }
}
