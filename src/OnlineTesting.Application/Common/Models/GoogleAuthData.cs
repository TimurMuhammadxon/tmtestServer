namespace OnlineTesting.Application.Common.Models;

public record GoogleAuthData(
    string ExternalUserId,
    string Email,
    string? Name);
