using AutoDoc.Core.Collectors;
using AutoDoc.Core.Models.Dataverse;
using AutoDoc.Infrastructure.Http;

namespace AutoDoc.Collectors.Dataverse;

public class PublisherCollector : DataverseCollectorBase, ICollector<PublisherModel>
{
    private const string Query =
        "publishers?" +
        "$select=publisherid,uniquename,friendlyname,description," +
        "customizationprefix,customizationoptionvalueprefix," +
        "supportingwebsiteurl,emailaddress,isreadonly,modifiedon&" +
        "$orderby=friendlyname asc";

    public PublisherCollector(DataverseClient client) : base(client) { }

    public async Task<IReadOnlyList<PublisherModel>> FetchAsync(CancellationToken ct = default)
    {
        var items = await Client.GetCollectionAsync(Query, ct);

        return items.Select(el => new PublisherModel
        {
            PublisherId                      = G(el, "publisherid") ?? Guid.Empty,
            UniqueName                       = S(el, "uniquename") ?? string.Empty,
            FriendlyName                     = S(el, "friendlyname") ?? string.Empty,
            Description                      = S(el, "description"),
            CustomizationPrefix              = S(el, "customizationprefix"),
            CustomizationOptionValuePrefix   = I(el, "customizationoptionvalueprefix"),
            SupportingWebsiteUrl             = S(el, "supportingwebsiteurl"),
            EMailAddress                     = S(el, "emailaddress"),
            IsReadonly                       = B(el, "isreadonly") ?? false,
            ModifiedOn                       = D(el, "modifiedon")
        }).ToList();
    }
}
