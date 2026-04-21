using AutoDoc.Core.Renderers;
using Microsoft.Extensions.Options;

namespace AutoDoc.Renderers;

/// <summary>
/// PDF rendering via PuppeteerSharp (HTML → headless Chrome → PDF).
/// Renders HTML first, then converts. Planned for Phase 2.
/// </summary>
public class PdfRenderer : IRenderer
{
    public string Format => "pdf";

    private readonly HtmlRenderer _html;

    public PdfRenderer(IOptions<RenderOptions> options)
    {
        _html = new HtmlRenderer(options);
    }

    public Task RenderAsync(
        object model,
        string templateName,
        string outputPath,
        RenderContext context,
        CancellationToken ct = default)
        => throw new NotImplementedException("PDF rendering will be implemented in Phase 2.");

    public Task RenderIndexAsync(
        IReadOnlyList<ReportEntry> entries,
        string outputDir,
        RenderContext context,
        CancellationToken ct = default)
        => throw new NotImplementedException("PDF rendering will be implemented in Phase 2.");
}
