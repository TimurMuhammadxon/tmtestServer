using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.UpdateTestLink;

public class UpdateTestLinkHandler : IRequestHandler<UpdateTestLinkCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateTestLinkHandler(IApplicationDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateTestLinkCommand request, CancellationToken ct)
    {
        var teacherId = _currentUser.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var link = await _db.TestLinks
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Test link '{request.Id}' not found.");

        if (link.TeacherId != teacherId)
            throw new NotFoundException($"Test link '{request.Id}' not found.");

        link.Update(request.Title, request.MaxAttempts, request.ExpiresAt);
        await _db.SaveChangesAsync(ct);
    }
}
