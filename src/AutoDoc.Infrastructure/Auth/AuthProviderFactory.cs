using AutoDoc.Core.Auth;

namespace AutoDoc.Infrastructure.Auth;

public static class AuthProviderFactory
{
    public static IAuthenticationProvider Create(AuthConfig config) =>
        config.Type switch
        {
            "ClientCredentials" => new ClientCredentialsAuthProvider(config),
            "PersonalAccessToken" => new PersonalAccessTokenAuthProvider(config),
            _ => throw new NotSupportedException($"Unsupported auth type: '{config.Type}'")
        };
}
