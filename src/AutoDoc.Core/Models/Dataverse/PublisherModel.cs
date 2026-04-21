namespace AutoDoc.Core.Models.Dataverse;

public record PublisherModel
{
    public Guid PublisherId { get; init; }
    public string UniqueName { get; init; } = string.Empty;
    public string FriendlyName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CustomizationPrefix { get; init; }
    public int? CustomizationOptionValuePrefix { get; init; }
    public string? SupportingWebsiteUrl { get; init; }
    public string? EMailAddress { get; init; }
    public bool IsReadonly { get; init; }
    public DateTimeOffset? ModifiedOn { get; init; }
}
