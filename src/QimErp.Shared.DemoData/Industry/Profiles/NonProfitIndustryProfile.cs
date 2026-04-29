using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

/// <summary>
/// Non-profit / NGO industry. Lifted from QimErp.IAM.Seeding.Demo's NonProfitIndustryData.
/// Stations are HQ + field offices in regions where the NGO operates programmes.
/// </summary>
public sealed class NonProfitIndustryProfile : IIndustryProfile
{
    public string Code => "NONPROFIT";
    public string DisplayName => "Non-Profit & NGO";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "World Vision Ghana", "Plan International Ghana", "ActionAid Ghana",
        "Ghana Red Cross", "Compassion International Ghana", "CARE International Ghana",
        "Save the Children Ghana", "Oxfam Ghana", "WaterAid Ghana",
        "SOS Children's Villages Ghana", "Catholic Relief Services Ghana",
        "Child Rights International"
    ];

    public OrgHierarchySpec BuildOrgHierarchy(CompanyTier tier, int targetEmployees, int randomSeed)
    {
        var (units, distribution) = tier switch
        {
            CompanyTier.Startup    => (StartupUnits,    StartupDistribution),
            CompanyTier.SME        => (SmeUnits,        SmeDistribution),
            CompanyTier.Corporate  => (CorporateUnits,  CorporateDistribution),
            CompanyTier.NonProfit  => (NonProfitUnits,  NonProfitDistribution),
            _                      => (NonProfitUnits,  NonProfitDistribution)
        };
        return OrgHierarchyBuilder.Build(units, distribution, targetEmployees, randomSeed);
    }

    public IReadOnlyList<JobTitleSpec> JobTitles => _jobTitles;

    public StationLayout BuildStations(CompanyTier tier, int targetEmployees, int randomSeed)
    {
        var rng = new Random(randomSeed);
        var hq = new StationSpec(
            Code: "HQ",
            Name: "Country Office",
            StationType: "Country Office",
            Region: "Greater Accra",
            City: "Accra",
            Address: "East Legon, Accra",
            CapacityMin: 30,
            CapacityMax: tier == CompanyTier.Corporate ? 400 : 150);

        var fieldCount = tier switch
        {
            CompanyTier.Startup   => 1,
            CompanyTier.SME       => Math.Max(2, targetEmployees / 100),
            CompanyTier.Corporate => Math.Max(4, Math.Min(15, targetEmployees / 80)),
            CompanyTier.NonProfit => Math.Max(2, targetEmployees / 100),
            _                     => 3
        };

        var fieldOffices = new List<StationSpec>(fieldCount);
        for (var i = 0; i < fieldCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            fieldOffices.Add(new StationSpec(
                Code: $"FO{i + 1:D3}",
                Name: $"{city} Field Office",
                StationType: "Field Office",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 5,
                CapacityMax: 40));
        }

        // No satellites — NGOs do community outreach from field offices directly.
        return new StationLayout(hq, fieldOffices, new List<StationSpec>());
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.010,
            [4] = 0.060,
            [3] = 0.300, // programme officers / field officers / accountants
            [2] = 0.470,
            [1] = 0.160
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (7_000m, 14_000m),
            [4] = (5_000m, 10_000m),
            [3] = (3_500m,  8_500m),
            [2] = (2_500m,  5_000m),
            [1] = (1_500m,  3_000m)
        });

    // ─────────── baseline org units (lifted from NonProfitIndustryData) ───────────

    private static readonly IReadOnlyList<string> ProgramsJobs    = ["PROGRAMS_DIR", "PROGRAM_MGR", "PROGRAM_OFFICER", "PROGRAM_ASSIST", "PROGRAM_INTERN", "VOLUNTEER_COORD"];
    private static readonly IReadOnlyList<string> GrantsJobs      = ["DEV_DIR", "GRANTS_MGR", "GRANTS_OFFICER", "FUNDRAISING_COORD", "FUNDRAISING_ASSIST"];
    private static readonly IReadOnlyList<string> FieldJobs       = ["FIELD_MGR", "FIELD_OFFICER", "OUTREACH_COORD", "FIELD_ASSIST"];
    private static readonly IReadOnlyList<string> AdvocacyJobs    = ["COMM_MGR", "COMM_OFFICER"];
    private static readonly IReadOnlyList<string> FinanceJobs     = ["FINANCE_MGR", "ACCOUNTANT"];
    private static readonly IReadOnlyList<string> AdminJobs       = ["ADMIN_ASSIST"];
    private static readonly IReadOnlyList<string> ItJobs          = [];
    private static readonly IReadOnlyList<string> ExecJobs        = ["EXEC_DIR"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",  "Founder/Executive Director", null,      OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMS", "Programs",                   "FOUNDER", OrgUnitKind.Function,  ProgramsJobs),
        new("ADMIN",    "Admin",                      "FOUNDER", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]  = 0.20,
        ["PROGRAMS"] = 0.60,
        ["ADMIN"]    = 0.20
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",     "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMS", "Programs",             "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("GRANTS",   "Grants & Fundraising", "EXEC", OrgUnitKind.Function,  GrantsJobs),
        new("FIELD",    "Field Operations",     "EXEC", OrgUnitKind.Function,  FieldJobs),
        new("ADMIN",    "Admin & HR",           "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.08,
        ["PROGRAMS"] = 0.40,
        ["GRANTS"]   = 0.18,
        ["FIELD"]    = 0.25,
        ["ADMIN"]    = 0.09
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",     "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMS", "Programs",             "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("GRANTS",   "Grants & Fundraising", "EXEC", OrgUnitKind.Function,  GrantsJobs),
        new("FIELD",    "Field Operations",     "EXEC", OrgUnitKind.Function,  FieldJobs),
        new("ADVOCACY", "Advocacy",             "EXEC", OrgUnitKind.Function,  AdvocacyJobs),
        new("FINANCE",  "Finance & Accounting", "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("ADMIN",    "Admin & HR",           "EXEC", OrgUnitKind.Function,  AdminJobs),
        new("IT",       "IT",                   "EXEC", OrgUnitKind.Function,  ItJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.06,
        ["PROGRAMS"] = 0.35,
        ["GRANTS"]   = 0.18,
        ["FIELD"]    = 0.20,
        ["ADVOCACY"] = 0.08,
        ["FINANCE"]  = 0.06,
        ["ADMIN"]    = 0.05,
        ["IT"]       = 0.02
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMS", "Programs",             "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("GRANTS",   "Grants & Fundraising", "EXEC", OrgUnitKind.Function,  GrantsJobs),
        new("FIELD",    "Field Operations",     "EXEC", OrgUnitKind.Function,  FieldJobs),
        new("ADMIN",    "Admin & HR",           "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.08,
        ["PROGRAMS"] = 0.40,
        ["GRANTS"]   = 0.18,
        ["FIELD"]    = 0.25,
        ["ADMIN"]    = 0.09
    };

    // ─────────── job titles (lifted from NonProfitIndustryData) ───────────
    // NOTE: Source places "Volunteer Coordinator" under the Entry-Level (1) comment
    // block but assigns it Level=2. We preserve the source's Level value (2).

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // Executive (5)
        new("EXEC_DIR",           "Executive Director",            5, 8_000m, 14_000m, "EXEC",     null,         true,  "Master's Degree",   8, "NonProfit Leadership, Strategic Planning"),
        new("PROGRAMS_DIR",       "Programs Director",             5, 7_000m, 12_000m, "PROGRAMS", "EXEC_DIR",   true,  "Master's Degree",   7, "Program Management, Community Development"),
        new("DEV_DIR",            "Development Director",          5, 7_000m, 12_000m, "GRANTS",   "EXEC_DIR",   true,  "Master's Degree",   7, "Fundraising, Grant Writing, Donor Relations"),
        // Senior (4)
        new("PROGRAM_MGR",        "Program Manager",               4, 5_000m,  9_000m, "PROGRAMS", "PROGRAMS_DIR",true, "Bachelor's Degree", 5, "Program Management, Implementation"),
        new("GRANTS_MGR",         "Grants Manager",                4, 5_500m,  9_500m, "GRANTS",   "DEV_DIR",    true,  "Bachelor's Degree", 5, "Grant Writing, Fundraising, Donor Relations"),
        new("FIELD_MGR",          "Field Operations Manager",      4, 5_000m,  9_000m, "FIELD",    null,         true,  "Bachelor's Degree", 5, "Field Operations, Community Engagement"),
        new("FINANCE_MGR",        "Finance Manager",               4, 6_000m, 10_000m, "FINANCE",  null,         true,  "Bachelor's Degree", 5, "Financial Management, Reporting, Compliance"),
        new("COMM_MGR",           "Communications Manager",        4, 5_000m,  9_000m, "ADVOCACY", null,         true,  "Bachelor's Degree", 5, "Communications, Advocacy, Marketing"),
        // Mid (3)
        new("PROGRAM_OFFICER",    "Program Officer",               3, 4_000m,  7_000m, "PROGRAMS", "PROGRAM_MGR",false, "Bachelor's Degree", 3, "Program Implementation, Monitoring, Evaluation"),
        new("GRANTS_OFFICER",     "Grants Officer",                3, 4_500m,  7_500m, "GRANTS",   "GRANTS_MGR", false, "Bachelor's Degree", 3, "Grant Writing, Proposal Development"),
        new("FIELD_OFFICER",      "Field Officer",                 3, 3_500m,  6_000m, "FIELD",    "FIELD_MGR",  false, "Diploma",           2, "Field Operations, Community Engagement"),
        new("OUTREACH_COORD",     "Community Outreach Coordinator",3, 4_000m,  7_000m, "FIELD",    "FIELD_MGR",  false, "Bachelor's Degree", 2, "Community Outreach, Event Coordination"),
        new("FUNDRAISING_COORD",  "Fundraising Coordinator",       3, 4_000m,  7_000m, "GRANTS",   "GRANTS_MGR", false, "Bachelor's Degree", 2, "Fundraising, Donor Relations, Events"),
        new("ACCOUNTANT",         "Accountant",                    3, 5_000m,  8_500m, "FINANCE",  "FINANCE_MGR",false, "Bachelor's Degree", 2, "Accounting, Financial Reporting"),
        new("COMM_OFFICER",       "Communications Officer",        3, 4_000m,  7_000m, "ADVOCACY", "COMM_MGR",   false, "Bachelor's Degree", 2, "Communications, Social Media, Content"),
        // Junior (2)
        new("PROGRAM_ASSIST",     "Program Assistant",             2, 2_500m,  4_500m, "PROGRAMS", "PROGRAM_OFFICER",false,"Diploma",       1, "Program Support, Data Entry"),
        new("FIELD_ASSIST",       "Field Assistant",               2, 2_500m,  4_000m, "FIELD",    "FIELD_OFFICER",false,"High School",     1, "Field Support, Community Engagement"),
        new("ADMIN_ASSIST",       "Administrative Assistant",      2, 2_500m,  4_500m, "ADMIN",    null,         false, "High School",       1, "Administrative Support, Filing"),
        new("FUNDRAISING_ASSIST", "Fundraising Assistant",         2, 3_000m,  5_000m, "GRANTS",   "FUNDRAISING_COORD",false,"High School", 1, "Fundraising Support, Events"),
        new("VOLUNTEER_COORD",    "Volunteer Coordinator",         2, 3_000m,  5_000m, "PROGRAMS", "PROGRAM_MGR",false, "High School",       1, "Volunteer Management, Coordination"),
        // Entry (1)
        new("PROGRAM_INTERN",     "Program Intern",                1, 1_500m,  3_000m, "PROGRAMS", "PROGRAM_ASSIST",false,"Student",        0, "Learning, Program Support")
    ];
}
