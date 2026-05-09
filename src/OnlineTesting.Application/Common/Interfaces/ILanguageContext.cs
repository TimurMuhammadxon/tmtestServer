namespace OnlineTesting.Application.Common.Interfaces;

/// <summary>
/// Resolved language for the current request. Populated by API middleware
/// from Accept-Language header or ?lang= query.
/// </summary>
public interface ILanguageContext
{
    string RequestedLanguage { get; }
    string DefaultLanguage { get; }
}