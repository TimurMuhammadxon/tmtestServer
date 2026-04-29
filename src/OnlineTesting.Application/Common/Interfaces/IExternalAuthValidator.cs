using OnlineTesting.Application.Common.Models;

namespace OnlineTesting.Application.Common.Interfaces;

public interface IExternalAuthValidator
{
    /// <summary>
    /// Валидирует подписанный initData от Telegram WebApp.
    /// Бросает UnauthorizedException, если подпись невалидна или auth_date устарел.
    /// </summary>
    Task<TelegramAuthData> ValidateTelegramAsync(string initData, CancellationToken ct = default);
}