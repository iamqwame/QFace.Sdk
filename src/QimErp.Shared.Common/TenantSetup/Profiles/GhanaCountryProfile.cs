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
            ApprovalProcess:     "Line manager approves; HR records."),

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
            ApprovalProcess:     "Line manager + HR Director approval required."),
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
