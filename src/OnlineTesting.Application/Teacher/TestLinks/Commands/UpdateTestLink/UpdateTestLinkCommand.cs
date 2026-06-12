using MediatR;

namespace OnlineTesting.Application.Teacher.TestLinks.Commands.UpdateTestLink;

public record UpdateTestLinkCommand(Guid Id, string Title, int MaxAttempts, DateTime ExpiresAt) : IRequest;
