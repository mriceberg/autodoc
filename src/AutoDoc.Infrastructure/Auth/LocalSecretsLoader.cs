using System.Text.Json;

namespace AutoDoc.Infrastructure.Auth;

public class LocalSecretsLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static SecretsConfig Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Secrets file not found: {path}");

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SecretsConfig>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Failed to parse secrets file: {path}");
    }

    public static EnvironmentSecrets GetEnvironment(SecretsConfig config, string environmentName)
    {
        if (!config.Environments.TryGetValue(environmentName, out var env))
            throw new KeyNotFoundException(
                $"Environment '{environmentName}' not found in secrets. " +
                $"Available: {string.Join(", ", config.Environments.Keys)}");

        return env;
    }
}
