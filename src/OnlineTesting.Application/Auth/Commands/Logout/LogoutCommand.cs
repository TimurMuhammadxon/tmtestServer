using MediatR;

namespace OnlineTesting.Application.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;