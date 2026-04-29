using MediatR;
using OnlineTesting.Application.Auth.Commands.Login;

namespace OnlineTesting.Application.Auth.Commands.TelegramLogin;

public record TelegramLoginCommand(string InitData) : IRequest<AuthResponse>;