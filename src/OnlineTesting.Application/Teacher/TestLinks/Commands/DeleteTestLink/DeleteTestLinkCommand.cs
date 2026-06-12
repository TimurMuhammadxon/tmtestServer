using MediatR;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.DeleteTestLink;

public record DeleteTestLinkCommand(Guid Id) : IRequest;
