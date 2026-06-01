using MediatR;

namespace OnlineTesting.Application.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(string? FirstName, string? LastName) : IRequest<UpdateProfileResponse>;

public record UpdateProfileResponse(string? FirstName, string? LastName);
