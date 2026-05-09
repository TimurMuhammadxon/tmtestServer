using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.DeleteTopic;

public class DeleteTopicHandler : IRequestHandler<DeleteTopicCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteTopicHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteTopicCommand request, CancellationToken ct)
    {
        var topic = await _db.Topics.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Topic '{request.Id}' not found.");

        var hasQuestions = await _db.Questions.AnyAsync(q => q.TopicId == topic.Id, ct);
        if (hasQuestions)
            throw new ConflictException("Cannot delete topic that has questions. Deactivate it instead.");

        _db.Topics.Remove(topic);
        await _db.SaveChangesAsync(ct);
    }
}