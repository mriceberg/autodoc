using System.Text.Json;
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
