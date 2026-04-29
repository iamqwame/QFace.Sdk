using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

/// <summary>
/// Construction & civil-works industry. Heavy weight at L1/L2 (skilled trades and
/// labourers are the bulk); stations are HQ + Project Sites (transient) + Equipment Yards.
/// </summary>
public sealed class ConstructionIndustryProfile : IIndustryProfile
{
    public string Code => "CONSTRUCTION";
    public string DisplayName => "Construction & Civil Works";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "Consar Ltd", "M Barbisotti & Sons", "Micheletti & Co", "P.W. Ghana Ltd",
        "Justmoh Construction", "Berock Ventures", "First Sky Group",
        "Lordmav Construction", "Top International Engineering",
        "Ghana Highway Authority Contractors", "Cherubim Construction",
        "RKL Engineering", "B5 Plus Building Contractors"
    ];

    public OrgHierarchySpec BuildOrgHierarchy(CompanyTier tier, int targetEmployees, int randomSeed)
    {
        var (units, distribution) = tier switch
        {
            CompanyTier.Startup    => (StartupUnits,    StartupDistribution),
            CompanyTier.SME        => (SmeUnits,        SmeDistribution),
            CompanyTier.Corporate  => (CorporateUnits,  CorporateDistribution),
            CompanyTier.NonProfit  => (SmeUnits,        SmeDistribution),
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
            Name: "Headquarters",
            StationType: "Head Office",
            Region: "Greater Accra",
            City: "Accra",
            Address: "Spintex Road, Accra",
            CapacityMin: 50,
            CapacityMax: tier == CompanyTier.Corporate ? 400 : 100);

        var siteCount = tier switch
        {
            CompanyTier.Startup   => 1,
            CompanyTier.SME       => Math.Max(2, targetEmployees / 80),
            CompanyTier.Corporate => Math.Max(5, Math.Min(20, targetEmployees / 100)),
            _                     => 3
        };

        var projects = new List<StationSpec>(siteCount);
        for (var i = 0; i < siteCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            projects.Add(new StationSpec(
                Code: $"PROJ{i + 1:D3}",
                Name: $"{city} Project Site",
                StationType: "Project Site",
                Region: region,
                City: city,
                Address: $"Site {i + 1}, {city}",
                CapacityMin: 30,
                CapacityMax: 350));
        }

        // Equipment / regional yards (small)
        var yardCount = Math.Max(1, siteCount / 3);
        var yards = new List<StationSpec>(yardCount);
        for (var i = 0; i < yardCount; i++)
        {
            yards.Add(new StationSpec(
                Code: $"YARD{i + 1:D2}",
                Name: $"Equipment Yard {i + 1}",
                StationType: "Equipment Yard",
                Region: "Greater Accra",
                City: "Tema",
                Address: $"Industrial Area, Tema",
                CapacityMin: 5,
                CapacityMax: 25));
        }

        return new StationLayout(hq, projects, yards);
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.005, // executive
            [4] = 0.030, // senior
            [3] = 0.080, // mid
            [2] = 0.485, // skilled trades
            [1] = 0.400  // labourers / apprentices
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (18_000m, 40_000m),
            [4] = (8_000m,  17_000m),
            [3] = (4_500m,  10_000m),
            [2] = (1_800m,   4_500m),
            [1] = (1_200m,   2_200m)
        });

    private static readonly IReadOnlyList<string> SiteJobs = ["SITE_ENG", "FOREMAN", "MASON", "CARPENTER", "STEELFIXER", "ELECTRICIAN", "PLUMBER", "DRIVER", "LABOURER", "APPRENTICE"];
    private static readonly IReadOnlyList<string> EngJobs  = ["CHIEF_ENG", "PROJECT_MGR", "QS", "SITE_ENG"];
    private static readonly IReadOnlyList<string> HseJobs  = ["HSE_MGR"];
    private static readonly IReadOnlyList<string> ExecJobs = ["MD", "OPS_DIR", "FIN_DIR"];
    private static readonly IReadOnlyList<string> AdminJobs = ["ACCOUNTANT", "HR_OFFICER"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER", "Founder/MD",   null,      OrgUnitKind.Executive, ExecJobs),
        new("SITE",    "Site Ops",     "FOUNDER", OrgUnitKind.Function,  SiteJobs),
        new("ADMIN",   "Admin",        "FOUNDER", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"] = 0.10,
        ["SITE"]    = 0.75,
        ["ADMIN"]   = 0.15
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",  "Executive",          null,   OrgUnitKind.Executive, ExecJobs),
        new("PMO",   "Project Management", "EXEC", OrgUnitKind.Function,  EngJobs),
        new("SITE",  "Site Operations",    "EXEC", OrgUnitKind.Function,  SiteJobs),
        new("ENG",   "Engineering",        "EXEC", OrgUnitKind.Function,  EngJobs),
        new("HSE",   "Health, Safety & Env","EXEC", OrgUnitKind.Function,  HseJobs),
        new("ADMIN", "Finance & Admin",    "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]  = 0.04,
        ["PMO"]   = 0.06,
        ["SITE"]  = 0.65,
        ["ENG"]   = 0.10,
        ["HSE"]   = 0.05,
        ["ADMIN"] = 0.10
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",  "Executive",          null,   OrgUnitKind.Executive, ExecJobs),
        new("PMO",   "Project Management", "EXEC", OrgUnitKind.Function,  EngJobs),
        new("SITE",  "Site Operations",    "EXEC", OrgUnitKind.Function,  SiteJobs),
        new("ENG",   "Engineering & Design","EXEC", OrgUnitKind.Function,  EngJobs),
        new("PROC",  "Procurement",        "EXEC", OrgUnitKind.Function,  AdminJobs),
        new("HSE",   "Health, Safety & Env","EXEC", OrgUnitKind.Function,  HseJobs),
        new("EQUIP", "Equipment & Maint.", "EXEC", OrgUnitKind.Function,  SiteJobs),
        new("FIN",   "Finance",            "EXEC", OrgUnitKind.Function,  AdminJobs),
        new("HR",    "HR & Admin",         "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]  = 0.02,
        ["PMO"]   = 0.05,
        ["SITE"]  = 0.65,
        ["ENG"]   = 0.10,
        ["PROC"]  = 0.04,
        ["HSE"]   = 0.04,
        ["EQUIP"] = 0.05,
        ["FIN"]   = 0.03,
        ["HR"]    = 0.02
    };

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // Executive (5)
        new("MD",          "Managing Director",       5, 25_000m, 40_000m, null,    null,    true,  "Master's Degree",   15, "Construction Leadership"),
        new("OPS_DIR",     "Operations Director",     5, 18_000m, 30_000m, "PMO",   "MD",    true,  "Master's Degree",   12, "Operations, Project Delivery"),
        new("FIN_DIR",     "Finance Director",        5, 18_000m, 28_000m, "FIN",   "MD",    true,  "Master's Degree",   12, "Finance, Reporting"),
        // Senior (4)
        new("PROJECT_MGR", "Project Manager",         4, 9_000m,  17_000m, "PMO",   "OPS_DIR", true,"Bachelor's Degree", 8,  "Project Planning, Cost Control"),
        new("CHIEF_ENG",   "Chief Engineer",          4, 9_000m,  16_000m, "ENG",   "OPS_DIR", true,"Bachelor's Degree", 8,  "Civil/Structural Engineering"),
        new("HSE_MGR",     "HSE Manager",             4, 7_000m,  13_000m, "HSE",   "OPS_DIR", true,"Bachelor's Degree", 6,  "OSHA, NEBOSH, Safety Audits"),
        // Mid (3)
        new("QS",          "Quantity Surveyor",       3, 5_000m,  10_000m, "PMO",   "PROJECT_MGR", false, "Bachelor's Degree", 4, "BoQ, Cost Estimation"),
        new("SITE_ENG",    "Site Engineer",           3, 4_500m,  9_000m,  "ENG",   "CHIEF_ENG", false, "Bachelor's Degree", 3, "Site Supervision, RC Design"),
        new("ACCOUNTANT",  "Accountant",              3, 4_500m,  8_500m,  "FIN",   "FIN_DIR", false, "Bachelor's Degree", 3, "Accounting, Project Costing"),
        new("HR_OFFICER",  "HR Officer",              3, 4_000m,  8_000m,  "HR",    null,    false, "Bachelor's Degree", 3, "Recruitment, Payroll"),
        // Junior / Skilled trades (2)
        new("FOREMAN",     "Foreman",                 2, 3_500m,  6_000m,  "SITE",  "SITE_ENG", true, "Diploma",           5, "Crew Leadership, Trades"),
        new("MASON",       "Mason",                   2, 2_500m,  4_500m,  "SITE",  "FOREMAN", false,"Trade Certificate",  3, "Block Laying, Concrete Work"),
        new("CARPENTER",   "Carpenter",               2, 2_500m,  4_500m,  "SITE",  "FOREMAN", false,"Trade Certificate",  3, "Formwork, Joinery"),
        new("STEELFIXER",  "Steel Fixer",             2, 2_500m,  4_500m,  "SITE",  "FOREMAN", false,"Trade Certificate",  3, "Rebar Tying, Cutting"),
        new("ELECTRICIAN", "Electrician",             2, 3_000m,  5_000m,  "SITE",  "FOREMAN", false,"Trade Certificate",  3, "Wiring, Conduit"),
        new("PLUMBER",     "Plumber",                 2, 2_800m,  4_800m,  "SITE",  "FOREMAN", false,"Trade Certificate",  3, "Pipe Fitting, Installation"),
        new("DRIVER",      "Heavy Equipment Driver",  2, 2_500m,  4_500m,  "SITE",  "FOREMAN", false,"Driver's Licence",   2, "Excavator, Loader"),
        // Entry (1)
        new("LABOURER",    "Labourer",                1, 1_500m,  2_200m,  "SITE",  "FOREMAN", false,"None",               0, "Manual Labour"),
        new("APPRENTICE",  "Apprentice",              1, 1_200m,  1_800m,  "SITE",  "FOREMAN", false,"Trade Apprentice",   0, "Learning, Assisting Trades")
    ];
}
