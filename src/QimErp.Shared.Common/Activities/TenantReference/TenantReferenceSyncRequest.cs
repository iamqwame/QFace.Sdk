namespace QimErp.Shared.Common.Activities.TenantReference;

public enum TenantReferenceEntityType
{
    Currency,
    ExchangeRate,
    Calendar,
    Holiday,
    Bank
}

public enum TenantReferenceSyncOperation
{
    Created,
    Updated,
    Deleted
}

/// <summary>
/// Payload for <see cref="ITenantReferenceSyncWorkflow"/> fan-out activities.
/// IAM is the master; modules upsert local read models from this payload.
/// </summary>
public class TenantReferenceSyncRequest
{
    public TenantReferenceEntityType EntityType { get; set; }
    public TenantReferenceSyncOperation Operation { get; set; }
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Resolved by orchestrator from IAM when not supplied by caller.</summary>
    public List<string>? SelectedModules { get; set; }

    public Guid EntityId { get; set; }

    // Currency
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public bool IsBaseCurrency { get; set; }
    public bool IsEnabled { get; set; }
    public int DecimalPlaces { get; set; } = 2;

    // Exchange rate
    public string? FromCurrency { get; set; }
    public string? ToCurrency { get; set; }
    public decimal Rate { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public string? Source { get; set; }

    // Calendar
    public string? WorkingDays { get; set; }
    public string? WorkHoursStart { get; set; }
    public string? WorkHoursEnd { get; set; }
    public string? DateFormat { get; set; }
    public string? DefaultPhonePrefix { get; set; }
    public string? FiscalYearStart { get; set; }
    public string? LeaveYearStart { get; set; }

    // Holiday
    public string? HolidayName { get; set; }
    public DateOnly? HolidayDate { get; set; }
    public string? HolidayType { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }

    // Bank
    public string? BankCode { get; set; }
    public string? BankName { get; set; }
    public string? BankShortName { get; set; }
    public string? BankCategory { get; set; }
    public string? BankCountryCode { get; set; }
    public string? BankSwiftCode { get; set; }
    public string? BankSortCode { get; set; }
    public string? BankLogoUrl { get; set; }
    public string? BankWebsite { get; set; }
    public string? BankPhoneNumber { get; set; }
    public string? BankEmail { get; set; }
    public string? BankHeadOfficeAddress { get; set; }
    public int BankDisplayOrder { get; set; }
    public bool BankIsSystemDefault { get; set; }
}
