namespace AutoDoc.Core.Renderers;

public record RenderContext
{
    public string EnvironmentName { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string RelativePathToRoot { get; set; } = "./";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<BreadcrumbItem> Breadcrumbs { get; set; } = [];
}

public record BreadcrumbItem(string Label, string? Url = null);
