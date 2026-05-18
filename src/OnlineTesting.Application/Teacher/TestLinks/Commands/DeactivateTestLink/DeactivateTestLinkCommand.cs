using MediatR;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.DeactivateTestLink;

public record DeactivateTestLinkCommand(Guid TestLinkId) : IRequest;
