using OnlineTesting.Domain.Users;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    int AccessTokenExpirationSeconds { get; }
}