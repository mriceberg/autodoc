using System.Net.Http.Headers;
using System.Text.Json;
using AutoDoc.Core.Auth;

namespace AutoDoc.Infrastructure.Http;

public class DataverseClient
{
    private const string ApiPath = "/api/data/v9.2/";

    private readonly HttpClient _http;
    private readonly IAuthenticationProvider _auth;
    private readonly string _environmentUrl;

    public DataverseClient(HttpClient http, IAuthenticationProvider auth, string environmentUrl)
    {
        _http = http;
        _auth = auth;
        _environmentUrl = environmentUrl.TrimEnd('/');
    }

    /// <summary>Fetches a single-record endpoint (e.g. organizations(id)).</summary>
    public async Task<JsonElement> GetSingleAsync(string relativeUrl, CancellationToken ct = default)
    {
        var response = await SendAsync(relativeUrl, ct);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.Clone();
    }

    /// <summary>Fetches all pages of a collection endpoint, following @odata.nextLink.</summary>
    public async Task<List<JsonElement>> GetCollectionAsync(string relativeUrl, CancellationToken ct = default)
    {
        var results = new List<JsonElement>();
        string? nextUrl = $"{_environmentUrl}{ApiPath}{relativeUrl}";

        while (nextUrl is not null)
        {
            var token = await _auth.GetAccessTokenAsync(_environmentUrl, ct);
            var request = BuildRequest(nextUrl, token);

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("value", out var valueArray))
                foreach (var item in valueArray.EnumerateArray())
                    results.Add(item.Clone());

            nextUrl = root.TryGetProperty("@odata.nextLink", out var next)
                ? next.GetString()
                : null;
        }

        return results;
    }

    private async Task<HttpResponseMessage> SendAsync(string relativeUrl, CancellationToken ct)
    {
        var token = await _auth.GetAccessTokenAsync(_environmentUrl, ct);
        var request = BuildRequest($"{_environmentUrl}{ApiPath}{relativeUrl}", token);
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private static HttpRequestMessage BuildRequest(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("OData-MaxVersion", "4.0");
        request.Headers.Add("OData-Version", "4.0");
        request.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
        return request;
    }
}
