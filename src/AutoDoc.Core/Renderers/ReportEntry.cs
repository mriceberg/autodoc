namespace AutoDoc.Core.Renderers;

public record ReportEntry(
    string Title,
    string RelativePath,
    string ElementType,
    string? Description = null);
