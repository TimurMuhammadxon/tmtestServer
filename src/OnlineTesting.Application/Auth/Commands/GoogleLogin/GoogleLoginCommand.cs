using MediatR;
using OnlineTesting.Application.Auth.Commands.Login;

namespace OnlineTesting.Application.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<AuthResponse>;
