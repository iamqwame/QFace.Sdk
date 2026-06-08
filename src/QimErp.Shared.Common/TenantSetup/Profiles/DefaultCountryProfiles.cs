namespace QimErp.Shared.Common.TenantSetup.Profiles;

/// <summary>
/// Nigeria stub — inherits Ghana statutory defaults.
/// TODO: Override GetPensionConfig (NSITF/PenCom rates), GetIncomeTaxBrackets (PAYE bands),
///       GetLeaveTypes (Employment Act 1990), and GetPublicHolidays when properly implemented.
/// </summary>
public sealed class NigeriaCountryProfile : GhanaCountryProfile
{
    public override string CountryCode => "NG";
    public override string CountryName => "Nigeria";
    public override string Currency => "NGN";
    public override string CurrencySymbol => "₦";

    public override IReadOnlyList<AllowanceDefinition> GetAllowances() =>
    [
        new("ALW-TRANS",  "Transport",      "Fixed",      15000m, false, "Standard transport allowance for office-based staff.",
            "6120-TRANSPORT-EXP",  "Transport Allowance Expense",
            "bus",       "var(--qim-info-bg)",    "var(--qim-sky)"),

        new("ALW-RENT",   "Housing",        "Percentage", 30m,    false, "30% of basic salary — paid as housing allowance.",
            "6110-HOUSING-EXP",    "Housing Allowance Expense",
            "home",      "rgb(237, 232, 254)",    "rgb(124, 58, 237)"),

        new("ALW-MEAL",   "Meal",           "Fixed",      5000m,  false, "Monthly meal subsidy for all staff.",
            "6140-MEAL-EXP",       "Meal Allowance Expense",
            "utensils",  "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("ALW-PHONE",  "Phone & Data",   "Fixed",      3000m,  false, "Phone bill reimbursement.",
            "6170-PHONE-EXP",      "Phone and Data Allowance Expense",
            "phone",     "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("ALW-RISK",   "Risk",           "Percentage", 10m,    false, "10% of basic — for warehouse, driving and field roles.",
            "6160-RISK-EXP",       "Risk Allowance Expense",
            "shield",    "var(--qim-danger-bg)",  "var(--qim-danger)"),
    ];

    public override IReadOnlyList<DeductionDefinition> GetDeductions() =>
    [
        // PAYE is excluded — seeded separately via TaxConfiguration.
        new("DED-NSITF",  "NSITF Contribution",    "Percentage", 1m,    true,  false, "National Social Insurance Trust Fund — 1% of basic.",
            "2100-NSITF-PAY",     "NSITF Contribution Payable",
            "shield-check", "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("DED-TIER1",  "PenCom Tier 1",         "Percentage", 8m,    true,  false, "Employee mandatory pension contribution — 8% of basic.",
            "2101-PENCOM-T1-PAY", "PenCom Tier 1 Payable",
            "piggy-bank",   "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("DED-TIER2",  "PenCom Tier 2",         "Percentage", 10m,   true,  false, "Employer mandatory pension contribution — 10% of basic.",
            "2102-PENCOM-T2-PAY", "PenCom Tier 2 Payable",
            "sparkles",     "rgb(237, 232, 254)",    "rgb(124, 58, 237)"),

        new("DED-LOAN",   "Loan Repayment",        "Fixed",      null,  false, false, "Automatic loan repayments by amortization.",
            "2300-LOAN-PAY",      "Loan Recovery Payable",
            "hand-coins",   "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-ADV",    "Salary Advance",        "Fixed",      null,  false, false, "Recovery of salary advances paid out.",
            "2310-ADVANCE-PAY",   "Salary Advance Recovery Payable",
            "banknote",     "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-UNION",  "Union Dues",            "Fixed",      2000m, false, false, "Workers' union monthly dues.",
            "2400-UNION-PAY",     "Union Dues Payable",
            "flag",         "rgb(255, 228, 230)",    "rgb(225, 29, 72)"),

        new("DED-HEALTH", "Health Insurance",      "Fixed",      5000m, false, false, "Employee share of private health cover.",
            "2410-HEALTH-PAY",    "Health Insurance Payable",
            "heart-pulse",  "var(--qim-danger-bg)",  "var(--qim-danger)"),

        new("DED-WELF",   "Staff Welfare",         "Fixed",      1000m, false, false, "Welfare contribution.",
            "2420-WELFARE-PAY",   "Staff Welfare Payable",
            "hand-heart",   "rgb(224, 242, 254)",    "var(--qim-sky)"),
    ];

