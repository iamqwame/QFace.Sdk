using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

/// <summary>
/// Manufacturing industry. Lifted from QimErp.IAM.Seeding.Demo's ManufacturingIndustryData.
/// Heavy weight at L1/L2 (operators, trainees, labourers); stations are HQ + factory plants
/// + distribution centres.
/// </summary>
public sealed class ManufacturingIndustryProfile : IIndustryProfile
{
    public string Code => "MANUFACTURING";
    public string DisplayName => "Manufacturing & Production";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "Unilever Ghana", "Nestlé Ghana", "Kasapreko", "Fan Milk Ghana",
        "B5 Plus Steel", "Tema Steel", "Wilmar Africa", "Ghacem",
        "Cocoa Processing Company", "PZ Cussons Ghana",
        "Guinness Ghana Breweries", "Promasidor Ghana"
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
            Name: "Headquarters",
            StationType: "Head Office",
            Region: "Greater Accra",
            City: "Tema",
            Address: "Heavy Industrial Area, Tema",
            CapacityMin: 60,
            CapacityMax: tier == CompanyTier.Corporate ? 800 : 250);

        var plantCount = tier switch
        {
            CompanyTier.Startup   => 1,
            CompanyTier.SME       => Math.Max(1, targetEmployees / 200),
            CompanyTier.Corporate => Math.Max(2, Math.Min(8, targetEmployees / 250)),
            CompanyTier.NonProfit => 1,
            _                     => 2
        };

        var plants = new List<StationSpec>(plantCount);
        for (var i = 0; i < plantCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            plants.Add(new StationSpec(
                Code: $"PLANT{i + 1:D2}",
                Name: $"{city} Factory",
                StationType: "Factory",
                Region: region,
                City: city,
                Address: $"Industrial Area, {city}",
                CapacityMin: 80,
                CapacityMax: 600));
        }

        // Distribution centres / depots — smaller satellites near urban demand.
        var depotCount = Math.Max(1, plantCount);
        var depots = new List<StationSpec>(depotCount);
        for (var i = 0; i < depotCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            depots.Add(new StationSpec(
                Code: $"DC{i + 1:D2}",
                Name: $"{city} Distribution Centre",
                StationType: "Distribution Centre",
                Region: region,
                City: city,
                Address: $"Warehouse District, {city}",
                CapacityMin: 10,
                CapacityMax: 50));
        }

        return new StationLayout(hq, plants, depots);
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.005,
            [4] = 0.030,
            [3] = 0.110,
            [2] = 0.485,
            [1] = 0.370 // many entry-level operators / trainees in factories
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (9_000m,  20_000m),
            [4] = (6_500m,  14_000m),
            [3] = (2_500m,  10_000m),
            [2] = (2_000m,   4_500m),
            [1] = (1_500m,   2_500m)
        });

    // ─────────── baseline org units (lifted from ManufacturingIndustryData) ───────────

    private static readonly IReadOnlyList<string> ProductionJobs = ["PLANT_MGR", "PROD_DIR", "PROD_MGR", "PROD_SUPER", "PROD_OPERATOR", "JUNIOR_OPERATOR", "PROD_TRAINEE"];
    private static readonly IReadOnlyList<string> QcJobs         = ["QUALITY_DIR", "QUALITY_MGR", "QC_INSPECTOR", "QC_ASSIST"];
    private static readonly IReadOnlyList<string> MaintJobs      = ["MAINT_MGR", "MAINT_TECH", "MAINT_ASSIST", "APPRENTICE"];
    private static readonly IReadOnlyList<string> HseJobs        = ["HSE_MGR", "HSE_OFFICER"];
    private static readonly IReadOnlyList<string> SupplyJobs     = ["SUPPLY_MGR", "PROC_OFFICER", "WH_ASSIST"];
    private static readonly IReadOnlyList<string> EngJobs        = ["SENIOR_ENG", "PROCESS_ENG"];
    private static readonly IReadOnlyList<string> SalesJobs      = [];
    private static readonly IReadOnlyList<string> FinanceJobs    = [];
    private static readonly IReadOnlyList<string> HrJobs         = [];
    private static readonly IReadOnlyList<string> AdminJobs      = [];
    private static readonly IReadOnlyList<string> ExecJobs       = ["PLANT_MGR", "PROD_DIR", "QUALITY_DIR"];
    private static readonly IReadOnlyList<string> ProgramsJobs   = ["PROD_SUPER", "PROD_MGR"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",    "Founder/CEO", null,      OrgUnitKind.Executive, ExecJobs),
        new("PRODUCTION", "Production",  "FOUNDER", OrgUnitKind.Function,  ProductionJobs),
        new("QUALITY",    "Quality",     "FOUNDER", OrgUnitKind.Function,  QcJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]    = 0.15,
        ["PRODUCTION"] = 0.70,
        ["QUALITY"]    = 0.15
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",        "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("PRODUCTION",  "Production",           "EXEC", OrgUnitKind.Function,  ProductionJobs),
        new("QC",          "Quality Control",      "EXEC", OrgUnitKind.Function,  QcJobs),
        new("MAINTENANCE", "Maintenance",          "EXEC", OrgUnitKind.Function,  MaintJobs),
        new("HSE",         "Health, Safety & Env", "EXEC", OrgUnitKind.Function,  HseJobs),
        new("SUPPLY",      "Supply Chain",         "EXEC", OrgUnitKind.Function,  SupplyJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]        = 0.05,
        ["PRODUCTION"]  = 0.50,
        ["QC"]          = 0.12,
        ["MAINTENANCE"] = 0.15,
        ["HSE"]         = 0.05,
        ["SUPPLY"]      = 0.13
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",        "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("PRODUCTION",  "Production",           "EXEC", OrgUnitKind.Function,  ProductionJobs),
        new("QC",          "Quality Control",      "EXEC", OrgUnitKind.Function,  QcJobs),
        new("MAINTENANCE", "Maintenance",          "EXEC", OrgUnitKind.Function,  MaintJobs),
        new("HSE",         "Health, Safety & Env", "EXEC", OrgUnitKind.Function,  HseJobs),
        new("SUPPLY",      "Supply Chain",         "EXEC", OrgUnitKind.Function,  SupplyJobs),
        new("ENG",         "Engineering",          "EXEC", OrgUnitKind.Function,  EngJobs),
        new("SALES",       "Sales & Marketing",    "EXEC", OrgUnitKind.Function,  SalesJobs),
        new("FINANCE",     "Finance & Admin",      "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("HR",          "HR",                   "EXEC", OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]        = 0.03,
        ["PRODUCTION"]  = 0.45,
        ["QC"]          = 0.10,
        ["MAINTENANCE"] = 0.12,
        ["HSE"]         = 0.05,
        ["SUPPLY"]      = 0.08,
        ["ENG"]         = 0.08,
        ["SALES"]       = 0.05,
        ["FINANCE"]     = 0.02,
        ["HR"]          = 0.02
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",       "Executive",  null,   OrgUnitKind.Executive, ExecJobs),
        new("PRODUCTION", "Production", "EXEC", OrgUnitKind.Function,  ProductionJobs),
        new("QUALITY",    "Quality",    "EXEC", OrgUnitKind.Function,  QcJobs),
        new("PROGRAMS",   "Programs",   "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("ADMIN",      "Admin",      "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]       = 0.10,
        ["PRODUCTION"] = 0.50,
        ["QUALITY"]    = 0.15,
        ["PROGRAMS"]   = 0.20,
        ["ADMIN"]      = 0.05
    };

    // ─────────── job titles (lifted from ManufacturingIndustryData) ───────────

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // Executive (5)
        new("PLANT_MGR",       "Plant Manager",            5, 12_000m, 20_000m, "PRODUCTION",  null,        true,  "Bachelor's Degree", 10, "Operations Management, Manufacturing"),
        new("PROD_DIR",        "Production Director",      5, 10_000m, 18_000m, "PRODUCTION",  "PLANT_MGR", true,  "Bachelor's Degree", 8,  "Production Management, Lean Manufacturing"),
        new("QUALITY_DIR",     "Quality Director",         5, 9_000m,  16_000m, "QC",          "PLANT_MGR", true,  "Bachelor's Degree", 8,  "Quality Management, ISO Standards"),
        // Senior (4)
        new("PROD_MGR",        "Production Manager",       4, 7_000m,  12_000m, "PRODUCTION",  "PROD_DIR",  true,  "Bachelor's Degree", 6,  "Production Planning, Team Management"),
        new("QUALITY_MGR",     "Quality Manager",          4, 6_500m,  11_000m, "QC",          "QUALITY_DIR", true,"Bachelor's Degree", 5,  "Quality Control, Process Improvement"),
        new("MAINT_MGR",       "Maintenance Manager",      4, 7_000m,  12_000m, "MAINTENANCE", null,        true,  "Bachelor's Degree", 6,  "Maintenance Management, Equipment Reliability"),
        new("HSE_MGR",         "HSE Manager",              4, 6_500m,  11_000m, "HSE",         null,        true,  "Bachelor's Degree", 5,  "Safety Management, Compliance"),
        new("SUPPLY_MGR",      "Supply Chain Manager",     4, 7_000m,  12_000m, "SUPPLY",      null,        true,  "Bachelor's Degree", 5,  "Supply Chain, Procurement, Logistics"),
        new("SENIOR_ENG",      "Senior Engineer",          4, 8_000m,  14_000m, "ENG",         null,        true,  "Bachelor's Degree", 6,  "Process Engineering, Design"),
        // Mid (3)
        new("PROD_SUPER",      "Production Supervisor",    3, 4_000m,  7_000m,  "PRODUCTION",  "PROD_MGR",  true,  "Diploma",           3,  "Production Supervision, Shift Management"),
        new("QC_INSPECTOR",    "Quality Inspector",        3, 3_500m,  6_000m,  "QC",          "QUALITY_MGR",false,"Diploma",           2,  "Quality Inspection, Testing"),
        new("MAINT_TECH",      "Maintenance Technician",   3, 4_000m,  7_000m,  "MAINTENANCE", "MAINT_MGR", false, "Diploma",           3,  "Equipment Maintenance, Troubleshooting"),
        new("PROD_OPERATOR",   "Production Operator",      3, 2_500m,  5_000m,  "PRODUCTION",  "PROD_SUPER",false, "High School",       2,  "Machine Operation, Production"),
        new("HSE_OFFICER",     "HSE Officer",              3, 3_500m,  6_000m,  "HSE",         "HSE_MGR",   false, "Diploma",           2,  "Safety Inspections, Compliance"),
        new("PROC_OFFICER",    "Procurement Officer",      3, 4_000m,  7_000m,  "SUPPLY",      "SUPPLY_MGR",false, "Bachelor's Degree", 2,  "Procurement, Vendor Management"),
        new("PROCESS_ENG",     "Process Engineer",         3, 6_000m,  10_000m, "ENG",         "SENIOR_ENG",false, "Bachelor's Degree", 3,  "Process Design, Improvement"),
        // Junior (2)
        new("JUNIOR_OPERATOR", "Junior Production Operator",2, 2_000m,  3_500m, "PRODUCTION",  "PROD_OPERATOR", false,"High School",    1,  "Basic Machine Operation"),
        new("QC_ASSIST",       "Quality Assistant",        2, 2_500m,  4_000m,  "QC",          "QC_INSPECTOR",false,"High School",     1,  "Quality Testing Support"),
        new("MAINT_ASSIST",    "Maintenance Assistant",    2, 2_500m,  4_500m,  "MAINTENANCE", "MAINT_TECH", false, "High School",      1,  "Maintenance Support, Cleaning"),
        new("WH_ASSIST",       "Warehouse Assistant",      2, 2_000m,  3_500m,  "SUPPLY",      "PROC_OFFICER",false,"High School",     1,  "Inventory, Material Handling"),
        // Entry (1)
        new("PROD_TRAINEE",    "Production Trainee",       1, 1_500m,  2_500m,  "PRODUCTION",  "JUNIOR_OPERATOR",false,"High School",  0,  "Learning, Supervised Operations"),
        new("APPRENTICE",      "Apprentice",               1, 1_500m,  2_500m,  "MAINTENANCE", "MAINT_ASSIST",false,"Student",         0,  "Learning, Technical Training")
    ];
}
