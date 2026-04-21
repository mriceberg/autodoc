using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using IScribanTemplateLoader = Scriban.Runtime.ITemplateLoader;

namespace AutoDoc.Renderers;

internal sealed class FileTemplateLoader : IScribanTemplateLoader
{
    private readonly string _baseDirectory;

    public FileTemplateLoader(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        => Path.Combine(_baseDirectory, templateName + ".html");

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        => File.ReadAllText(templatePath);

    public async ValueTask<string?> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        => await File.ReadAllTextAsync(templatePath);
}
