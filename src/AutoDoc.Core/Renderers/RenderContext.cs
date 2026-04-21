namespace AutoDoc.Core.Renderers;

public record RenderContext
{
    public string EnvironmentName { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string RelativePathToRoot { get; set; } = "./";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<BreadcrumbItem> Breadcrumbs { get; set; } = [];

    /// <summary>
    /// LCID of the language to use when resolving localizable labels.
    /// 0 means use the default label from each LabelField.
    /// </summary>
    public int LanguageCode { get; set; } = 0;
}

public record BreadcrumbItem(string Label, string? Url = null);
