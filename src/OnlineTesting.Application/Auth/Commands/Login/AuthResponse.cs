namespace OnlineTesting.Application.Auth.Commands.Login;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);