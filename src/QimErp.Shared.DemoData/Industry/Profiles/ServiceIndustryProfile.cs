using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

public sealed class ServiceIndustryProfile : IIndustryProfile
{
    public string Code => "SERVICE";
    public string DisplayName => "Professional Services & Consulting";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "PwC Ghana", "Deloitte Ghana", "KPMG Ghana", "Boston Consulting Ghana",
        "Ernst & Young Ghana", "Accenture Ghana", "Andersen Ghana",
        "Grant Thornton Ghana", "BDO Ghana", "Baker Tilly Ghana",
        "Sankara & Associates", "JLD Ghana"
    ];

    public OrgHierarchySpec BuildOrgHierarchy(CompanyTier tier, int targetEmployees, int randomSeed)
    {
        var (units, distribution) = tier switch
        {
            CompanyTier.Startup    => (StartupUnits,    StartupDistribution),
            CompanyTier.SME        => (SmeUnits,        SmeDistribution),
            CompanyTier.Corporate  => (CorporateUnits,  CorporateDistribution),
            CompanyTier.NonProfit  => (NonProfitUnits,  NonProfitDistribution),
            _                      => (SmeUnits,        SmeDistribution)
        };
        return OrgHierarchyBuilder.Build(units, distribution, targetEmployees, randomSeed);
    }

    public IReadOnlyList<JobTitleSpec> JobTitles => _jobTitles;

    public StationLayout BuildStations(CompanyTier tier, int targetEmployees, int randomSeed)
    {
        var rng = new Random(randomSeed);
        var hq = new StationSpec(
            Code: "HQ",
            Name: "Head Office",
            StationType: "Head Office",
            Region: "Greater Accra",
            City: "Accra",
            Address: "Ridge, Accra",
            CapacityMin: 50,
            CapacityMax: tier == CompanyTier.Corporate ? 700 : 250);

        var officeCount = tier switch
        {
            CompanyTier.Startup   => 0,
            CompanyTier.SME       => 1,
            CompanyTier.Corporate => Math.Min(3, Math.Max(1, targetEmployees / 300)),
            CompanyTier.NonProfit => 1,
            _                     => 1
        };

        var offices = new List<StationSpec>(officeCount);
        for (var i = 0; i < officeCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            offices.Add(new StationSpec(
                Code: $"REG{i + 1:D2}",
                Name: $"{city} Regional Office",
                StationType: "Regional Office",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 10,
                CapacityMax: 80));
        }

        return new StationLayout(hq, offices, new List<StationSpec>());
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.020,
            [4] = 0.080,
            [3] = 0.300,
            [2] = 0.420,
            [1] = 0.180
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (10_000m, 25_000m),
            [4] = (7_000m,  15_000m),
            [3] = (4_000m,  11_000m),
            [2] = (2_500m,   6_000m),
            [1] = (1_500m,   3_500m)
        });

    private static readonly IReadOnlyList<string> AdvisoryJobs = ["PARTNER", "SENIOR_CONSULTANT", "PRINCIPAL_CONSULTANT", "CONSULTANT", "SENIOR_ASSOC", "ASSOCIATE", "JUNIOR_CONSULTANT", "INTERN"];
    private static readonly IReadOnlyList<string> DeliveryJobs = ["DIRECTOR", "PM", "PROGRAM_MGR", "PROJECT_COORD", "BA", "PROJECT_ASSIST", "TRAINEE"];
    private static readonly IReadOnlyList<string> SalesJobs    = ["SALES_MGR", "ACCOUNT_EXEC"];
    private static readonly IReadOnlyList<string> FinanceJobs  = [];
    private static readonly IReadOnlyList<string> ItJobs       = ["IT_CONSULTANT"];
    private static readonly IReadOnlyList<string> HrJobs       = [];
    private static readonly IReadOnlyList<string> OpsJobs      = ["CSR"];
    private static readonly IReadOnlyList<string> ExecJobs     = ["MANAGING_PARTNER"];
    private static readonly IReadOnlyList<string> ProgramsJobs = ["PROGRAM_MGR", "PM"];
    private static readonly IReadOnlyList<string> ServicesJobs = ["CONSULTANT", "SENIOR_CONSULTANT"];
    private static readonly IReadOnlyList<string> FundJobs     = ["ACCOUNT_EXEC"];
    private static readonly IReadOnlyList<string> AdminJobs    = [];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",    "Founder/CEO", null,      OrgUnitKind.Executive, ExecJobs),
        new("CONSULTANT", "Consulting",  "FOUNDER", OrgUnitKind.Function,  AdvisoryJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]    = 0.30,
        ["CONSULTANT"] = 0.70
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",     "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("ADVISORY", "Advisory",          "EXEC", OrgUnitKind.Function,  AdvisoryJobs),
        new("DELIVERY", "Delivery / PMO",    "EXEC", OrgUnitKind.Function,  DeliveryJobs),
        new("SALES",    "Sales & Marketing", "EXEC", OrgUnitKind.Function,  SalesJobs),
        new("FINANCE",  "Finance & Admin",   "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("IT",       "IT Services",       "EXEC", OrgUnitKind.Function,  ItJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.10,
        ["ADVISORY"] = 0.40,
        ["DELIVERY"] = 0.25,
        ["SALES"]    = 0.15,
        ["FINANCE"]  = 0.05,
        ["IT"]       = 0.05
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",     "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("ADVISORY", "Advisory",          "EXEC", OrgUnitKind.Function,  AdvisoryJobs),
        new("DELIVERY", "Delivery / PMO",    "EXEC", OrgUnitKind.Function,  DeliveryJobs),
        new("SALES",    "Sales & Marketing", "EXEC", OrgUnitKind.Function,  SalesJobs),
        new("FINANCE",  "Finance & Admin",   "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("IT",       "IT Services",       "EXEC", OrgUnitKind.Function,  ItJobs),
        new("HR",       "HR",                "EXEC", OrgUnitKind.Function,  HrJobs),
        new("OPS",      "Operations",        "EXEC", OrgUnitKind.Function,  OpsJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.08,
        ["ADVISORY"] = 0.35,
        ["DELIVERY"] = 0.25,
        ["SALES"]    = 0.12,
        ["FINANCE"]  = 0.05,
        ["IT"]       = 0.08,
        ["HR"]       = 0.04,
        ["OPS"]      = 0.03
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",        "Executive",   null,   OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMS",    "Programs",    "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("SERVICES",    "Services",    "EXEC", OrgUnitKind.Function,  ServicesJobs),
        new("FUNDRAISING", "Fundraising", "EXEC", OrgUnitKind.Function,  FundJobs),
        new("ADMIN",       "Admin",       "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]        = 0.12,
        ["PROGRAMS"]    = 0.40,
        ["SERVICES"]    = 0.30,
        ["FUNDRAISING"] = 0.13,
        ["ADMIN"]       = 0.05
    };

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        new("MANAGING_PARTNER",     "Managing Partner",      5, 15_000m, 25_000m, null,       null,            true,  "Master's Degree",   10, "Business Leadership, Strategy"),
        new("PARTNER",              "Partner",               5, 12_000m, 20_000m, "ADVISORY", "MANAGING_PARTNER", true,"Master's Degree", 8,  "Client Relations, Practice Leadership"),
        new("DIRECTOR",             "Director",              5, 10_000m, 18_000m, "DELIVERY", "MANAGING_PARTNER", true,"Master's Degree", 8,  "Service Delivery, Practice Management"),
        new("SENIOR_CONSULTANT",    "Senior Consultant",     4, 7_000m,  12_000m, "ADVISORY", "PARTNER",       true,  "Bachelor's Degree", 6,  "Consulting, Client Engagement, Analysis"),
        new("PRINCIPAL_CONSULTANT", "Principal Consultant",  4, 9_000m,  15_000m, "ADVISORY", "PARTNER",       true,  "Master's Degree",   7,  "Strategic Consulting, Leadership"),
        new("PM",                   "Project Manager",       4, 8_000m,  14_000m, "DELIVERY", "DIRECTOR",      true,  "Bachelor's Degree", 5,  "Project Management, Delivery, Stakeholder Management"),
        new("PROGRAM_MGR",          "Program Manager",       4, 9_000m,  15_000m, "DELIVERY", "DIRECTOR",      true,  "Bachelor's Degree", 6,  "Program Management, Portfolio Management"),
        new("SALES_MGR",            "Sales Manager",         4, 7_000m,  12_000m, "SALES",    null,            true,  "Bachelor's Degree", 5,  "Business Development, Client Acquisition"),
        new("CONSULTANT",           "Consultant",            3, 5_000m,  9_000m,  "ADVISORY", "SENIOR_CONSULTANT",false,"Bachelor's Degree",3,"Consulting, Analysis, Client Service"),
        new("SENIOR_ASSOC",         "Senior Associate",      3, 6_000m,  10_000m, "ADVISORY", "SENIOR_CONSULTANT",false,"Bachelor's Degree",3,"Analysis, Research, Client Support"),
        new("PROJECT_COORD",        "Project Coordinator",   3, 4_000m,  7_000m,  "DELIVERY", "PM",            false, "Bachelor's Degree", 2,  "Project Coordination, Documentation"),
        new("BA",                   "Business Analyst",      3, 5_000m,  9_000m,  "DELIVERY", "PM",            false, "Bachelor's Degree", 2,  "Business Analysis, Requirements Gathering"),
        new("ACCOUNT_EXEC",         "Account Executive",     3, 5_000m,  9_000m,  "SALES",    "SALES_MGR",     false, "Bachelor's Degree", 2,  "Sales, Client Relations, Prospecting"),
        new("IT_CONSULTANT",        "IT Consultant",         3, 6_000m,  11_000m, "IT",       null,            false, "Bachelor's Degree", 3,  "IT Solutions, Implementation, Support"),
        new("ASSOCIATE",            "Associate",             2, 3_000m,  5_500m,  "ADVISORY", "CONSULTANT",    false, "Bachelor's Degree", 1,  "Analysis, Research, Support"),
        new("JUNIOR_CONSULTANT",    "Junior Consultant",     2, 3_500m,  6_000m,  "ADVISORY", "CONSULTANT",    false, "Bachelor's Degree", 1,  "Basic Consulting, Learning"),
        new("PROJECT_ASSIST",       "Project Assistant",     2, 3_000m,  5_000m,  "DELIVERY", "PROJECT_COORD", false, "Diploma",           1,  "Administrative Support, Documentation"),
        new("CSR",                  "Customer Service Rep",  2, 2_500m,  4_500m,  "OPS",      null,            false, "High School",       1,  "Customer Service, Support"),
        new("INTERN",               "Intern",                1, 1_500m,  3_000m,  "ADVISORY", "ASSOCIATE",     false, "Student",           0,  "Learning, Research, Support"),
        new("TRAINEE",              "Trainee",               1, 2_000m,  3_500m,  "DELIVERY", "PROJECT_ASSIST",false, "High School",       0,  "Learning, Basic Tasks")
    ];
}
