using AutoDoc.Core.Auth;
using Microsoft.Identity.Client;

namespace AutoDoc.Infrastructure.Auth;

public class ClientCredentialsAuthProvider : IAuthenticationProvider
{
    private readonly IConfidentialClientApplication _app;

    public ClientCredentialsAuthProvider(AuthConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.TenantId))
            throw new ArgumentException("TenantId is required for ClientCredentials auth.");
        if (string.IsNullOrWhiteSpace(config.ClientId))
            throw new ArgumentException("ClientId is required for ClientCredentials auth.");
        if (string.IsNullOrWhiteSpace(config.ClientSecret))
            throw new ArgumentException("ClientSecret is required for ClientCredentials auth.");

        _app = ConfidentialClientApplicationBuilder
            .Create(config.ClientId)
            .WithClientSecret(config.ClientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{config.TenantId}")
            .Build();
    }

    public async Task<string> GetAccessTokenAsync(string resource, CancellationToken ct = default)
    {
        var scopes = new[] { $"{resource.TrimEnd('/')}/.default" };
        var result = await _app
            .AcquireTokenForClient(scopes)
            .ExecuteAsync(ct);
        return result.AccessToken;
    }
}
