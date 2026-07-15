namespace QimErp.Shared.Common.TenantSetup.Profiles;

/// <summary>
/// Ghana country setup profile — the canonical reference implementation.
/// Rates sourced from: Ghana Labour Act 2003 (Act 651), SSNIT Act 1991 (PNDC Law 247),
/// and GRA PAYE bands (effective 2024).
/// </summary>
public class GhanaCountryProfile : ICountrySetupProfile
{
    public virtual string CountryCode => "GH";
    public virtual string CountryName => "Ghana";
    public virtual string Currency => "GHS";
    public virtual string CurrencySymbol => "₵";

    public virtual StatutoryPensionConfig GetPensionConfig() => new(
        PensionAuthorityName:       "SSNIT",
        EmployeeRateTier1:          5.5m,
        EmployerRateTier1:          13.0m,
        EmployeeRateTier2:          5.0m,
        EmployerRateTier2:          0m,
        EmployeeRateTier3:          5.0m,
        EmployerRateTier3:          0m,
        Tier1Code:                  "DED-SSNIT",
        Tier2Code:                  "DED-TIER2",
        Tier3Code:                  "DED-TIER3",
        Tier1TrusteeName:           "SSNIT",
        Tier2TrusteeName:           "Old Mutual Trust",
        AnnualContributionCeiling:  42_000m * 12m);  // monthly ceiling × 12

    public virtual IReadOnlyList<TaxBracketDefinition> GetIncomeTaxBrackets() =>
    [
        // Ghana PAYE bands — chargeable income slab widths per month (GHS), GRA 2024.
        // SlabWidth=0 on the last band means "remainder / unlimited".
        new(490m,       0m),
        new(110m,       5m),
        new(130m,      10m),
        new(3166.67m,  17.5m),
        new(16000m,    25m),
        new(30520m,    30m),
        new(0m,        35m),
    ];

    public virtual PayrollSettingsDefaults GetPayrollDefaults() => new(
        Currency:                "GHS",
        CurrencySymbol:          "₵",
        DefaultWorkingDaysPerWeek: 5,
        DefaultWorkingHoursPerDay: 8,
        IsMonthly:               true,
        TaxAuthorityName:        "Ghana Revenue Authority (GRA)");

