namespace AutoDoc.Core.Models.Dataverse;

public record OrganizationModel
{
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;

    // Localization
    public int LanguageCode { get; init; }
    public int LocaleId { get; init; }
    public string? DateFormatString { get; init; }
    public string? TimeFormatString { get; init; }
    public string? DefaultCountryCode { get; init; }

    // Currency
    public string? BaseCurrencySymbol { get; init; }
    public string? BaseCurrencyCode { get; init; }
    public string? BaseCurrencyName { get; init; }

    // Auditing
    public bool? IsAuditEnabled { get; init; }
    public bool? IsUserAccessAuditEnabled { get; init; }
    public bool? IsReadAuditEnabled { get; init; }
    public int? AuditRetentionPeriodV2 { get; init; }

    // Features
    public bool? IsMobileOfflineEnabled { get; init; }
    public bool? IsPresenceEnabled { get; init; }
    public bool? IsSOPIntegrationEnabled { get; init; }
    public bool? IsAutoSaveEnabled { get; init; }
    public bool? IsRelationshipInsightsEnabled { get; init; }

    // Limits
    public int? MaxUploadFileSizeInBytes { get; init; }

    // Versioning
    public string? SchemaName { get; init; }
    public string? Version { get; init; }

    // Timestamps
    public DateTimeOffset? CreatedOn { get; init; }
    public DateTimeOffset? ModifiedOn { get; init; }

    // All remaining settings captured as-is from the API
    public Dictionary<string, string?> AdditionalSettings { get; init; } = [];
}
