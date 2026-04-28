using MediatR;
using OnlineTesting.Application.Auth.Commands.Login;

namespace OnlineTesting.Application.Auth.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<AuthResponse>;