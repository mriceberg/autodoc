namespace AutoDoc.Core.Models.Dataverse;

public record SolutionModel
{
    public Guid SolutionId { get; init; }
    public string UniqueName { get; init; } = string.Empty;
    public string FriendlyName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Version { get; init; } = string.Empty;
    public bool IsManaged { get; init; }
    public bool IsVisible { get; init; }

    // Publisher (expanded)
    public Guid? PublisherId { get; init; }
    public string? PublisherName { get; init; }
    public string? PublisherPrefix { get; init; }

    public DateTimeOffset? InstalledOn { get; init; }
    public DateTimeOffset? ModifiedOn { get; init; }
}
