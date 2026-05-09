using MediatR;
using OnlineTesting.Application.Common.Models;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicsList;

public record GetTopicsListQuery(
    int Page = 1,
    int PageSize = 20,
    bool IncludeTranslations = false) : IRequest<PagedResult<TopicAdminDto>>;

public record TopicAdminDto(
    Guid Id,
    string Code,
    int OrderIndex,
    bool IsDemo,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<TopicTranslationDto>? Translations);