using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.SetTopicActive;

public class SetTopicActiveHandler : IRequestHandler<SetTopicActiveCommand>
{
    private readonly IApplicationDbContext _db;
    public SetTopicActiveHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(SetTopicActiveCommand request, CancellationToken ct)
    {
        var topic = await _db.Topics.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Topic '{request.Id}' not found.");

        if (request.IsActive) topic.Activate();
        else topic.Deactivate();

        await _db.SaveChangesAsync(ct);
    }
}