using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Tests.Common;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.CreateTopic;

public class CreateTopicHandler : IRequestHandler<CreateTopicCommand, CreateTopicResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IDbExceptionInspector _inspector;

    public CreateTopicHandler(IApplicationDbContext db, IDbExceptionInspector inspector)
    {
        _db = db;
        _inspector = inspector;
    }

    public async Task<CreateTopicResponse> Handle(CreateTopicCommand request, CancellationToken ct)
    {
        var translations = request.Translations
            .Select(t => (t.LanguageCode.ToLowerInvariant(), t.Name))
            .ToList();

        var topic = Topic.Create(request.Code, request.OrderIndex, request.IsDemo, translations);

        _db.Topics.Add(topic);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_inspector.IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException($"Topic with code '{request.Code}' already exists.");
        }

        return new CreateTopicResponse(topic.Id);
    }
}