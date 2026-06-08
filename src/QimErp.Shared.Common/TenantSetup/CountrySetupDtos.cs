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

/// <summary>
/// Defines a default allowance type to seed when a tenant is set up.
/// <see cref="CalculationType"/> is a string ("Fixed", "Percentage", "PerUnit") to avoid
/// cross-project enum coupling between the SDK and individual payroll modules.
/// </summary>
public sealed record AllowanceDefinition(
    string Code,
    string Name,
    string CalculationType,
    decimal? DefaultAmount,
    bool IsTaxable,
    string? Description,
    string? ExpenseAccountCode,
    string? ExpenseAccountName,
    string? IconName,
    string? IconBackground,
    string? IconForeground);

/// <summary>
/// Defines a default deduction type to seed when a tenant is set up.
/// PAYE/income-tax deductions should NOT be included here — they are seeded
/// separately through <see cref="ICountrySetupProfile.GetIncomeTaxBrackets"/>.
/// </summary>
public sealed record DeductionDefinition(
    string Code,
    string Name,
    string CalculationType,
    decimal? DefaultAmount,
    bool IsStatutory,
    bool IsTaxable,
    string? Description,
    string? PayableAccountCode,
    string? PayableAccountName,
    string? IconName,
    string? IconBackground,
    string? IconForeground);

/// <summary>
/// Defines a pay grade with a salary band in the country's local currency.
/// </summary>
public sealed record GradeDefinition(
    string Code,
    string Name,
    int Level,
    decimal MinSalary,
    decimal MaxSalary,
    string? Description);

/// <summary>
/// Defines a bank record to seed when a tenant is set up for a given country.
/// </summary>
public sealed record BankSeedDefinition(
    string Code,
    string Name,
    string ShortName,
    string Category,
    string Phone,
    string? Website,
    string? Email,
    string HeadOfficeAddress,
    int DisplayOrder);
