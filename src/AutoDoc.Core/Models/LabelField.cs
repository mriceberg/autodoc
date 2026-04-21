using System.Globalization;

namespace AutoDoc.Core.Models;

/// <summary>
/// Represents a Dataverse Label — a localizable string that may have translations
/// for multiple installed languages.
/// </summary>
public record LabelField
{
    /// <summary>The label text in the caller's UI language (UserLocalizedLabel).</summary>
    public string? DefaultText { get; init; }

    /// <summary>All available translations, keyed by LCID.</summary>
    public IReadOnlyList<LocalizedLabel> Translations { get; init; } = [];

    public bool HasTranslations => Translations.Count > 0;

    /// <summary>
    /// Returns the text for the requested LCID, falling back to DefaultText if not found.
    /// </summary>
    public string? GetText(int languageCode) =>
        Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.Text
        ?? DefaultText;

    public static LabelField Empty => new();
}

public record LocalizedLabel
{
    public int LanguageCode { get; init; }
    public string Text { get; init; } = string.Empty;

    /// <summary>Human-readable language name derived from the LCID.</summary>
    public string LanguageName => ResolveLanguageName(LanguageCode);

    private static string ResolveLanguageName(int lcid)
    {
        try { return CultureInfo.GetCultureInfo(lcid).DisplayName; }
        catch { return lcid.ToString(); }
    }
}
