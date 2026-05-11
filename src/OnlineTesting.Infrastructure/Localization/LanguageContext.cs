using Microsoft.AspNetCore.Http;
using OnlineTesting.Application.Common.Constants;
using OnlineTesting.Application.Common.Interfaces;

namespace OnlineTesting.Infrastructure.Localization;

public class LanguageContext : ILanguageContext
{
    public string RequestedLanguage { get; private set; } = Languages.Default;
    public string DefaultLanguage => Languages.Default;

    public void SetFromRequest(HttpContext ctx)
    {
        var fromQuery = ctx.Request.Query["lang"].FirstOrDefault();
        if (Languages.IsSupported(fromQuery))
        {
            RequestedLanguage = fromQuery!.ToLowerInvariant();
            return;
        }

        var header = ctx.Request.Headers.AcceptLanguage.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(header))
        {
            var first = header.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(';')[0].Trim())
                .FirstOrDefault(Languages.IsSupported);
            if (first is not null)
            {
                RequestedLanguage = first.ToLowerInvariant();
                return;
            }
        }

        RequestedLanguage = Languages.Default;
    }
}