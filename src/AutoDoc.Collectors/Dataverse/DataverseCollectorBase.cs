using System.Text.Json;
using AutoDoc.Core.Models;
using AutoDoc.Infrastructure.Http;

namespace AutoDoc.Collectors.Dataverse;

public abstract class DataverseCollectorBase
{
    protected readonly DataverseClient Client;

    protected DataverseCollectorBase(DataverseClient client)
    {
        Client = client;
    }

    protected static string? S(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetString()
            : null;

    protected static Guid? G(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetGuid()
            : null;

    protected static bool? B(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetBoolean()
            : null;

    protected static int? I(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetInt32()
            : null;

    protected static DateTimeOffset? D(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.GetDateTimeOffset()
            : null;

    /// <summary>
    /// Parses a Dataverse Label object (as returned by the metadata API) into a LabelField.
    /// Expected JSON shape:
    /// { "LocalizedLabels": [{ "Label": "...", "LanguageCode": 1033 }, ...],
    ///   "UserLocalizedLabel": { "Label": "...", "LanguageCode": 1033 } }
    /// </summary>
    protected static LabelField ParseLabel(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var labelEl) || labelEl.ValueKind != JsonValueKind.Object)
            return LabelField.Empty;

        string? defaultText = null;
        if (labelEl.TryGetProperty("UserLocalizedLabel", out var userLabel)
            && userLabel.ValueKind == JsonValueKind.Object)
            defaultText = S(userLabel, "Label");

        var translations = new List<LocalizedLabel>();
        if (labelEl.TryGetProperty("LocalizedLabels", out var localizedArray)
            && localizedArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in localizedArray.EnumerateArray())
            {
                var text = S(item, "Label");
                var code = I(item, "LanguageCode");
                if (text is not null && code is not null)
                    translations.Add(new LocalizedLabel { LanguageCode = code.Value, Text = text });
            }
        }

        // Sort by language code for consistent display order
        translations.Sort((a, b) => a.LanguageCode.CompareTo(b.LanguageCode));

        return new LabelField
        {
            DefaultText  = defaultText ?? translations.FirstOrDefault()?.Text,
            Translations = translations
        };
    }

    /// <summary>
    /// Collects all string-valued properties from the element into a dictionary,
    /// excluding the ones already explicitly mapped (to avoid duplication).
    /// </summary>
    protected static Dictionary<string, string?> AdditionalProperties(
        JsonElement el, IEnumerable<string> excludedKeys)
    {
        var excluded = new HashSet<string>(excludedKeys, StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, string?>();

        foreach (var prop in el.EnumerateObject())
        {
            if (excluded.Contains(prop.Name) || prop.Name.StartsWith('@'))
                continue;

            result[prop.Name] = prop.Value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => prop.Value.GetString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => prop.Value.ToString(),
                _ => null
            };
        }

        return result;
    }
}
