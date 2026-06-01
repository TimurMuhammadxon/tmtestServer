using MediatR;
using OnlineTesting.Application.Auth.Commands.Login;

namespace OnlineTesting.Application.Users.Commands.SetCredentials;

public record SetCredentialsCommand(string Email, string Password) : IRequest<AuthResponse>;
