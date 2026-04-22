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

    /// <summary>
    /// File name (no path) of the company logo, relative to output/shared/.
    /// Null when no logo was configured.
    /// </summary>
    public string? CompanyLogoFilename { get; set; }

    /// <summary>
    /// File name (no path) of the client logo, relative to output/shared/.
    /// Null when no logo was configured.
    /// </summary>
    public string? ClientLogoFilename { get; set; }
}

public record BreadcrumbItem(string Label, string? Url = null);
