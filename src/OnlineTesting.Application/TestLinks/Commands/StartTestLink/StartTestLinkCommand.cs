using MediatR;

namespace OnlineTesting.Application.TestLinks.Commands.StartTestLink;

public record StartTestLinkCommand(string Code) : IRequest<Guid>;
