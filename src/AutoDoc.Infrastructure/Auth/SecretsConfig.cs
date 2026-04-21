using System.Text.Json.Serialization;

namespace AutoDoc.Infrastructure.Auth;

public class SecretsConfig
{
    [JsonPropertyName("environments")]
    public Dictionary<string, EnvironmentSecrets> Environments { get; set; } = [];
}

public class EnvironmentSecrets
{
    [JsonPropertyName("dataverse")]
    public DataverseSecrets? Dataverse { get; set; }

    [JsonPropertyName("internalDataverse")]
    public DataverseSecrets? InternalDataverse { get; set; }

    [JsonPropertyName("azureDevOps")]
    public AzureDevOpsSecrets? AzureDevOps { get; set; }

    [JsonPropertyName("projectOnline")]
    public DataverseSecrets? ProjectOnline { get; set; }
}

public class DataverseSecrets
{
    [JsonPropertyName("environmentUrl")]
    public string EnvironmentUrl { get; set; } = string.Empty;

    [JsonPropertyName("auth")]
    public AuthConfig Auth { get; set; } = new();
}

public class AzureDevOpsSecrets
{
    [JsonPropertyName("organizationUrl")]
    public string OrganizationUrl { get; set; } = string.Empty;

    [JsonPropertyName("project")]
    public string Project { get; set; } = string.Empty;

    [JsonPropertyName("auth")]
    public AuthConfig Auth { get; set; } = new();
}

public class AuthConfig
{
    /// <summary>"ClientCredentials" | "PersonalAccessToken"</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    // Client credentials fields
    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }

    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; set; }

    // Personal access token field
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