    public override IReadOnlyList<GradeDefinition> GetGrades() =>
    [
        new("GRD-01", "Entry Level",      1,   900_000m,  1_400_000m, "Entry-level roles: graduates, trainees and support staff."),
        new("GRD-02", "Junior",           2, 1_400_000m,  2_100_000m, "Junior professionals with 1-3 years experience under supervision."),
        new("GRD-03", "Intermediate",     3, 2_100_000m,  3_250_000m, "Intermediate independent contributors with project ownership."),
        new("GRD-04", "Senior",           4, 3_250_000m,  4_750_000m, "Senior individual contributors, technical leads and specialists."),
        new("GRD-05", "Principal",        5, 4_750_000m,  6_500_000m, "Principal experts and team leads with cross-functional scope."),
        new("GRD-06", "Manager",          6, 6_500_000m, 10_000_000m, "People managers with budget and team responsibility."),
        new("GRD-07", "Senior Manager",   7,10_000_000m, 15_000_000m, "Senior managers with multi-team scope and strategic planning."),
        new("GRD-08", "Executive",        8,15_000_000m, 30_000_000m, "Executive level: VP and above with company-wide accountability."),
    ];

    // Nigerian banks to be seeded in a future iteration.
    public override IReadOnlyList<BankSeedDefinition> GetBanks() => [];

    public override string GetPensionLedgerCode(int tier) => tier switch
    {
        1 => "2100-NSITF-PAY",
        2 => "2101-PENCOM-T1-PAY",
        3 => "2102-PENCOM-T2-PAY",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };

    public override string GetPensionLedgerName(int tier) => tier switch
    {
        1 => "NSITF Contribution Payable",
        2 => "PenCom Tier 1 Payable",
        3 => "PenCom Tier 2 Payable",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };
}

/// <summary>
/// Kenya stub — inherits Ghana statutory defaults.
/// TODO: Override GetPensionConfig (NSSF rates), GetIncomeTaxBrackets (KRA PAYE bands),
///       GetLeaveTypes (Employment Act 2007), and GetPublicHolidays when properly implemented.
/// </summary>
public sealed class KenyaCountryProfile : GhanaCountryProfile
{
    public override string CountryCode => "KE";
    public override string CountryName => "Kenya";
    public override string Currency => "KES";
    public override string CurrencySymbol => "KSh";

    public override IReadOnlyList<AllowanceDefinition> GetAllowances() =>
    [
        new("ALW-TRANS",  "Transport",      "Fixed",      5000m,  false, "Standard transport allowance for office-based staff.",
            "6120-TRANSPORT-EXP",  "Transport Allowance Expense",
            "bus",       "var(--qim-info-bg)",    "var(--qim-sky)"),

        new("ALW-RENT",   "Housing",        "Percentage", 15m,    false, "15% of basic salary — paid as housing allowance.",
            "6110-HOUSING-EXP",    "Housing Allowance Expense",
            "home",      "rgb(237, 232, 254)",    "rgb(124, 58, 237)"),

        new("ALW-MEAL",   "Meal",           "Fixed",      1500m,  false, "Monthly meal subsidy for all staff.",
            "6140-MEAL-EXP",       "Meal Allowance Expense",
            "utensils",  "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("ALW-PHONE",  "Phone & Data",   "Fixed",      2000m,  false, "Phone bill reimbursement.",
            "6170-PHONE-EXP",      "Phone and Data Allowance Expense",
            "phone",     "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("ALW-RISK",   "Risk",           "Percentage", 10m,    false, "10% of basic — for field and hazardous roles.",
            "6160-RISK-EXP",       "Risk Allowance Expense",
            "shield",    "var(--qim-danger-bg)",  "var(--qim-danger)"),
    ];

