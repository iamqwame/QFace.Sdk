namespace QimErp.Shared.Common.TenantSetup;

public interface ICountrySetupProfile
{
    string CountryCode { get; }           // ISO 3166-1 alpha-2: "GH", "NG", "KE"
    string CountryName { get; }
    string Currency { get; }              // "GHS", "NGN", "KES"
    string CurrencySymbol { get; }        // "₵", "₦", "KSh"

    StatutoryPensionConfig GetPensionConfig();
    IReadOnlyList<TaxBracketDefinition> GetIncomeTaxBrackets();
    PayrollSettingsDefaults GetPayrollDefaults();
    IReadOnlyList<LeaveTypeDefinition> GetLeaveTypes();
    IReadOnlyList<PublicHolidayDefinition> GetPublicHolidays(int year);
}
