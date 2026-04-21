using AutoDoc.Core.Collectors;
using AutoDoc.Core.Models.Dataverse;
using AutoDoc.Infrastructure.Http;

namespace AutoDoc.Collectors.Dataverse;

public class OrganizationCollector : DataverseCollectorBase, ICollector<OrganizationModel>
{
    private static readonly string[] MappedFields =
    [
        "organizationid", "name", "languagecode", "localeid",
        "dateformatstring", "timeformatstring", "defaultcountrycode",
        "basecurrencysymbol", "basecurrencyid", "currencyformatcode",
        "isauditenabled", "isuseraccessauditenabled", "isreadauditenabled",
        "auditretentionperiodv2", "ismobileofflineenabled", "ispresenceenabled",
        "issopintegrationenabled", "isautosaveenabled",
        "maxuploadfilesize", "schemanameprefix", "version",
        "createdon", "modifiedon"
    ];

    private const string Query =
        "organizations?" +
        "$select=organizationid,name,languagecode,localeid," +
        "dateformatstring,timeformatstring,defaultcountrycode," +
        "basecurrencysymbol,currencyformatcode," +
        "isauditenabled,isuseraccessauditenabled,isreadauditenabled,auditretentionperiodv2," +
        "ismobileofflineenabled,ispresenceenabled,issopintegrationenabled,isautosaveenabled," +
        "maxuploadfilesize,version,createdon,modifiedon";

    public OrganizationCollector(DataverseClient client) : base(client) { }

    public async Task<IReadOnlyList<OrganizationModel>> FetchAsync(CancellationToken ct = default)
    {
        var labelsTask = FetchAttributeLabelsAsync("organization", ct);
        var items  = await Client.GetCollectionAsync(Query, ct);
        var labels = await labelsTask;

        return items.Select(el => new OrganizationModel
        {
            OrganizationId              = G(el, "organizationid") ?? Guid.Empty,
            Name                        = S(el, "name") ?? string.Empty,
            LanguageCode                = I(el, "languagecode") ?? 0,
            LocaleId                    = I(el, "localeid") ?? 0,
            DateFormatString            = S(el, "dateformatstring"),
            TimeFormatString            = S(el, "timeformatstring"),
            DefaultCountryCode          = S(el, "defaultcountrycode"),
            BaseCurrencySymbol          = S(el, "basecurrencysymbol"),
            BaseCurrencyCode            = S(el, "currencyformatcode"),
            IsAuditEnabled              = B(el, "isauditenabled"),
            IsUserAccessAuditEnabled    = B(el, "isuseraccessauditenabled"),
            IsReadAuditEnabled          = B(el, "isreadauditenabled"),
            AuditRetentionPeriodV2      = I(el, "auditretentionperiodv2"),
            IsMobileOfflineEnabled      = B(el, "ismobileofflineenabled"),
            IsPresenceEnabled           = B(el, "ispresenceenabled"),
            IsSOPIntegrationEnabled     = B(el, "issopintegrationenabled"),
            IsAutoSaveEnabled           = B(el, "isautosaveenabled"),
            MaxUploadFileSizeInBytes    = I(el, "maxuploadfilesize"),
            Version                     = S(el, "version"),
            CreatedOn                   = D(el, "createdon"),
            ModifiedOn                  = D(el, "modifiedon"),
            AdditionalSettings          = AdditionalProperties(el, MappedFields),
            Labels                      = labels
        }).ToList();
    }
}
