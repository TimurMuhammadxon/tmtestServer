using OnlineTesting.Application.Common.Constants;
using OnlineTesting.Application.Common.Interfaces;
using OnlineTesting.Domain.Tests;

namespace OnlineTesting.Application.Progress;

internal static class ProgressHelpers
{
    internal static string GetGrade(int total, double accuracy, string lang = Languages.UzLatn)
    {
        if (total < 5) return lang switch
        {
            Languages.Ru => "Не изучено",
            Languages.UzCyrl => "Ўрганилмаган",
            _ => "O'rganilmagan",
        };
        return (accuracy, lang) switch
        {
            (>= 85, Languages.Ru) => "Отлично",
            (>= 85, Languages.UzCyrl) => "Аъло",
            (>= 85, _) => "A'lo",

            (>= 65, Languages.Ru) => "Хорошо",
            (>= 65, Languages.UzCyrl) => "Яхши",
            (>= 65, _) => "Yaxshi",

            (>= 40, Languages.Ru) => "Нужно повторить",
            (>= 40, Languages.UzCyrl) => "Такрорлаш керак",
            (>= 40, _) => "Takrorlash kerak",

            (_, Languages.Ru) => "Критично",
            (_, Languages.UzCyrl) => "Критик",
            _ => "Kritik",
        };
    }

    internal static string GetTopicName(Topic topic, ILanguageContext lang)
    {
        return topic.Translations.FirstOrDefault(t => t.LanguageCode == lang.RequestedLanguage)?.Name
            ?? topic.Translations.FirstOrDefault(t => t.LanguageCode == lang.DefaultLanguage)?.Name
            ?? "(no translation)";
    }
}
