namespace AutoDoc.Renderers;

public class RenderOptions
{
    public string TemplatesDirectory { get; set; } = "./templates";
    public string OutputDirectory { get; set; } = "./output";

    /// <summary>Absolute or relative path to the company logo file on disk. Null = none.</summary>
    public string? CompanyLogoPath { get; set; }

    /// <summary>Absolute or relative path to the client logo file on disk. Null = none.</summary>
    public string? ClientLogoPath { get; set; }
}
