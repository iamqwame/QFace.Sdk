namespace QimErp.Shared.Common.TenantSetup.Profiles;

/// <summary>
/// Nigeria country setup profile — FIRS PAYE 2024, PenCom/NSITF pension,
/// Labour Act 1990 leave entitlements, and statutory public holidays.
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

    public override IReadOnlyList<DocumentTypeDefinition> GetDocumentTypes() =>
    [
        new("PASSPORT",      "Passport",       1),
        new("NIN",           "NIN",            2),
        new("BVN",           "BVN",            3),
        new("TIN",           "TIN",            4),
        new("VOTERID",       "Voter Card",     5),
        new("DRIVERLICENSE", "Driver License", 6),
    ];

    public override StatutoryPensionConfig GetPensionConfig() => new(
        PensionAuthorityName:      "Pencom / NSITF",
        EmployeeRateTier1:         8m,
        EmployerRateTier1:         10m,
        EmployeeRateTier2:         0m,
        EmployerRateTier2:         0m,
        EmployeeRateTier3:         0m,
        EmployerRateTier3:         0m,
        Tier1Code:                 "DED-TIER1",
        Tier2Code:                 "DED-TIER2",
        Tier3Code:                 "DED-TIER3",
        Tier1TrusteeName:          "Pencom",
        Tier2TrusteeName:          string.Empty,
        AnnualContributionCeiling: null);

    public override IReadOnlyList<TaxBracketDefinition> GetIncomeTaxBrackets() =>
    [
        // Nigeria FIRS PAYE bands — chargeable income slab widths per year (NGN), 2024.
        // SlabWidth=0 on the last band means "remainder / unlimited".
        new(300_000m,   7m),
        new(300_000m,  11m),
        new(500_000m,  15m),
        new(500_000m,  19m),
        new(1_600_000m, 21m),
        new(0m,         24m),
    ];

    public override IReadOnlyList<LeaveTypeDefinition> GetLeaveTypes() =>
    [
        new(
            Name:                "Annual Leave",
            Code:                "ANNUAL",
            EntitlementDays:     21,
            WomenOnly:           false,
            Icon:                "🌴",
            Category:            "Time off",
            IsEarned:            true,
            CarryOverAllowed:    true,
            CarryOverDays:       5,
            MaxConsecutiveDays:  15,
            MinAdvanceNoticeDays: 7,
            MaxPerRequest:       15,
            Description:         "Paid annual leave per the Nigerian Labour Act (min. 6 working days; most employers offer 21).",
            Rules:               "21 working days per year after 12 months of continuous service. Pro-rated for partial years.",
            Eligibility:         "All confirmed staff. Probationers after 6 months at manager discretion.",
            Documentation:       "Not required.",
            ApprovalProcess:     "Line manager approves; HR notified."),

        new(
            Name:                "Sick Leave",
            Code:                "SICK",
            EntitlementDays:     12,
            WomenOnly:           false,
            Icon:                "🤒",
            Category:            "Health",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  12,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       12,
            Description:         "Paid sick leave when medically unfit to work.",
            Rules:               "Up to 12 working days per year. Medical certificate required for absences > 2 consecutive days.",
            Eligibility:         "All staff including probationers.",
            Documentation:       "Medical certificate from a licensed hospital for absences > 2 days.",
            ApprovalProcess:     "Notify line manager same day; HR records; certificate attached."),

        new(
            Name:                "Maternity Leave",
            Code:                "MATERNITY",
            EntitlementDays:     84,
            WomenOnly:           true,
            Icon:                "🤰",
            Category:            "Parental",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  84,
            MinAdvanceNoticeDays: 14,
            MaxPerRequest:       84,
            Description:         "Paid maternity leave — 12 weeks at full pay per the Nigerian Labour Act.",
            Rules:               "12 weeks (84 days) at full pay.",
            Eligibility:         "All permanent female employees.",
            Documentation:       "Medical certificate confirming expected date of delivery.",
            ApprovalProcess:     "Line manager + HR; cover plan attached."),

        new(
            Name:                "Paternity Leave",
            Code:                "PATERNITY",
            EntitlementDays:     5,
            WomenOnly:           false,
            Icon:                "👶",
            Category:            "Parental",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  5,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       5,
            Description:         "Short paid leave around the birth of an employee's child.",
            Rules:               "Up to 5 working days within 30 days of birth.",
            Eligibility:         "All male employees.",
            Documentation:       "Birth notification.",
            ApprovalProcess:     "Line manager approves; HR records."),

        new(
            Name:                "Study / Exam Leave",
            Code:                "STUDY",
            EntitlementDays:     3,
            WomenOnly:           false,
            Icon:                "📚",
            Category:            "Development",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  3,
            MinAdvanceNoticeDays: 14,
            MaxPerRequest:       3,
            Description:         "Paid leave to sit examinations relevant to the employee's role.",
            Rules:               "Up to 3 working days per year. Subject to L&D approval.",
            Eligibility:         "Confirmed staff with at least 12 months service.",
            Documentation:       "Exam timetable or schedule attached to request.",
            ApprovalProcess:     "Line manager + L&D approve; HR records."),

        new(
            Name:                "Emergency / Compassionate Leave",
            Code:                "COMPASSIONATE",
            EntitlementDays:     3,
            WomenOnly:           false,
            Icon:                "🫂",
            Category:            "Family",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  3,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       3,
            Description:         "Paid leave on the death of an immediate family member or major family emergency.",
            Rules:               "Up to 3 working days per occurrence.",
            Eligibility:         "All staff.",
            Documentation:       "Death certificate or supporting evidence on return.",
            ApprovalProcess:     "Notify line manager; HR records."),
    ];

    public override IReadOnlyList<PublicHolidayDefinition> GetPublicHolidays(int year) =>
    [
        new("New Year's Day",   1,  1,  true,  "Public holiday — start of the calendar year."),
        new("Workers' Day",     5,  1,  true,  "International Labour Day."),
        new("Democracy Day",    6,  12, true,  "Commemorates Nigeria's return to democracy (1993/1999)."),
        new("Independence Day", 10, 1,  true,  "Commemorates Nigeria's independence from Britain (1960)."),
        new("Christmas Day",    12, 25, true,  "Christian observance — birth of Christ."),
        new("Boxing Day",       12, 26, true,  "Public holiday observed after Christmas Day."),
    ];
}