    public override IReadOnlyList<DeductionDefinition> GetDeductions() =>
    [
        // PAYE is excluded — seeded separately via TaxConfiguration.
        new("DED-NSSF",   "NSSF Contribution",     "Fixed",      200m,  true,  false, "National Social Security Fund — flat KES 200/month.",
            "2100-NSSF-PAY",      "NSSF Employee Payable",
            "shield-check", "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("DED-NHIF",   "NHIF / SHA",            "Fixed",      1700m, true,  false, "National Hospital Insurance Fund contribution.",
            "2101-NHIF-PAY",      "NHIF Payable",
            "heart-pulse",  "var(--qim-danger-bg)",  "var(--qim-danger)"),

        new("DED-LOAN",   "Loan Repayment",        "Fixed",      null,  false, false, "Automatic loan repayments by amortization.",
            "2300-LOAN-PAY",      "Loan Recovery Payable",
            "hand-coins",   "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-ADV",    "Salary Advance",        "Fixed",      null,  false, false, "Recovery of salary advances paid out.",
            "2310-ADVANCE-PAY",   "Salary Advance Recovery Payable",
            "banknote",     "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-UNION",  "Union Dues",            "Fixed",      500m,  false, false, "Workers' union monthly dues.",
            "2400-UNION-PAY",     "Union Dues Payable",
            "flag",         "rgb(255, 228, 230)",    "rgb(225, 29, 72)"),

        new("DED-WELF",   "Staff Welfare",         "Fixed",      300m,  false, false, "Welfare contribution.",
            "2420-WELFARE-PAY",   "Staff Welfare Payable",
            "hand-heart",   "rgb(224, 242, 254)",    "var(--qim-sky)"),
    ];

    public override IReadOnlyList<GradeDefinition> GetGrades() =>
    [
        new("GRD-01", "Entry Level",      1,  25_000m,  40_000m, "Entry-level roles: graduates, trainees and support staff."),
        new("GRD-02", "Junior",           2,  40_000m,  65_000m, "Junior professionals with 1-3 years experience under supervision."),
        new("GRD-03", "Intermediate",     3,  65_000m, 100_000m, "Intermediate independent contributors with project ownership."),
        new("GRD-04", "Senior",           4, 100_000m, 150_000m, "Senior individual contributors, technical leads and specialists."),
        new("GRD-05", "Principal",        5, 150_000m, 210_000m, "Principal experts and team leads with cross-functional scope."),
        new("GRD-06", "Manager",          6, 210_000m, 300_000m, "People managers with budget and team responsibility."),
        new("GRD-07", "Senior Manager",   7, 300_000m, 450_000m, "Senior managers with multi-team scope and strategic planning."),
        new("GRD-08", "Executive",        8, 450_000m, 800_000m, "Executive level: VP and above with company-wide accountability."),
    ];

    // Kenyan banks to be seeded in a future iteration.
    public override IReadOnlyList<BankSeedDefinition> GetBanks() => [];

    public override string GetPensionLedgerCode(int tier) => tier switch
    {
        1 => "2100-NSSF-PAY",
        2 => "2101-NSSF-EMP-PAY",
        3 => "2102-PENSION-VOL-PAY",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };

    public override string GetPensionLedgerName(int tier) => tier switch
    {
        1 => "NSSF Employee Payable",
        2 => "NSSF Employer Payable",
        3 => "Voluntary Pension Payable",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };
}

/// <summary>
/// Tanzania stub — inherits Ghana statutory defaults.
/// TODO: Override GetPensionConfig (NSSF/PPF rates), GetIncomeTaxBrackets (TRA PAYE bands),
///       GetLeaveTypes (Employment and Labour Relations Act 2004), and GetPublicHolidays when properly implemented.
/// </summary>
public sealed class TanzaniaCountryProfile : GhanaCountryProfile
{
    public override string CountryCode => "TZ";
    public override string CountryName => "Tanzania";
    public override string Currency => "TZS";
    public override string CurrencySymbol => "TSh";

