using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    private const int TokenBytes = 64;

    private readonly JwtOptions _options;

    public RefreshTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TimeSpan Lifetime => TimeSpan.FromDays(_options.RefreshTokenDays);

    public (string Raw, string Hash) Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenBytes);
        var raw = Base64UrlEncode(bytes);
        return (raw, Hash(raw));
    }

    public string Hash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}