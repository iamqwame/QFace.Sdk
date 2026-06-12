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

    // Returns country-specific default allowance types to seed on tenant setup.
    IReadOnlyList<AllowanceDefinition> GetAllowances();

    // Returns country-specific default deduction types to seed on tenant setup.
    // Should NOT include PAYE — that is seeded separately via TaxConfiguration.
    IReadOnlyList<DeductionDefinition> GetDeductions();

    // Returns country-specific default pay grades with salary bands in local currency.
    IReadOnlyList<GradeDefinition> GetGrades();

    // Returns country-specific banks to seed on tenant setup.
    IReadOnlyList<BankSeedDefinition> GetBanks();

    /// <summary>
    /// Returns country-specific employee identity document types.
    /// Passport is expected in every country's list.
    /// </summary>
    IReadOnlyList<DocumentTypeDefinition> GetDocumentTypes();

    // Returns the GL ledger code for the statutory pension contribution payable account.
    // tier: 1 = basic/mandatory, 2 = occupational/supplementary, 3 = voluntary
    string GetPensionLedgerCode(int tier);

    // Returns the GL ledger name for the pension payable account.
    string GetPensionLedgerName(int tier);
}
