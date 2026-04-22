using AutoDoc.Core.Models;
using AutoDoc.Core.Renderers;
using Microsoft.Extensions.Options;
using Scriban;
using Scriban.Runtime;

namespace AutoDoc.Renderers;

public class HtmlRenderer : IRenderer
{
    public string Format => "html";

    private readonly string _templatesDir;
    private readonly string? _companyLogoPath;
    private readonly string? _clientLogoPath;

    public HtmlRenderer(IOptions<RenderOptions> options)
    {
        _templatesDir    = options.Value.TemplatesDirectory;
        _companyLogoPath = options.Value.CompanyLogoPath;
        _clientLogoPath  = options.Value.ClientLogoPath;
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
        scriptObject["breadcrumbs"]            = context.Breadcrumbs;
        scriptObject["language_code"]          = context.LanguageCode;
        scriptObject["company_logo_filename"]  = context.CompanyLogoFilename ?? string.Empty;
        scriptObject["client_logo_filename"]   = context.ClientLogoFilename  ?? string.Empty;

        // Resolve localizable labels for the requested language and inject as a flat
        // Dictionary<string, string> so templates can simply write {{ labels["fieldname"] }}.
        if (model is IHasLabels hasLabels)
            scriptObject["labels"] = ResolveLabels(hasLabels, context.LanguageCode);

        var templateContext = new TemplateContext
        {
            TemplateLoader = new FileTemplateLoader(_templatesDir),
            StrictVariables = false
        };
        templateContext.PushGlobal(scriptObject);

        return await template.RenderAsync(templateContext);
    }

    private static Dictionary<string, string> ResolveLabels(IHasLabels model, int languageCode)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, label) in model.Labels)
        {
            var text = languageCode > 0
                ? label.GetText(languageCode) ?? label.DefaultText
                : label.DefaultText;
            result[key] = text ?? FormatLogicalName(key);
        }
        return result;
    }

    /// Converts "organizationid" → "Organization Id" as a last-resort fallback.
    private static string FormatLogicalName(string logicalName)
    {
        if (string.IsNullOrEmpty(logicalName)) return logicalName;
        return char.ToUpperInvariant(logicalName[0]) + logicalName[1..];
    }

    // File extensions that should be copied from the templates/shared dir to output/shared.
    // Template fragments (*.html) are excluded — they are Scriban source, not web assets.
    private static readonly HashSet<string> _assetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".svg", ".gif", ".webp", ".ico", ".woff", ".woff2"
    };

    private async Task CopySharedAssetsAsync(string outputDir, CancellationToken ct)
    {
        var sharedSrc = Path.Combine(_templatesDir, "shared");
        var sharedDst = Path.Combine(outputDir, "shared");
        Directory.CreateDirectory(sharedDst);

        // Copy web assets (CSS, images, fonts) — skip Scriban template fragments
        foreach (var file in Directory.GetFiles(sharedSrc))
        {
            if (!_assetExtensions.Contains(Path.GetExtension(file))) continue;
            var dest = Path.Combine(sharedDst, Path.GetFileName(file));
            await using var src = File.OpenRead(file);
            await using var dst = File.Create(dest);
            await src.CopyToAsync(dst, ct);
        }

        // Copy logo files if configured
        await CopyLogoAsync(_companyLogoPath, sharedDst, ct);
        await CopyLogoAsync(_clientLogoPath,  sharedDst, ct);
    }

    private static async Task CopyLogoAsync(string? sourcePath, string sharedDst, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sourcePath)) return;
        if (!File.Exists(sourcePath)) return;

        var dest = Path.Combine(sharedDst, Path.GetFileName(sourcePath));
        await using var src = File.OpenRead(sourcePath);
        await using var dst = File.Create(dest);
        await src.CopyToAsync(dst, ct);
    }
}
