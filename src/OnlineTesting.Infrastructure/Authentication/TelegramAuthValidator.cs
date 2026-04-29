using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Infrastructure.Authentication;

public class TelegramAuthValidator : ITelegramAuthValidator
{
    private const string InvalidAuthMessage = "Telegram authentication failed.";

    private readonly TelegramOptions _options;
    private readonly byte[] _secretKey;

    public TelegramAuthValidator(IOptions<TelegramOptions> options)
    {
        _options = options.Value;

        // Конфигурация уже провалидирована в Infrastructure/DependencyInjection.AddTelegram() (fail-fast при старте).
        // secret_key детерминирован от bot token — кэшируем один раз, чтобы не пересчитывать на каждом запросе.
        _secretKey = HmacSha256(
            key: Encoding.UTF8.GetBytes("WebAppData"),
            data: Encoding.UTF8.GetBytes(_options.BotToken));
    }

    public Task<TelegramAuthData> ValidateAsync(string initData, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(initData))
            throw new UnauthorizedException(InvalidAuthMessage);

        var pairs = ParseInitData(initData);

        if (!pairs.TryGetValue("hash", out var providedHash) || string.IsNullOrEmpty(providedHash))
            throw new UnauthorizedException(InvalidAuthMessage);

        if (!pairs.TryGetValue("auth_date", out var authDateStr) ||
            !long.TryParse(authDateStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var authDateUnix))
            throw new UnauthorizedException(InvalidAuthMessage);

        var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix).UtcDateTime;
        var age = DateTime.UtcNow - authDate;
        var maxAge = TimeSpan.FromHours(_options.AuthDateExpirationHours);

        // Учитываем как устаревший, так и "из будущего" timestamp.
        if (age < TimeSpan.Zero || age > maxAge)
            throw new UnauthorizedException(InvalidAuthMessage);

        if (!IsHashValid(pairs, providedHash))
            throw new UnauthorizedException(InvalidAuthMessage);

        if (!pairs.TryGetValue("user", out var userJson) || string.IsNullOrEmpty(userJson))
            throw new UnauthorizedException(InvalidAuthMessage);

        var user = ParseUser(userJson)
            ?? throw new UnauthorizedException(InvalidAuthMessage);

        return Task.FromResult(new TelegramAuthData(
            ExternalUserId: user.Id.ToString(CultureInfo.InvariantCulture),
            Username: user.Username,
            FirstName: user.FirstName,
            LastName: user.LastName,
            AuthDate: authDate));
    }

    private static Dictionary<string, string> ParseInitData(string initData)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var pair in initData.Split('&'))
        {
            var eqIdx = pair.IndexOf('=');
            if (eqIdx < 0) continue;

            var key = pair[..eqIdx];
            var value = HttpUtility.UrlDecode(pair[(eqIdx + 1)..]);
            result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// Спецификация: https://core.telegram.org/bots/webapps#validating-data-received-via-the-mini-app
    /// </summary>
    private bool IsHashValid(Dictionary<string, string> pairs, string providedHash)
    {
        var dataCheckString = string.Join('\n',
            pairs.Where(p => p.Key != "hash")
                 .OrderBy(p => p.Key, StringComparer.Ordinal)
                 .Select(p => $"{p.Key}={p.Value}"));

        var computedHash = HmacSha256(
            key: _secretKey,
            data: Encoding.UTF8.GetBytes(dataCheckString));

        byte[] providedBytes;
        try
        {
            providedBytes = Convert.FromHexString(providedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        if (providedBytes.Length != computedHash.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(computedHash, providedBytes);
    }

    private static byte[] HmacSha256(byte[] key, byte[] data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }

    private static TelegramUserDto? ParseUser(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<TelegramUserDto>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record TelegramUserDto(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("username")] string? Username,
        [property: JsonPropertyName("first_name")] string? FirstName,
        [property: JsonPropertyName("last_name")] string? LastName);
}