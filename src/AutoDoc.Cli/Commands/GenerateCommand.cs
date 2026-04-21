using AutoDoc.Collectors.Dataverse;
using AutoDoc.Core.Models.Dataverse;
using AutoDoc.Core.Renderers;
using AutoDoc.Infrastructure.Auth;
using AutoDoc.Infrastructure.Http;
using AutoDoc.Renderers;
using Microsoft.Extensions.Options;
using System.CommandLine;

namespace AutoDoc.Cli.Commands;

public static class GenerateCommand
{
    public static Command Build()
    {
        var envOption = new Option<string>(
            name: "--environment",
            description: "Target environment name as defined in secrets.json")
        { IsRequired = true };
        envOption.AddAlias("-e");

        var formatOption = new Option<string>(
            name: "--format",
            getDefaultValue: () => "html",
            description: "Output format: html (default) | pdf");
        formatOption.AddAlias("-f");

        var outputOption = new Option<string>(
            name: "--output",
            getDefaultValue: () => "./output",
            description: "Output directory");
        outputOption.AddAlias("-o");

        var secretsOption = new Option<string>(
            name: "--secrets",
            getDefaultValue: () => "./secrets.json",
            description: "Path to secrets.json");

        var languageOption = new Option<int>(
            name: "--language",
            getDefaultValue: () => 0,
            description: "LCID of the language to use for field labels (e.g. 1033 for English, " +
                         "1036 for French). Defaults to 0 which uses the Dataverse default label.");
        languageOption.AddAlias("-l");

        var cmd = new Command("generate", "Generate documentation for a Power Platform environment");
        cmd.AddOption(envOption);
        cmd.AddOption(formatOption);
        cmd.AddOption(outputOption);
        cmd.AddOption(secretsOption);
        cmd.AddOption(languageOption);

        cmd.SetHandler(async (env, format, output, secretsPath, language) =>
        {
            await RunAsync(env, format, output, secretsPath, language);
        }, envOption, formatOption, outputOption, secretsOption, languageOption);

        return cmd;
    }

