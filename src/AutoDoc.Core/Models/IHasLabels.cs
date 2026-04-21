namespace AutoDoc.Core.Models;

/// <summary>
/// Implemented by any model whose fields have Dataverse metadata labels.
/// The renderer uses this to inject a resolved labels dictionary into the template context.
/// </summary>
public interface IHasLabels
{
    /// <summary>
    /// Maps Dataverse attribute logical names (lowercase) to their localizable display labels.
    /// </summary>
    IReadOnlyDictionary<string, LabelField> Labels { get; }
}
