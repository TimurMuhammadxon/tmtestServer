using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.UpsertTopicTranslation;

public class UpsertTopicTranslationHandler : IRequestHandler<UpsertTopicTranslationCommand>
{
    private readonly IApplicationDbContext _db;
    public UpsertTopicTranslationHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpsertTopicTranslationCommand request, CancellationToken ct)
    {
        var topic = await _db.Topics
            .Include(t => t.Translations)
            .FirstOrDefaultAsync(t => t.Id == request.TopicId, ct)
            ?? throw new NotFoundException($"Topic '{request.TopicId}' not found.");

        topic.UpsertTranslation(request.LanguageCode, request.Name);
        await _db.SaveChangesAsync(ct);
    }
}