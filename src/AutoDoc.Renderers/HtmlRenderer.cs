using AutoDoc.Core.Renderers;
using Microsoft.Extensions.Options;
using Scriban;
using Scriban.Runtime;

namespace AutoDoc.Renderers;

public class HtmlRenderer : IRenderer
{
    public string Format => "html";

    private readonly string _templatesDir;

    public HtmlRenderer(IOptions<RenderOptions> options)
    {
        _templatesDir = options.Value.TemplatesDirectory;
    }

    public async Task RenderAsync(
        object model,
        string templateName,
        string outputPath,
        RenderContext context,
        CancellationToken ct = default)
    {
        var html = await RenderTemplateAsync(templateName, model, context, ct);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, html, ct);
    }

    public async Task RenderIndexAsync(
        IReadOnlyList<ReportEntry> entries,
        string outputDir,
        RenderContext context,
        CancellationToken ct = default)
    {
        var html = await RenderTemplateAsync("index", entries, context, ct);
        Directory.CreateDirectory(outputDir);
        await File.WriteAllTextAsync(Path.Combine(outputDir, "index.html"), html, ct);

        await CopySharedAssetsAsync(outputDir, ct);
    }

    private async Task<string> RenderTemplateAsync(
        string templateName, object model, RenderContext context, CancellationToken ct)
    {
        var templatePath = Path.Combine(_templatesDir, templateName + ".html");
        var templateContent = await File.ReadAllTextAsync(templatePath, ct);

        var template = Template.Parse(templateContent, templatePath);
        if (template.HasErrors)
            throw new InvalidOperationException(
                $"Template errors in '{templateName}':\n" +
                string.Join("\n", template.Messages));

        var scriptObject = new ScriptObject();
        scriptObject.Import(model, renamer: member => member.Name
            .ToLower()
            .Replace("_", string.Empty)); // models use PascalCase; Scriban prefers snake_case via Import

        // Expose context variables directly in the root scope
        scriptObject["page_title"]            = context.PageTitle;
        scriptObject["environment_name"]      = context.EnvironmentName;
        scriptObject["relative_path_to_root"] = context.RelativePathToRoot;
        scriptObject["generated_at"]          = context.GeneratedAt.ToString("yyyy-MM-dd HH:mm") + " UTC";
        scriptObject["breadcrumbs"]           = context.Breadcrumbs;

        var templateContext = new TemplateContext
        {
            TemplateLoader = new FileTemplateLoader(_templatesDir),
            StrictVariables = false
        };
        templateContext.PushGlobal(scriptObject);

        return await template.RenderAsync(templateContext);
    }

    private async Task CopySharedAssetsAsync(string outputDir, CancellationToken ct)
    {
        var sharedSrc = Path.Combine(_templatesDir, "shared");
        var sharedDst = Path.Combine(outputDir, "shared");
        Directory.CreateDirectory(sharedDst);

        foreach (var file in Directory.GetFiles(sharedSrc))
        {
            var dest = Path.Combine(sharedDst, Path.GetFileName(file));
            await using var src = File.OpenRead(file);
            await using var dst = File.Create(dest);
            await src.CopyToAsync(dst, ct);
        }
    }
}
