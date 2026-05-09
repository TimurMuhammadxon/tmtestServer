using MediatR;
using OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicsList;

namespace OnlineTesting.Application.Tests.Admin.Topics.Queries.GetTopicById;

public record GetTopicByIdQuery(Guid Id) : IRequest<TopicAdminDto>;