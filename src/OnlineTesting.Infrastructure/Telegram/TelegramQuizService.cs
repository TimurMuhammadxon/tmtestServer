using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineTesting.Application.Common.Constants;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Users;
using OnlineTesting.Infrastructure.Authentication;

namespace OnlineTesting.Infrastructure.Telegram;

public class TelegramQuizService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramQuizService> _logger;
    private readonly HttpClient _http;
    private readonly string _botToken;
    private readonly IStorageService _storage;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(3);

    public TelegramQuizService(
        IServiceScopeFactory scopeFactory,
        ILogger<TelegramQuizService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<TelegramOptions> options,
        IStorageService storage)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _http = httpClientFactory.CreateClient();
        _botToken = options.Value.BotToken;
        _storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_botToken))
        {
            _logger.LogWarning("Telegram BotToken is not configured. Quiz service disabled.");
            return;
        }

        await Task.Delay(TimeSpan.FromMinutes(1), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await SendRandomQuizAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to send Telegram quiz");
            }

            await Task.Delay(Interval, ct);
        }
    }

    private async Task SendRandomQuizAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var totalActive = await db.Questions.CountAsync(q => q.IsActive, ct);
        if (totalActive == 0) return;

        var offset = Random.Shared.Next(totalActive);
        var question = await db.Questions
            .Where(q => q.IsActive)
            .Include(q => q.Translations)
            .Include(q => q.Answers).ThenInclude(a => a.Translations)
            .OrderBy(q => q.Id)
            .Skip(offset)
            .FirstAsync(ct);

        var questionText = question.Translations
            .FirstOrDefault(t => t.LanguageCode == Languages.UzLatn)?.Text
            ?? question.Translations.FirstOrDefault()?.Text
            ?? "Savol";

        var answers = question.Answers.OrderBy(a => a.OrderIndex).ToList();
        if (answers.Count < 2) return;

        var options = answers.Select(a =>
            a.Translations.FirstOrDefault(t => t.LanguageCode == Languages.UzLatn)?.Text
            ?? a.Translations.FirstOrDefault()?.Text
            ?? "—"
        ).ToList();

        var correctIndex = answers.FindIndex(a => a.IsCorrect);
        if (correctIndex < 0) return;

        var explanation = question.Translations
            .FirstOrDefault(t => t.LanguageCode == Languages.UzLatn)?.Explanation;

        var chatIds = await db.ExternalLogins
            .Where(e => e.Provider == ExternalLoginProvider.Telegram)
            .Select(e => e.ExternalUserId)
            .ToListAsync(ct);

        if (chatIds.Count == 0) return;

        string? imageUrl = question.ImageKey is not null ? _storage.GetPublicUrl(question.ImageKey) : null;

        var sent = 0;
        var failed = 0;

        foreach (var chatId in chatIds)
        {
            try
            {
                if (imageUrl is not null)
                    await SendPhotoAsync(chatId, imageUrl, ct);

                await SendQuizAsync(chatId, questionText, options, correctIndex, explanation, ct);
                sent++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogDebug(ex, "Failed to send quiz to chat {ChatId}", chatId);
            }
        }

        _logger.LogInformation("Quiz sent: {Sent} success, {Failed} failed, question {QuestionId}", sent, failed, question.Id);
    }

    private async Task SendPhotoAsync(string chatId, string photoUrl, CancellationToken ct)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendPhoto";
        var payload = new { chat_id = chatId, photo = photoUrl };
        var response = await _http.PostAsJsonAsync(url, payload, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task SendQuizAsync(
        string chatId, string question, List<string> options,
        int correctIndex, string? explanation, CancellationToken ct)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendPoll";
        var payload = new Dictionary<string, object>
        {
            ["chat_id"] = chatId,
            ["question"] = question.Length > 300 ? question[..297] + "..." : question,
            ["options"] = JsonSerializer.Serialize(
                options.Select(o => o.Length > 100 ? o[..97] + "..." : o)),
            ["type"] = "quiz",
            ["correct_option_id"] = correctIndex,
            ["is_anonymous"] = false,
        };

        if (!string.IsNullOrWhiteSpace(explanation))
            payload["explanation"] = explanation.Length > 200 ? explanation[..197] + "..." : explanation;

        var response = await _http.PostAsJsonAsync(url, payload, ct);
        response.EnsureSuccessStatusCode();
    }
}
