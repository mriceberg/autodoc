namespace AutoDoc.Core.Models.Dataverse;

public record BusinessUnitModel : IHasLabels
{
    public Guid BusinessUnitId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    // Hierarchy
    public Guid? ParentBusinessUnitId { get; init; }
    public string? ParentBusinessUnitName { get; init; }
    public bool IsRoot { get; init; }

    // Contact
    public string? WebsiteUrl { get; init; }
    public string? EmailAddress { get; init; }
    public string? Address1_City { get; init; }
    public string? Address1_Country { get; init; }

    // State
    public bool IsDisabled { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
    public DateTimeOffset? ModifiedOn { get; init; }

    public IReadOnlyDictionary<string, LabelField> Labels { get; init; } =
        new Dictionary<string, LabelField>(StringComparer.OrdinalIgnoreCase);
}
