using System.Text;
using AutoDoc.Core.Auth;

namespace AutoDoc.Infrastructure.Auth;

/// <summary>
/// Encodes a PAT as a Basic auth header (used by Azure DevOps).
/// </summary>
public class PersonalAccessTokenAuthProvider : IAuthenticationProvider
{
    private readonly string _encodedToken;

    public PersonalAccessTokenAuthProvider(AuthConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Token))
            throw new ArgumentException("Token is required for PersonalAccessToken auth.");

        // Azure DevOps expects Base64(":{pat}")
        _encodedToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{config.Token}"));
    }

    // Resource is ignored for PAT — the token is scoped at creation time.
    public Task<string> GetAccessTokenAsync(string resource, CancellationToken ct = default)
        => Task.FromResult(_encodedToken);
}