    private static async Task RunAsync(
        string environmentName, string format, string outputDir, string secretsPath, int languageCode)
    {
        Console.WriteLine($"AutoDoc — generating documentation for environment: {environmentName}");
        if (languageCode > 0)
            Console.WriteLine($"  Language: {languageCode}");

        // Load secrets
        var secrets = LocalSecretsLoader.Load(secretsPath);
        var env = LocalSecretsLoader.GetEnvironment(secrets, environmentName);

        if (env.Dataverse is null)
            throw new InvalidOperationException(
                $"Environment '{environmentName}' has no Dataverse configuration.");

        // Build auth + HTTP client
        var auth = AuthProviderFactory.Create(env.Dataverse.Auth);
        var http = new HttpClient();
        var dvClient = new DataverseClient(http, auth, env.Dataverse.EnvironmentUrl);

        // Build renderer
        var renderOptions = Options.Create(new RenderOptions
        {
            TemplatesDirectory = ResolveTemplatesDir(),
            OutputDirectory    = outputDir
        });

        IRenderer renderer = format.ToLowerInvariant() switch
        {
            "html" => new HtmlRenderer(renderOptions),
            "pdf"  => new PdfRenderer(renderOptions),
            _      => throw new ArgumentException($"Unknown format: '{format}'")
        };

        var baseContext = new RenderContext
        {
            EnvironmentName = environmentName,
            GeneratedAt     = DateTime.UtcNow,
            LanguageCode    = languageCode
        };

        var reportEntries = new List<ReportEntry>();

        // --- Organization ---
        Console.Write("  Fetching organization...");
        var orgCollector = new OrganizationCollector(dvClient);
        var orgs = await orgCollector.FetchAsync();
        if (orgs.Count > 0)
        {
            var org = orgs[0];
            var ctx = baseContext with
            {
                PageTitle          = $"Organization — {org.Name}",
                RelativePathToRoot = "../",
                Breadcrumbs        = [new("Home", "../index.html"), new("Organization")]
            };
            var path = Path.Combine(outputDir, "dataverse", "organization.html");
            await renderer.RenderAsync(org, "dataverse/organization", path, ctx);
            reportEntries.Add(new ReportEntry(
                $"Organization: {org.Name}", "dataverse/organization.html",
                "Organization", org.Name));
        }
        Console.WriteLine(" done.");

        // --- Publishers ---
        Console.Write("  Fetching publishers...");
        var pubCollector = new PublisherCollector(dvClient);
        var publishers = await pubCollector.FetchAsync();
        foreach (var pub in publishers)
        {
            var ctx = baseContext with
            {
                PageTitle          = $"Publisher — {pub.FriendlyName}",
                RelativePathToRoot = "../",
                Breadcrumbs        = [new("Home", "../index.html"), new("Publishers", null), new(pub.FriendlyName)]
            };
            var fileName = $"publisher-{Slugify(pub.UniqueName)}.html";
            var path = Path.Combine(outputDir, "dataverse", fileName);
            await renderer.RenderAsync(pub, "dataverse/publisher", path, ctx);
            reportEntries.Add(new ReportEntry(
                $"Publisher: {pub.FriendlyName}", $"dataverse/{fileName}",
                "Publisher", pub.Description));
        }
        Console.WriteLine($" {publishers.Count} done.");

        // --- Solutions ---
        Console.Write("  Fetching solutions...");
        var solCollector = new SolutionCollector(dvClient);
        var solutions = await solCollector.FetchAsync();
        foreach (var sol in solutions)
        {
            var ctx = baseContext with
            {
                PageTitle          = $"Solution — {sol.FriendlyName}",
                RelativePathToRoot = "../",
                Breadcrumbs        = [new("Home", "../index.html"), new("Solutions", null), new(sol.FriendlyName)]
            };
            var fileName = $"solution-{Slugify(sol.UniqueName)}.html";
            var path = Path.Combine(outputDir, "dataverse", fileName);
            await renderer.RenderAsync(sol, "dataverse/solution", path, ctx);
            reportEntries.Add(new ReportEntry(
                $"Solution: {sol.FriendlyName}", $"dataverse/{fileName}",
                "Solution", sol.Description));
        }
        Console.WriteLine($" {solutions.Count} done.");

        // --- Business Units ---
        Console.Write("  Fetching business units...");
        var buCollector = new BusinessUnitCollector(dvClient);
        var businessUnits = await buCollector.FetchAsync();
        foreach (var bu in businessUnits)
        {
            var ctx = baseContext with
            {
                PageTitle          = $"Business Unit — {bu.Name}",
                RelativePathToRoot = "../",
                Breadcrumbs        = [new("Home", "../index.html"), new("Business Units", null), new(bu.Name)]
            };
            var fileName = $"bu-{Slugify(bu.Name)}.html";
            var path = Path.Combine(outputDir, "dataverse", fileName);
            await renderer.RenderAsync(bu, "dataverse/business_unit", path, ctx);
            reportEntries.Add(new ReportEntry(
                $"Business Unit: {bu.Name}", $"dataverse/{fileName}",
                "Business Unit", bu.Description));
        }
        Console.WriteLine($" {businessUnits.Count} done.");

        // --- Index ---
        Console.Write("  Generating index...");
        var indexCtx = baseContext with
        {
            PageTitle          = $"Documentation — {environmentName}",
            RelativePathToRoot = "./"
        };
        await renderer.RenderIndexAsync(reportEntries, outputDir, indexCtx);
        Console.WriteLine(" done.");

        Console.WriteLine($"\nDocumentation written to: {Path.GetFullPath(outputDir)}");
    }

    private static string Slugify(string value) =>
        System.Text.RegularExpressions.Regex
            .Replace(value.ToLowerInvariant(), @"[^a-z0-9]+", "-")
            .Trim('-');

    private static string ResolveTemplatesDir()
    {
        // Look for templates next to the executable, then fall back to cwd
        var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        var candidate = Path.Combine(exeDir, "templates");
        return Directory.Exists(candidate) ? candidate : Path.Combine(Directory.GetCurrentDirectory(), "templates");
    }
}
