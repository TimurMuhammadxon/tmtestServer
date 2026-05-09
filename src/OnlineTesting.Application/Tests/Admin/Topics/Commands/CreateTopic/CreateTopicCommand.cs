using MediatR;
using OnlineTesting.Application.Tests.Common;

namespace OnlineTesting.Application.Tests.Admin.Topics.Commands.CreateTopic;

public record CreateTopicCommand(
    string Code,
    int OrderIndex,
    bool IsDemo,
    List<TopicTranslationDto> Translations) : IRequest<CreateTopicResponse>;

public record CreateTopicResponse(Guid Id);

