using MediatR;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.DeleteTopicTranslation;

public record DeleteTopicTranslationCommand(
    Guid TopicId,
    string LanguageCode) : IRequest;