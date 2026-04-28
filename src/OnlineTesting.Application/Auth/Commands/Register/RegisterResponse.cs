using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Auth.Commands.Register;

public record RegisterResponse(Guid Id, string Email, Role Role);