using MediatR;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Tests.Admin.Questions.Queries.GetQuestionsList;

public record GetQuestionsListQuery(
    Guid? TopicId,
    string? Search,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<QuestionAdminListItemDto>>;

public record QuestionAdminListItemDto(
    Guid Id,
    Guid TopicId,
    string? ImageKey,
    string? ImageUrl,
    bool IsActive,
    string DefaultText,
    int AnswersCount);