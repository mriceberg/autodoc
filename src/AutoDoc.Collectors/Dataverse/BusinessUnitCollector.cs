using AutoDoc.Core.Collectors;
using AutoDoc.Core.Models.Dataverse;
using AutoDoc.Infrastructure.Http;

namespace AutoDoc.Collectors.Dataverse;

public class BusinessUnitCollector : DataverseCollectorBase, ICollector<BusinessUnitModel>
{
    private const string Query =
        "businessunits?" +
        "$select=businessunitid,name,description,websiteurl,emailaddress," +
        "address1_city,address1_country,isdisabled,parentbusinessunitid,createdon,modifiedon&" +
        "$expand=parentbusinessunitid($select=businessunitid,name)&" +
        "$orderby=name asc";

    public BusinessUnitCollector(DataverseClient client) : base(client) { }

    public async Task<IReadOnlyList<BusinessUnitModel>> FetchAsync(CancellationToken ct = default)
    {
        var labelsTask = FetchAttributeLabelsAsync("businessunit", ct);
        var items  = await Client.GetCollectionAsync(Query, ct);
        var labels = await labelsTask;

        return items.Select(el =>
        {
            el.TryGetProperty("parentbusinessunitid", out var parent);
            var parentKind = parent.ValueKind;

            return new BusinessUnitModel
            {
                BusinessUnitId         = G(el, "businessunitid") ?? Guid.Empty,
                Name                   = S(el, "name") ?? string.Empty,
                Description            = S(el, "description"),
                ParentBusinessUnitId   = parentKind == System.Text.Json.JsonValueKind.Object ? G(parent, "businessunitid") : null,
                ParentBusinessUnitName = parentKind == System.Text.Json.JsonValueKind.Object ? S(parent, "name") : null,
                IsRoot                 = parentKind != System.Text.Json.JsonValueKind.Object,
                WebsiteUrl             = S(el, "websiteurl"),
                EmailAddress           = S(el, "emailaddress"),
                Address1_City          = S(el, "address1_city"),
                Address1_Country       = S(el, "address1_country"),
                IsDisabled             = B(el, "isdisabled") ?? false,
                CreatedOn              = D(el, "createdon"),
                ModifiedOn             = D(el, "modifiedon"),
                Labels                 = labels
            };
        }).ToList();
    }
}
