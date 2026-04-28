using MediatR;

namespace OnlineTesting.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<RegisterResponse>;