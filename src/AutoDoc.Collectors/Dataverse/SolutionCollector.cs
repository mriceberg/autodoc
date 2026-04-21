using AutoDoc.Core.Collectors;
using AutoDoc.Core.Models.Dataverse;
using AutoDoc.Infrastructure.Http;

namespace AutoDoc.Collectors.Dataverse;

public class SolutionCollector : DataverseCollectorBase, ICollector<SolutionModel>
{
    private const string Query =
        "solutions?" +
        "$select=solutionid,uniquename,friendlyname,description,version,ismanaged,isvisible,installedon,modifiedon&" +
        "$expand=publisherid($select=publisherid,uniquename,friendlyname,customizationprefix)&" +
        "$filter=isvisible eq true&" +
        "$orderby=friendlyname asc";

    public SolutionCollector(DataverseClient client) : base(client) { }

    public async Task<IReadOnlyList<SolutionModel>> FetchAsync(CancellationToken ct = default)
    {
        var items = await Client.GetCollectionAsync(Query, ct);

        return items.Select(el =>
        {
            el.TryGetProperty("publisherid", out var pub);
            var pubKind = pub.ValueKind;

            return new SolutionModel
            {
                SolutionId      = G(el, "solutionid") ?? Guid.Empty,
                UniqueName      = S(el, "uniquename") ?? string.Empty,
                FriendlyName    = S(el, "friendlyname") ?? string.Empty,
                Description     = S(el, "description"),
                Version         = S(el, "version") ?? string.Empty,
                IsManaged       = B(el, "ismanaged") ?? false,
                IsVisible       = B(el, "isvisible") ?? true,
                PublisherId     = pubKind == System.Text.Json.JsonValueKind.Object ? G(pub, "publisherid") : null,
                PublisherName   = pubKind == System.Text.Json.JsonValueKind.Object ? S(pub, "friendlyname") : null,
                PublisherPrefix = pubKind == System.Text.Json.JsonValueKind.Object ? S(pub, "customizationprefix") : null,
                InstalledOn     = D(el, "installedon"),
                ModifiedOn      = D(el, "modifiedon")
            };
        }).ToList();
    }
}