/// <summary>
/// Kenya country setup profile — KRA PAYE 2024, NSSF pension,
/// Employment Act 2007 leave entitlements, and statutory public holidays.
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

    public override IReadOnlyList<DocumentTypeDefinition> GetDocumentTypes() =>
    [
        new("PASSPORT",      "Passport",       1),
        new("NATIONAL_ID",   "National ID",    2),
        new("KRA_PIN",       "KRA PIN",        3),
        new("NSSF",          "NSSF Card",      4),
        new("NHIF",          "NHIF Card",      5),
        new("DRIVERLICENSE", "Driver License", 6),
    ];

    public override StatutoryPensionConfig GetPensionConfig() => new(
        // NSSF Act 2013: flat KES 200/month employee + KES 200/month employer.
        // Rates stored as 0 because the deduction is flat (not percentage).
        // Flat amounts are seeded separately via GetDeductions (DED-NSSF).
        PensionAuthorityName:      "NSSF",
        EmployeeRateTier1:         0m,
        EmployerRateTier1:         0m,
        EmployeeRateTier2:         0m,
        EmployerRateTier2:         0m,
        EmployeeRateTier3:         0m,
        EmployerRateTier3:         0m,
        Tier1Code:                 "DED-NSSF",
        Tier2Code:                 string.Empty,
        Tier3Code:                 string.Empty,
        Tier1TrusteeName:          "NSSF",
        Tier2TrusteeName:          string.Empty,
        AnnualContributionCeiling: null);

    public override IReadOnlyList<TaxBracketDefinition> GetIncomeTaxBrackets() =>
    [
        // Kenya KRA PAYE bands — chargeable income slab widths per month (KES), 2024.
        // SlabWidth=0 on the last band means "remainder / unlimited".
        new(24_000m,  10m),
        new(8_333m,   25m),
        new(0m,       30m),
    ];

    public override IReadOnlyList<LeaveTypeDefinition> GetLeaveTypes() =>
    [
        new(
            Name:                "Annual Leave",
            Code:                "ANNUAL",
            EntitlementDays:     21,
            WomenOnly:           false,
            Icon:                "🌴",
            Category:            "Time off",
            IsEarned:            true,
            CarryOverAllowed:    true,
            CarryOverDays:       5,
            MaxConsecutiveDays:  15,
            MinAdvanceNoticeDays: 7,
            MaxPerRequest:       15,
            Description:         "Paid annual leave per the Kenya Employment Act 2007 (min. 21 working days).",
            Rules:               "21 working days per year after 12 months of continuous service. Pro-rated for partial years.",
            Eligibility:         "All confirmed staff. Probationers after 6 months at manager discretion.",
            Documentation:       "Not required.",
            ApprovalProcess:     "Line manager approves; HR notified."),

        new(
            Name:                "Sick Leave",
            Code:                "SICK",
            EntitlementDays:     7,
            WomenOnly:           false,
            Icon:                "🤒",
            Category:            "Health",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  7,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       7,
            Description:         "Paid sick leave — 7 days full pay + 7 days half pay per year per the Employment Act 2007.",
            Rules:               "First 7 days at full pay; next 7 days at half pay. Medical certificate required for absences > 3 consecutive days.",
            Eligibility:         "All staff including probationers.",
            Documentation:       "Medical certificate from a licensed facility for absences > 3 days.",
            ApprovalProcess:     "Notify line manager same day; HR records; certificate attached."),

        new(
            Name:                "Maternity Leave",
            Code:                "MATERNITY",
            EntitlementDays:     90,
            WomenOnly:           true,
            Icon:                "🤰",
            Category:            "Parental",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  90,
            MinAdvanceNoticeDays: 14,
            MaxPerRequest:       90,
            Description:         "Paid maternity leave — 90 days at full pay per the Kenya Employment Act 2007.",
            Rules:               "90 calendar days at full pay.",
            Eligibility:         "All permanent female employees.",
            Documentation:       "Medical certificate confirming expected date of delivery.",
            ApprovalProcess:     "Line manager + HR; cover plan attached."),

        new(
            Name:                "Paternity Leave",
            Code:                "PATERNITY",
            EntitlementDays:     14,
            WomenOnly:           false,
            Icon:                "👶",
            Category:            "Parental",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  14,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       14,
            Description:         "Paid paternity leave — 14 days per the Kenya Employment Act 2007.",
            Rules:               "14 calendar days within 7 days of birth.",
            Eligibility:         "All male employees.",
            Documentation:       "Birth notification.",
            ApprovalProcess:     "Line manager approves; HR records."),

        new(
            Name:                "Study / Exam Leave",
            Code:                "STUDY",
            EntitlementDays:     5,
            WomenOnly:           false,
            Icon:                "📚",
            Category:            "Development",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  5,
            MinAdvanceNoticeDays: 14,
            MaxPerRequest:       5,
            Description:         "Paid leave to sit examinations relevant to the employee's role.",
            Rules:               "Up to 5 working days per year. Subject to L&D approval and study plan on file.",
            Eligibility:         "Confirmed staff with at least 12 months service.",
            Documentation:       "Exam timetable or schedule attached to request.",
            ApprovalProcess:     "Line manager + L&D approve; HR records."),

        new(
            Name:                "Emergency / Compassionate Leave",
            Code:                "COMPASSIONATE",
            EntitlementDays:     3,
            WomenOnly:           false,
            Icon:                "🫂",
            Category:            "Family",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  3,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       3,
            Description:         "Paid leave on the death of an immediate family member or major family emergency.",
            Rules:               "Up to 3 working days per occurrence.",
            Eligibility:         "All staff.",
            Documentation:       "Death certificate or supporting evidence on return.",
            ApprovalProcess:     "Notify line manager; HR records."),
    ];

    public override IReadOnlyList<PublicHolidayDefinition> GetPublicHolidays(int year) =>
    [
        new("New Year's Day",   1,  1,  true,  "Public holiday — start of the calendar year."),
        new("Good Friday",      0,  0,  false, "Christian observance — Friday before Easter. Date varies."),
        new("Easter Monday",    0,  0,  false, "Christian observance — Monday after Easter. Date varies."),
        new("Labour Day",       5,  1,  true,  "International Workers' Day."),
        new("Madaraka Day",     6,  1,  true,  "Commemorates Kenya's self-governance (1963)."),
        new("Huduma Day",       10, 10, true,  "National public service day."),
        new("Mashujaa Day",     10, 20, true,  "Heroes' Day — celebrates national heroes."),
        new("Jamhuri Day",      12, 12, true,  "Republic Day — independence from Britain (1963)."),
        new("Christmas Day",    12, 25, true,  "Christian observance — birth of Christ."),
        new("Boxing Day",       12, 26, true,  "Public holiday observed after Christmas Day."),
    ];
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
