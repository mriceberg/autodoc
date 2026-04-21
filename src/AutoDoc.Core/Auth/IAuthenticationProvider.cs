namespace AutoDoc.Core.Auth;

public interface IAuthenticationProvider
{
    Task<string> GetAccessTokenAsync(string resource, CancellationToken ct = default);
}
