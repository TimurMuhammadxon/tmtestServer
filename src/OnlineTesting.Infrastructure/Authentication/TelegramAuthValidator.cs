using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Exceptions;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Infrastructure.Authentication;

public class TelegramAuthValidator : IExternalAuthValidator
{
    private const string InvalidAuthMessage = "Telegram authentication failed.";

    private readonly TelegramOptions _options;

    public TelegramAuthValidator(IOptions<TelegramOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.BotToken))
            throw new InvalidOperationException("Telegram:BotToken is not configured.");
    }

    public Task<TelegramAuthData> ValidateTelegramAsync(string initData, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(initData))
            throw new UnauthorizedException(InvalidAuthMessage);

        // Шаг 1: Парсинг initData (формат query string)
        var pairs = ParseInitData(initData);

        if (!pairs.TryGetValue("hash", out var providedHash) || string.IsNullOrEmpty(providedHash))
            throw new UnauthorizedException(InvalidAuthMessage);

        if (!pairs.TryGetValue("auth_date", out var authDateStr) ||
            !long.TryParse(authDateStr, out var authDateUnix))
            throw new UnauthorizedException(InvalidAuthMessage);

        // Шаг 2: Свежесть auth_date
        var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix).UtcDateTime;
        var maxAge = TimeSpan.FromHours(_options.AuthDateExpirationHours);

        if (DateTime.UtcNow - authDate > maxAge)
            throw new UnauthorizedException(InvalidAuthMessage);

        // Шаг 3: Валидация подписи
        if (!IsHashValid(pairs, providedHash))
            throw new UnauthorizedException(InvalidAuthMessage);

        // Шаг 4: Извлечение user из JSON
        if (!pairs.TryGetValue("user", out var userJson) || string.IsNullOrEmpty(userJson))
            throw new UnauthorizedException(InvalidAuthMessage);

        var user = ParseUser(userJson)
            ?? throw new UnauthorizedException(InvalidAuthMessage);

        return Task.FromResult(new TelegramAuthData(
            ExternalUserId: user.Id.ToString(),
            Username: user.Username,
            FirstName: user.FirstName,
            LastName: user.LastName,
            AuthDate: authDate));
    }

    /// <summary>
    /// Парсит initData как query string. Значения URL-decoded.
    /// </summary>
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
    /// Проверяет HMAC-SHA256 подпись по официальной спецификации Telegram WebApp.
    /// https://core.telegram.org/bots/webapps#validating-data-received-via-the-mini-app
    /// </summary>
    private bool IsHashValid(Dictionary<string, string> pairs, string providedHash)
    {
        // 1. Собираем data-check-string из всех пар, кроме hash, отсортированных по ключу.
        var dataCheckString = string.Join('\n',
            pairs.Where(p => p.Key != "hash")
                 .OrderBy(p => p.Key, StringComparer.Ordinal)
                 .Select(p => $"{p.Key}={p.Value}"));

        // 2. secret_key = HMAC_SHA256("WebAppData", bot_token)
        var secretKey = HmacSha256(
            key: Encoding.UTF8.GetBytes("WebAppData"),
            data: Encoding.UTF8.GetBytes(_options.BotToken));

        // 3. computed_hash = HMAC_SHA256(secret_key, data_check_string)
        var computedHash = HmacSha256(
            key: secretKey,
            data: Encoding.UTF8.GetBytes(dataCheckString));

        // 4. Сравнение в hex, constant-time
        var providedBytes = ConvertHexToBytes(providedHash);
        if (providedBytes is null || providedBytes.Length != computedHash.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(computedHash, providedBytes);
    }

    private static byte[] HmacSha256(byte[] key, byte[] data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }

    private static byte[]? ConvertHexToBytes(string hex)
    {
        if (hex.Length % 2 != 0) return null;

        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber,
                    null, out bytes[i]))
                return null;
        }
        return bytes;
    }

    private static TelegramUserDto? ParseUser(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<TelegramUserDto>(
                json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record TelegramUserDto(
        long Id,
        string? Username,
        string? FirstName,
        string? LastName);
}