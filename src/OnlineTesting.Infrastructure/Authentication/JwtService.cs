using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(Guid userId, string email)
    {
        // TODO: реализовать генерацию JWT
        throw new NotImplementedException();
    }
}