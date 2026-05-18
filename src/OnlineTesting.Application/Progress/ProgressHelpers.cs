using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Progress;

internal static class ProgressHelpers
{
    internal static string GetGrade(int total, double accuracy)
    {
        if (total < 5) return "Не изучено";
        return accuracy switch
        {
            >= 85 => "Отлично",
            >= 65 => "Хорошо",
            >= 40 => "Нужно повторить",
            _ => "Критично"
        };
    }

    internal static string GetTopicName(Topic topic, ILanguageContext lang)
    {
        return topic.Translations.FirstOrDefault(t => t.LanguageCode == lang.RequestedLanguage)?.Name
            ?? topic.Translations.FirstOrDefault(t => t.LanguageCode == lang.DefaultLanguage)?.Name
            ?? "(no translation)";
    }
}
