namespace QimErp.Shared.Common.TenantSetup;

public sealed record StatutoryPensionConfig(
    string PensionAuthorityName,
    decimal EmployeeRateTier1, decimal EmployerRateTier1,
    decimal EmployeeRateTier2, decimal EmployerRateTier2,
    decimal EmployeeRateTier3, decimal EmployerRateTier3,
    string Tier1Code, string Tier2Code, string Tier3Code,
    string Tier1TrusteeName, string Tier2TrusteeName,
    decimal? AnnualContributionCeiling);

/// <summary>
/// Defines a single PAYE tax band as a slab width + rate pair,
/// matching the TaxBracket entity model used by the payroll engine.
/// The last band typically has <see cref="SlabWidth"/> = 0 to indicate "remainder / unlimited".
/// </summary>
public sealed record TaxBracketDefinition(
    decimal SlabWidth,
    decimal Rate);

public sealed record PayrollSettingsDefaults(
    string Currency, string CurrencySymbol,
    int DefaultWorkingDaysPerWeek, int DefaultWorkingHoursPerDay,
    bool IsMonthly, string TaxAuthorityName);

public sealed record LeaveTypeDefinition(
    string Name, string Code, int EntitlementDays,
    bool WomenOnly, string Icon, string Category,
    bool IsEarned, bool CarryOverAllowed, int? CarryOverDays,
    int? MaxConsecutiveDays, int? MinAdvanceNoticeDays, int? MaxPerRequest,
    string Description, string Rules, string Eligibility,
    string Documentation, string ApprovalProcess);

public sealed record PublicHolidayDefinition(
    string Name, int Month, int Day, bool IsFixed,
    string? Note = null);
