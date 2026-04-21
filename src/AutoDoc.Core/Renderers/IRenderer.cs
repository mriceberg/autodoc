namespace AutoDoc.Core.Renderers;

public interface IRenderer
{
    string Format { get; }

    Task RenderAsync(
        object model,
        string templateName,
        string outputPath,
        RenderContext context,
        CancellationToken ct = default);

    Task RenderIndexAsync(
        IReadOnlyList<ReportEntry> entries,
        string outputDir,
        RenderContext context,
        CancellationToken ct = default);
}