    public virtual IReadOnlyList<LeaveTypeDefinition> GetLeaveTypes() =>
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
            Description:         "Paid time away from work for rest and recreation. Accrues with continuous service.",
            Rules:               "Min. 21 working days/year after 12 months of continuous service. Pro-rated for partial years.",
            Eligibility:         "All permanent staff. Probationers may apply after 6 months at the manager's discretion.",
            Documentation:       "Not required.",
            ApprovalProcess:     "Line manager approves; HR notified."),

        new(
            Name:                "Sick Leave",
            Code:                "SICK",
            EntitlementDays:     14,
            WomenOnly:           false,
            Icon:                "🤒",
            Category:            "Health",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  14,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       14,
            Description:         "Paid leave when an employee is medically unfit to work.",
            Rules:               "Up to 14 working days/year. Beyond 3 consecutive days a medical certificate is mandatory.",
            Eligibility:         "All staff including probationers.",
            Documentation:       "Medical certificate from a licensed health facility for absences > 3 days.",
            ApprovalProcess:     "Notify line manager same day; HR records; medical proof attached."),

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
            MaxPerRequest:       98,
            Description:         "Paid leave granted to a female employee before and after childbirth.",
            Rules:               "12 weeks (84 days) at full pay. Extendable by 2 weeks for complications or twins.",
            Eligibility:         "Permanent female employees. No minimum service required.",
            Documentation:       "Medical certificate confirming expected/actual date of delivery.",
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
            ApprovalProcess:     "Line manager approves; HR records.",
            MenOnly:             true),

        new(
            Name:                "Study / Exam Leave",
            Code:                "STUDY",
            EntitlementDays:     10,
            WomenOnly:           false,
            Icon:                "📚",
            Category:            "Development",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  10,
            MinAdvanceNoticeDays: 14,
            MaxPerRequest:       10,
            Description:         "Paid leave to sit examinations or attend training relevant to the employee's role.",
            Rules:               "Up to 10 working days/year. Subject to L&D approval and a study plan on file.",
            Eligibility:         "Confirmed staff with at least 12 months service.",
            Documentation:       "Exam timetable or training schedule attached to the request.",
            ApprovalProcess:     "Line manager + L&D approve; HR records."),

        new(
            Name:                "Emergency / Compassionate Leave",
            Code:                "COMPASSIONATE",
            EntitlementDays:     5,
            WomenOnly:           false,
            Icon:                "🫂",
            Category:            "Family",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  5,
            MinAdvanceNoticeDays: 0,
            MaxPerRequest:       5,
            Description:         "Paid leave on the death of an immediate family member or major family emergency.",
            Rules:               "Up to 5 working days per occurrence. Additional unpaid days may be requested.",
            Eligibility:         "All staff.",
            Documentation:       "Death certificate or supporting evidence on return.",
            ApprovalProcess:     "Notify line manager; HR records."),

        new(
            Name:                "Unpaid Leave",
            Code:                "UNPAID",
            EntitlementDays:     30,
            WomenOnly:           false,
            Icon:                "📋",
            Category:            "Time off",
            IsEarned:            false,
            CarryOverAllowed:    false,
            CarryOverDays:       null,
            MaxConsecutiveDays:  30,
            MinAdvanceNoticeDays: 7,
            MaxPerRequest:       30,
            Description:         "Leave without pay for personal reasons not covered by other leave types.",
            Rules:               "Maximum 30 calendar days per year. Does not affect statutory entitlements.",
            Eligibility:         "All confirmed staff. Subject to manager and HR approval.",
            Documentation:       "Written request with reason. Supporting documents where applicable.",
            ApprovalProcess:     "Line manager + HR Director approval required.",
            Unpaid:              true),
    ];

    // Codes must match PayrollCodes.AllowanceCodes.X / DeductionCodes.X in QimErp.Payroll.
    public virtual IReadOnlyList<AllowanceDefinition> GetAllowances() =>
    [
        new("ALW-TRANS",  "Transport",         "Fixed",      600m,   true,  "Standard transport allowance for office-based staff.",
            "6120-TRANSPORT-EXP",  "Transport Allowance Expense",
            "bus",       "var(--qim-info-bg)",    "var(--qim-sky)"),

        new("ALW-RENT",   "Rent",              "Percentage", 25m,    true,  "25% of basic salary — paid as rent allowance.",
            "6110-HOUSING-EXP",    "Housing Allowance Expense",
            "home",      "rgb(237, 232, 254)",    "rgb(124, 58, 237)"),

        new("ALW-FUEL",   "Fuel",              "Fixed",      1200m,  true,  "For roles entitled to a company vehicle.",
            "6130-FUEL-EXP",       "Fuel Allowance Expense",
            "fuel",      "rgb(255, 237, 213)",    "var(--qim-orange)"),

        new("ALW-MEAL",   "Lunch",             "Fixed",      300m,   false, "Subsidised meals — non-taxable up to GHS 300/mo.",
            "6140-MEAL-EXP",       "Meal Allowance Expense",
            "utensils",  "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("ALW-RESP",   "Responsibility",    "Fixed",      1500m,  true,  "For managers and team leads (grade G6+).",
            "6150-RESP-EXP",       "Responsibility Allowance Expense",
            "badge",     "var(--qim-info-bg)",    "rgb(0, 79, 190)"),

        new("ALW-RISK",   "Risk",              "Percentage", 10m,    true,  "10% of basic — for warehouse, driving and field roles.",
            "6160-RISK-EXP",       "Risk Allowance Expense",
            "shield",    "var(--qim-danger-bg)",  "var(--qim-danger)"),

        new("ALW-PHONE",  "Phone & Data",      "Fixed",      200m,   false, "Phone bill reimbursement up to GHS 200.",
            "6170-PHONE-EXP",      "Phone and Data Allowance Expense",
            "phone",     "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("ALW-NIGHT",  "Night Shift",       "PerUnit",    80m,    true,  "GHS 80 per night shift worked.",
            "6180-NIGHTSHIFT-EXP", "Night Shift Allowance Expense",
            "moon",      "rgb(224, 231, 255)",    "rgb(99, 102, 241)"),

        new("ALW-ACTING", "Acting",            "Percentage", 15m,    true,  "15% of basic — for staff in acting roles.",
            "6190-ACTING-EXP",     "Acting Allowance Expense",
            "user-plus", "rgb(255, 228, 230)",    "rgb(225, 29, 72)"),
    ];

    public virtual IReadOnlyList<DeductionDefinition> GetDeductions() =>
    [
        // PAYE is excluded — it is seeded separately via TaxConfiguration/GetIncomeTaxBrackets.
        new("DED-SSNIT",  "SSNIT (Tier 1)",        "Percentage", 5.5m,  true,  true,  "Employee Tier 1 contribution to SSNIT — 5.5%.",
            "2100-SSNIT-T1-PAY",  "SSNIT Tier 1 Payable",
            "shield-check", "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("DED-TIER2",  "Tier 2 Pension",        "Percentage", 5m,    true,  false, "Tier 2 mandatory occupational pension — 5%.",
            "2101-SSNIT-T2-PAY",  "SSNIT Tier 2 Payable",
            "piggy-bank",   "rgb(204, 251, 241)",    "var(--qim-teal)"),

        new("DED-TIER3",  "Tier 3 Voluntary",      "Percentage", 5m,    false, false, "Voluntary provident fund contribution.",
            "2102-TIER3-PAY",     "Tier 3 Voluntary Payable",
            "sparkles",     "rgb(237, 232, 254)",    "rgb(124, 58, 237)"),

        new("DED-LOAN",   "Loan Repayment",        "Fixed",      null,  false, false, "Automatic loan repayments by amortization.",
            "2300-LOAN-PAY",      "Loan Recovery Payable",
            "hand-coins",   "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-ADV",    "Salary Advance",        "Fixed",      null,  false, false, "Recovery of salary advances paid out.",
            "2310-ADVANCE-PAY",   "Salary Advance Recovery Payable",
            "banknote",     "var(--qim-warning-bg)", "var(--qim-warning)"),

        new("DED-UNION",  "Union Dues",            "Fixed",      30m,   false, false, "Workers' union monthly dues.",
            "2400-UNION-PAY",     "Union Dues Payable",
            "flag",         "rgb(255, 228, 230)",    "rgb(225, 29, 72)"),

        new("DED-HEALTH", "Health Insurance",      "Fixed",      120m,  false, false, "Employee share of private health cover.",
            "2410-HEALTH-PAY",    "Health Insurance Payable",
            "heart-pulse",  "var(--qim-danger-bg)",  "var(--qim-danger)"),

        new("DED-WELF",   "Staff Welfare",         "Fixed",      25m,   false, false, "Funeral & welfare contribution.",
            "2420-WELFARE-PAY",   "Staff Welfare Payable",
            "hand-heart",   "rgb(224, 242, 254)",    "var(--qim-sky)"),

        new("DED-GARN",   "Court Garnishment",     "Percentage", 10m,   false, false, "Court-ordered garnishment.",
            "2500-GARNISH-PAY",   "Court Garnishment Payable",
            "gavel",        "var(--qim-bg-muted)",   "var(--qim-steel)"),
    ];

    public virtual IReadOnlyList<GradeDefinition> GetGrades() =>
    [
        new("GRD-01", "Entry Level",     1,  1800m,  2800m,  "Entry-level roles: graduates, trainees and support staff."),
        new("GRD-02", "Junior",          2,  2800m,  4200m,  "Junior professionals with 1-3 years experience under supervision."),
        new("GRD-03", "Intermediate",    3,  4200m,  6500m,  "Intermediate independent contributors with project ownership."),
        new("GRD-04", "Senior",          4,  6500m,  9500m,  "Senior individual contributors, technical leads and specialists."),
        new("GRD-05", "Principal",       5,  9500m, 13000m,  "Principal experts and team leads with cross-functional scope."),
        new("GRD-06", "Manager",         6, 13000m, 20000m,  "People managers with budget and team responsibility."),
        new("GRD-07", "Senior Manager",  7, 20000m, 30000m,  "Senior managers with multi-team scope and strategic planning."),
        new("GRD-08", "Executive",       8, 30000m, 60000m,  "Executive level: VP and above with company-wide accountability."),
    ];

    public virtual IReadOnlyList<BankSeedDefinition> GetBanks() =>
    [
        new("ACCESSGH",   "Access Bank (Ghana) Plc",
            "Access Bank",        "Universal",
            "+233-302-661769",    "www.ghana.accessbankplc.com", "info@ghana.accessbankplc.com",
            "Starlets' 91 Road, Opposite Accra Sports Stadium, Osu, Accra", 1),

        new("ADB",        "Agricultural Development Bank Limited",
            "ADB",                "Universal",
            "+233-302-770403",    "www.agricbank.com",           "customercare@agricbank.com",
            "Accra Financial Centre, 3rd Ambassadorial Development Area, Ridge, Accra", 2),

        new("BOA",        "Bank of Africa Ghana Limited",
            "Bank of Africa",     "Universal",
            "+233-302-249690",    "www.boaghana.com",            "complaints@boaghana.com",
            "1st Floor, The Octagon, Independence Avenue, Cantonments, Accra", 3),

        new("BARCLAYS",   "Barclays Bank of Ghana Limited",
            "Barclays Bank",      "Universal",
            "+233-302-664901",    "www.gh.barclaysafrica.com",   "service.excellence@barclays.com",
            "Barclays House, High Street, Accra", 4),

        new("CALBANK",    "CAL Bank Limited",
            "CAL Bank",           "Universal",
            "+233-302-680061",    "www.calbank.net",             "customercare@calbank.net",
            "45 Independence Avenue, Accra", 5),

        new("CBG",        "Consolidated Bank Ghana Limited",
            "Consolidated Bank",  "Universal",
            "+233-302-634330",    "www.cbg.com.gh",              "info@cbg.com.gh",
            "First Floor, Manet Tower 3, Airport City, Accra", 6),

        new("ECOBANK",    "Ecobank Ghana Limited",
            "Ecobank Ghana",      "Universal",
            "+233-302-681146",    "www.ecobank.com",             "ecobankenquiries@ecobank.com",
            "2 Morocco Lane, Off Independence Avenue, Accra North", 7),

        new("FBNBANK",    "FBNBank (Ghana) Limited",
            "FBNBank Ghana",      "Universal",
            "+233-302-236136",    "www.fbnbankghana.com",        "fbn@fbnbankghana.com",
            "Plot No. 678, Liberation Road, Airport, Accra North", 8),

        new("FIDELITY",   "Fidelity Bank Ghana Limited",
            "Fidelity Bank",      "Universal",
            "+233-302-214490",    "www.fidelitybank.com.gh",     "wecare@myfidelitybank.net",
            "Ridge Towers, Ridge, Accra", 9),

        new("FAB",        "First Atlantic Bank Limited",
            "First Atlantic Bank","Universal",
            "+233-302-682203",    "www.firstatlanticbank.com.gh","info@firstatlanticbank.com.gh",
            "Atlantic Place, 1 Seventh Avenue, Ridge West, Cantonments, Accra", 10),

        new("FNB",        "First National Bank (Ghana) Limited",
            "First National Bank","Universal",
            "+233-302-242435050", "www.firstnationalbank.com.gh","info@firstnationalbank.com.gh",
            "6th Floor, Accra Financial Centre, Cnr. Independence Ave./Liberation Road, Accra", 11),

        new("GCB",        "GCB Bank Limited",
            "GCB Bank",           "Universal",
            "+233-302-672852",    "www.gcbbank.com.gh",          "corporateaffairs@gcb.com.gh",
            "High Street, Accra", 12),

        new("GTB",        "Guaranty Trust Bank (Ghana) Limited",
            "GTBank Ghana",       "Universal",
            "+233-302-680668",    "www.gtbghana.com",            "gh.corporateaffairs@gtbank.com",
            "25A Castle Road, Ambassadorial Enclave, Ridge, Accra", 13),

        new("NIB",        "National Investment Bank Limited",
            "NIB",                "Universal",
            "+233-302-661701",    "www.nib-ghana.com",           "info@nib-ghana.com",
            "Kwame Nkrumah Avenue, Accra", 14),

        new("OMNIBSIC",   "Omni-BSIC Bank Ghana Limited",
            "Omni-BSIC Bank",     "Universal",
            "+233-307-086000",    "www.omnibank.com.gh",         "info@omnibank.com.gh",
            "C9/14 Dzorwulu, Olusegun Way, Kaneshie, Accra", 15),

        new("PRUDENTIAL", "Prudential Bank Limited",
            "Prudential Bank",    "Universal",
            "+233-302-781200",    "www.prudentialbank.com.gh",   "headoffice@prudentialbank.com.gh",
            "Ring Road Central, Accra", 16),

        new("REPUBLIC",   "Republic Bank (Ghana) Limited",
            "Republic Bank",      "Universal",
            "+233-302-242090",    "www.republicghana.com",       "email@republicghana.com",
            "Ebankese No. 35, Sixth Avenue, North Ridge, Accra", 17),

        new("SOCGEN",     "Societe Generale (Ghana) Limited",
            "Société Générale",   "Universal",
            "+233-302-202001",    "www.societegenerale.com.gh",  "sgghana.info@socgen.com",
            "Ring Road Central, Accra", 18),

        new("STANBIC",    "Stanbic Bank Ghana Limited",
            "Stanbic Bank",       "Universal",
            "+233-302-687670",    "www.stanbicbank.com.gh",      "customercare@stanbic.com.gh",
            "Stanbic Heights, 25 Liberation Link, Airport City, Cantonments, Accra", 19),

        new("SCB",        "Standard Chartered Bank (Ghana) Limited",
            "Standard Chartered", "Universal",
            "+233-302-664591",    "www.sc.com/gh",               "feedback.ghana@sc.com",
            "No. 87 Independence Avenue, Accra", 20),

        new("UBA",        "United Bank for Africa (Ghana) Limited",
            "UBA Ghana",          "Universal",
            "+233-302-674085",    "www.ubagroup.com",            null,
            "Heritage Towers, Ambassadorial Enclave, Off Liberia Road, Ridge, Accra", 21),

        new("UMB",        "Universal Merchant Bank Limited",
            "UMB",                "Universal",
            "+233-302-666331",    "www.myumbbank.com",           "info@myumbbank.com",
            "SSNIT Emporium, Airport City, North Ridge, Accra", 22),

        new("ZENITH",     "Zenith Bank (Ghana) Limited",
            "Zenith Bank",        "Universal",
            "+233-302-660075",    "www.zenithbank.com.gh",       "info@zenithbank.com.gh",
            "Zenith Heights, No. 31 Independence Avenue, Cantonments, Accra", 23),

        new("GHLBANK",    "GHL Bank Limited",
            "GHL Bank",           "Universal",
            "+233-302-912958",    "www.ghlbank.com",             "info@ghlbank.com",
            "No. 63a Aviation Road, Airport Residential Area, Accra", 24),

        new("ARBAPEX",    "ARB Apex Bank Limited",
            "ARB Apex Bank",      "Apex",
            "+233-302-247633",    "www.arbapexbank.com",         "info@arbapexbank.com",
            "Accra, Ghana", 25),
    ];

    public virtual string GetPensionLedgerCode(int tier) => tier switch
    {
        1 => "2100-SSNIT-T1",
        2 => "2101-SSNIT-T2",
        3 => "2102-TIER3",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };

    public virtual string GetPensionLedgerName(int tier) => tier switch
    {
        1 => "SSNIT Tier 1 Payable",
        2 => "SSNIT Tier 2 Payable",
        3 => "Tier 3 Voluntary Payable",
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Pension tier must be 1, 2, or 3.")
    };

    public virtual IReadOnlyList<DocumentTypeDefinition> GetDocumentTypes() =>
    [
        new("PASSPORT",      "Passport",       1),
        new("SSNIT",         "SSNIT Card",     2),
        new("NHIL",          "NHIL Card",      3),
        new("TIN",           "TIN",            4),
        new("VOTERID",       "Voter ID",       5),
        new("DRIVERLICENSE", "Driver License", 6),
    ];

    public virtual IReadOnlyList<PublicHolidayDefinition> GetPublicHolidays(int year) =>
    [
        new("New Year's Day",            1,  1, true,  "Public holiday — start of the calendar year."),
        new("Constitution Day",          1,  7, true,  "Constitution Day — national public holiday."),
        new("Independence Day",          3,  6, true,  "Commemorates Ghana's independence (1957)."),
        new("Good Friday",               0,  0, false, "Christian observance — Friday before Easter. Date varies."),
        new("Easter Monday",             0,  0, false, "Christian observance — Monday after Easter. Date varies."),
        new("Labour Day",                5,  1, true,  "International Workers' Day."),
        new("Africa Day",                5, 25, true,  "African Union Day — national public holiday."),
        new("Founders' Day",             8,  4, true,  "Ghana Founders' Day — national public holiday."),
        new("Kwame Nkrumah Memorial Day",9, 21, true,  "Birthday of Ghana's first President."),
        new("Farmers' Day",             12,  0, false, "First Friday of December — celebrating farmers."),
        new("Christmas Day",            12, 25, true,  "Christian observance — birth of Christ."),
        new("Boxing Day",               12, 26, true,  "Public holiday observed after Christmas Day."),
    ];
}
