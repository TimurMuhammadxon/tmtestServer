using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.DeleteTopicTranslation;

public class DeleteTopicTranslationHandler : IRequestHandler<DeleteTopicTranslationCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteTopicTranslationHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteTopicTranslationCommand request, CancellationToken ct)
    {
        var topic = await _db.Topics
            .Include(t => t.Translations)
            .FirstOrDefaultAsync(t => t.Id == request.TopicId, ct)
            ?? throw new NotFoundException($"Topic '{request.TopicId}' not found.");

        try
        {
            topic.RemoveTranslation(request.LanguageCode);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _db.SaveChangesAsync(ct);
    }
}