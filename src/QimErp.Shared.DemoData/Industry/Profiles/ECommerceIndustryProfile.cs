using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

/// <summary>
/// E-commerce industry. Lifted from QimErp.IAM.Seeding.Demo's ECommerceIndustryData.
/// Stations are HQ + warehouses + last-mile fulfilment hubs.
/// </summary>
public sealed class ECommerceIndustryProfile : IIndustryProfile
{
    public string Code => "ECOMMERCE";
    public string DisplayName => "E-Commerce & Online Retail";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "Jumia Ghana", "Tonaton", "Jiji Ghana", "Melcom Online", "Glovo Ghana",
        "Bolt Food Ghana", "Hubtel Mall", "Shoprite Online", "Zoobashop",
        "Superprice Online", "OLX Ghana", "Kikuu Ghana"
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
            Address: "Spintex Road, Accra",
            CapacityMin: 40,
            CapacityMax: tier == CompanyTier.Corporate ? 500 : 200);

        var warehouseCount = tier switch
        {
            CompanyTier.Startup   => 1,
            CompanyTier.SME       => Math.Max(1, targetEmployees / 200),
            CompanyTier.Corporate => Math.Max(2, Math.Min(8, targetEmployees / 200)),
            CompanyTier.NonProfit => 1,
            _                     => 2
        };

        var warehouses = new List<StationSpec>(warehouseCount);
        for (var i = 0; i < warehouseCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            warehouses.Add(new StationSpec(
                Code: $"WH{i + 1:D2}",
                Name: $"{city} Warehouse",
                StationType: "Warehouse",
                Region: region,
                City: city,
                Address: $"Industrial Estate, {city}",
                CapacityMin: 25,
                CapacityMax: 200));
        }

        // Last-mile hubs — small dispatch points distributed across cities.
        var hubCount = warehouseCount * 3;
        var hubs = new List<StationSpec>(hubCount);
        for (var i = 0; i < hubCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            hubs.Add(new StationSpec(
                Code: $"HUB{i + 1:D3}",
                Name: $"{city} Last-Mile Hub",
                StationType: "Last-Mile Hub",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 3,
                CapacityMax: 15));
        }

        return new StationLayout(hq, warehouses, hubs);
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.005,
            [4] = 0.040,
            [3] = 0.180,
            [2] = 0.500, // CSR / warehouse / fulfilment make the bulk
            [1] = 0.275
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (12_000m, 25_000m),
            [4] = (6_000m,  15_000m),
            [3] = (3_000m,  12_000m),
            [2] = (2_500m,   5_000m),
            [1] = (1_500m,   3_000m)
        });

    // ─────────── baseline org units (lifted from ECommerceIndustryData) ───────────

    private static readonly IReadOnlyList<string> OpsJobs       = ["COO", "OPS_MGR", "OPS_SUPER", "FULFILLMENT", "OPS_ASSIST", "OPS_TRAINEE"];
    private static readonly IReadOnlyList<string> CustomerJobs  = ["CUSTOMER_MGR", "CSR", "CUSTOMER_ASSIST", "CUSTOMER_TRAINEE"];
    private static readonly IReadOnlyList<string> SalesJobs     = ["SALES_MGR", "SALES_EXEC", "DIGITAL_MKTG", "SALES_ASSIST"];
    private static readonly IReadOnlyList<string> ItJobs        = ["CTO", "IT_MGR", "IT_DEV"];
    private static readonly IReadOnlyList<string> WarehouseJobs = ["WH_MGR", "WH_SUPER", "WH_ASSIST"];
    private static readonly IReadOnlyList<string> FinanceJobs   = [];
    private static readonly IReadOnlyList<string> HrJobs        = [];
    private static readonly IReadOnlyList<string> ExecJobs      = ["CEO"];
    private static readonly IReadOnlyList<string> TechJobs      = ["IT_DEV", "IT_MGR"];
    private static readonly IReadOnlyList<string> ProgramsJobs  = ["OPS_MGR", "OPS_SUPER"];
    private static readonly IReadOnlyList<string> AdminJobs     = [];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER", "Founder/CEO", null,      OrgUnitKind.Executive, ExecJobs),
        new("OPS",     "Operations",  "FOUNDER", OrgUnitKind.Function,  OpsJobs),
        new("TECH",    "Technology",  "FOUNDER", OrgUnitKind.Function,  TechJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"] = 0.20,
        ["OPS"]     = 0.50,
        ["TECH"]    = 0.30
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",     "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("OPS",      "Operations",        "EXEC", OrgUnitKind.Function,  OpsJobs),
        new("CUSTOMER", "Customer Care",     "EXEC", OrgUnitKind.Function,  CustomerJobs),
        new("SALES",    "Sales & Marketing", "EXEC", OrgUnitKind.Function,  SalesJobs),
        new("IT",       "IT",                "EXEC", OrgUnitKind.Function,  ItJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.05,
        ["OPS"]      = 0.40,
        ["CUSTOMER"] = 0.25,
        ["SALES"]    = 0.20,
        ["IT"]       = 0.10
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",      "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("OPS",       "Operations",        "EXEC", OrgUnitKind.Function,  OpsJobs),
        new("CUSTOMER",  "Customer Care",     "EXEC", OrgUnitKind.Function,  CustomerJobs),
        new("SALES",     "Sales & Marketing", "EXEC", OrgUnitKind.Function,  SalesJobs),
        new("IT",        "IT",                "EXEC", OrgUnitKind.Function,  ItJobs),
        new("WAREHOUSE", "Warehouse",         "OPS",  OrgUnitKind.Function,  WarehouseJobs),
        new("FINANCE",   "Finance",           "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("HR",        "HR",                "EXEC", OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]      = 0.04,
        ["OPS"]       = 0.30,
        ["CUSTOMER"]  = 0.20,
        ["SALES"]     = 0.15,
        ["IT"]        = 0.12,
        ["WAREHOUSE"] = 0.10,
        ["FINANCE"]   = 0.05,
        ["HR"]        = 0.04
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive", null,   OrgUnitKind.Executive, ExecJobs),
        new("OPS",      "Operations","EXEC", OrgUnitKind.Function,  OpsJobs),
        new("PROGRAMS", "Programs",  "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("ADMIN",    "Admin",     "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.10,
        ["OPS"]      = 0.50,
        ["PROGRAMS"] = 0.30,
        ["ADMIN"]    = 0.10
    };

    // ─────────── job titles (lifted from ECommerceIndustryData) ───────────

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // Executive (5)
        new("CEO",              "Chief Executive Officer",       5, 15_000m, 25_000m, null,        null,         true,  "Master's Degree",   10, "E-commerce Strategy, Business Leadership"),
        new("COO",              "Chief Operating Officer",       5, 12_000m, 20_000m, "OPS",       "CEO",        true,  "Master's Degree",   8,  "Operations Management, Logistics"),
        new("CTO",              "Chief Technology Officer",      5, 13_000m, 22_000m, "IT",        "CEO",        true,  "Master's Degree",   8,  "Technology Leadership, Platform Development"),
        // Senior (4)
        new("OPS_MGR",          "Operations Manager",            4, 7_000m,  12_000m, "OPS",       "COO",        true,  "Bachelor's Degree", 5,  "Operations Management, Fulfillment"),
        new("CUSTOMER_MGR",     "Customer Care Manager",         4, 6_000m,  10_000m, "CUSTOMER",  null,         true,  "Bachelor's Degree", 5,  "Customer Service, Support Management"),
        new("SALES_MGR",        "Sales Manager",                 4, 7_000m,  12_000m, "SALES",     null,         true,  "Bachelor's Degree", 5,  "Sales Management, Business Development"),
        new("WH_MGR",           "Warehouse Manager",             4, 6_000m,  10_000m, "WAREHOUSE", "COO",        true,  "Bachelor's Degree", 5,  "Warehouse Management, Logistics"),
        new("IT_MGR",           "IT Manager",                    4, 9_000m,  15_000m, "IT",        "CTO",        true,  "Bachelor's Degree", 6,  "IT Operations, E-commerce Platform"),
        // Mid (3)
        new("OPS_SUPER",        "Operations Supervisor",         3, 4_000m,  7_000m,  "OPS",       "OPS_MGR",    true,  "Diploma",           3,  "Operations Supervision, Order Processing"),
        new("CSR",              "Customer Service Representative",3, 3_000m, 5_500m,  "CUSTOMER",  "CUSTOMER_MGR",false,"High School",      2,  "Customer Service, Support, Returns"),
        new("SALES_EXEC",       "Sales Executive",               3, 4_000m,  7_000m,  "SALES",     "SALES_MGR",  false, "Bachelor's Degree", 2,  "Sales, Account Management, Business Development"),
        new("WH_SUPER",         "Warehouse Supervisor",          3, 4_000m,  7_000m,  "WAREHOUSE", "WH_MGR",     true,  "Diploma",           2,  "Warehouse Operations, Inventory"),
        new("FULFILLMENT",      "Order Fulfillment Specialist",  3, 3_000m,  5_500m,  "OPS",       "OPS_SUPER",  false, "High School",       2,  "Order Processing, Packing, Shipping"),
        new("IT_DEV",           "IT Developer",                  3, 7_000m,  12_000m, "IT",        "IT_MGR",     false, "Bachelor's Degree", 3,  "Web Development, E-commerce Platform"),
        new("DIGITAL_MKTG",     "Digital Marketing Specialist",  3, 5_000m,  9_000m,  "SALES",     "SALES_MGR",  false, "Bachelor's Degree", 2,  "Digital Marketing, SEO, Social Media"),
        // Junior (2)
        new("OPS_ASSIST",       "Operations Assistant",          2, 2_500m,  4_500m,  "OPS",       "OPS_SUPER",  false, "High School",       1,  "Operations Support, Order Entry"),
        new("CUSTOMER_ASSIST",  "Customer Service Assistant",    2, 2_500m,  4_000m,  "CUSTOMER",  "CSR",        false, "High School",       1,  "Customer Support, Basic Inquiries"),
        new("WH_ASSIST",        "Warehouse Assistant",           2, 2_500m,  4_000m,  "WAREHOUSE", "WH_SUPER",   false, "High School",       1,  "Warehouse Support, Picking, Packing"),
        new("SALES_ASSIST",     "Sales Assistant",               2, 3_000m,  5_000m,  "SALES",     "SALES_EXEC", false, "High School",       1,  "Sales Support, Data Entry"),
        // Entry (1)
        new("OPS_TRAINEE",      "Operations Trainee",            1, 1_500m,  3_000m,  "OPS",       "OPS_ASSIST", false, "High School",       0,  "Learning, Operations Support"),
        new("CUSTOMER_TRAINEE", "Customer Service Trainee",      1, 1_500m,  3_000m,  "CUSTOMER",  "CUSTOMER_ASSIST", false,"High School",   0,  "Learning, Customer Support")
    ];
}
