namespace OnlineTesting.Application.Common.Models;

public record TelegramAuthData(
    string ExternalUserId,
    string? Username,
    string? FirstName,
    string? LastName,
    DateTime AuthDate);