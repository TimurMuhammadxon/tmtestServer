using MediatR;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.ActivateTestLink;

public record ActivateTestLinkCommand(Guid TestLinkId) : IRequest;