    public override IReadOnlyList<AllowanceDefinition> GetAllowances() =>
    [
        new("ALW-TRANS",  "Transport",      "Fixed",      50_000m,  false, "Standard transport allowance for office-based staff.",
            "6120-TRANSPORT-EXP",  "Transport Allowance Expense",
            "bus",       "var(--qim-info-bg)",    "var(--qim-sky)"),

        new("ALW-RENT",   "Housing",        "Percentage", 15m,      false, "15% of basic salary — paid as housing allowance.",
            "6110-HOUSING-EXP",    "Housing Allowance Expense",
            "home",      "rgb(237, 232, 254)",    "rgb(124, 58, 237)"),

        new("ALW-MEAL",   "Meal",           "Fixed",      15_000m,  false, "Monthly meal subsidy for all staff.",
            "6140-MEAL-EXP",       "Meal Allowance Expense",
            "utensils",  "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("ALW-PHONE",  "Phone & Data",   "Fixed",      20_000m,  false, "Phone bill reimbursement.",
            "6170-PHONE-EXP",      "Phone and Data Allowance Expense",
            "phone",     "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("ALW-RISK",   "Risk",           "Percentage", 10m,      false, "10% of basic — for field and hazardous roles.",
            "6160-RISK-EXP",       "Risk Allowance Expense",
            "shield",    "var(--qim-danger-bg)",  "var(--qim-danger)"),
    ];

    public override IReadOnlyList<DeductionDefinition> GetDeductions() =>
    [
        // PAYE is excluded — seeded separately via TaxConfiguration.
        new("DED-NSSF",   "NSSF Contribution",     "Percentage", 10m,   true,  false, "National Social Security Fund — 10% of basic salary.",
            "2100-NSSF-PAY",      "NSSF Employee Payable",
            "shield-check", "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("DED-SDL",    "SDL",                   "Percentage", 4.5m,  true,  false, "Skills Development Levy — 4.5% of gross payroll.",
            "2101-SDL-PAY",       "SDL Payable",
            "graduation-cap","rgb(237, 232, 254)",  "rgb(124, 58, 237)"),

        new("DED-LOAN",   "Loan Repayment",        "Fixed",      null,  false, false, "Automatic loan repayments by amortization.",
            "2300-LOAN-PAY",      "Loan Recovery Payable",
            "hand-coins",   "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-ADV",    "Salary Advance",        "Fixed",      null,  false, false, "Recovery of salary advances paid out.",
            "2310-ADVANCE-PAY",   "Salary Advance Recovery Payable",
            "banknote",     "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-UNION",  "Union Dues",            "Fixed",      5_000m,false, false, "Workers' union monthly dues.",
            "2400-UNION-PAY",     "Union Dues Payable",
            "flag",         "rgb(255, 228, 230)",    "rgb(225, 29, 72)"),

        new("DED-WELF",   "Staff Welfare",         "Fixed",      3_000m,false, false, "Welfare contribution.",
            "2420-WELFARE-PAY",   "Staff Welfare Payable",
            "hand-heart",   "rgb(224, 242, 254)",    "var(--qim-sky)"),
    ];

    public override IReadOnlyList<GradeDefinition> GetGrades() =>
    [
        new("GRD-01", "Entry Level",      1,   250_000m,   400_000m, "Entry-level roles: graduates, trainees and support staff."),
        new("GRD-02", "Junior",           2,   400_000m,   650_000m, "Junior professionals with 1-3 years experience under supervision."),
        new("GRD-03", "Intermediate",     3,   650_000m, 1_000_000m, "Intermediate independent contributors with project ownership."),
        new("GRD-04", "Senior",           4, 1_000_000m, 1_500_000m, "Senior individual contributors, technical leads and specialists."),
        new("GRD-05", "Principal",        5, 1_500_000m, 2_100_000m, "Principal experts and team leads with cross-functional scope."),
        new("GRD-06", "Manager",          6, 2_100_000m, 3_000_000m, "People managers with budget and team responsibility."),
        new("GRD-07", "Senior Manager",   7, 3_000_000m, 4_500_000m, "Senior managers with multi-team scope and strategic planning."),
        new("GRD-08", "Executive",        8, 4_500_000m, 8_000_000m, "Executive level: VP and above with company-wide accountability."),
    ];

    // Tanzanian banks to be seeded in a future iteration.
    public override IReadOnlyList<BankSeedDefinition> GetBanks() => [];

    public override string GetPensionLedgerCode(int tier) => tier switch
    {
        1 => "2100-NSSF-PAY",
        2 => "2101-PPF-PAY",
        3 => "2102-PENSION-VOL-PAY",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };

    public override string GetPensionLedgerName(int tier) => tier switch
    {
        1 => "NSSF Employee Payable",
        2 => "PPF Employer Payable",
        3 => "Voluntary Pension Payable",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };
}
