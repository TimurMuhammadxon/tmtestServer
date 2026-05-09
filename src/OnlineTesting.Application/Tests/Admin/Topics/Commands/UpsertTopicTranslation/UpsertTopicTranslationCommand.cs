using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.UpsertTopicTranslation;

public record UpsertTopicTranslationCommand(
    Guid TopicId,
    string LanguageCode,
    string Name) : IRequest;