using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicsList;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicById;

public class GetTopicByIdHandler : IRequestHandler<GetTopicByIdQuery, TopicAdminDto>
{
    private readonly IApplicationDbContext _db;
    public GetTopicByIdHandler(IApplicationDbContext db) => _db = db;

    public async Task<TopicAdminDto> Handle(GetTopicByIdQuery request, CancellationToken ct)
    {
        var topic = await _db.Topics
            .AsNoTracking()
            .Include(t => t.Translations)
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new NotFoundException($"Topic '{request.Id}' not found.");

        return new TopicAdminDto(
            topic.Id, topic.Code, topic.OrderIndex, topic.IsDemo, topic.IsActive,
            topic.CreatedAt, topic.UpdatedAt,
            topic.Translations
                .OrderBy(t => t.LanguageCode)
                .Select(t => new TopicTranslationDto(t.LanguageCode, t.Name))
                .ToList());
    }
}