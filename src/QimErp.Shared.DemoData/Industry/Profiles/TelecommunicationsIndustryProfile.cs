using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

public sealed class TelecommunicationsIndustryProfile : IIndustryProfile
{
    public string Code => "TELECOMMUNICATIONS";
    public string DisplayName => "Telecommunications";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "MTN Ghana", "Telecel Ghana", "AT Ghana", "Vodafone Cable",
        "Ghana Cable", "Surfline Communications", "Busy Internet",
        "Glo Mobile Ghana", "Expresso Telecom", "Comsys Ghana",
        "K-Net", "Teledata ICT"
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
            Address: "Independence Avenue, Accra",
            CapacityMin: 80,
            CapacityMax: tier == CompanyTier.Corporate ? 1500 : 350);

        var regionalCount = tier switch
        {
            CompanyTier.Startup   => 0,
            CompanyTier.SME       => Math.Max(1, targetEmployees / 200),
            CompanyTier.Corporate => Math.Max(3, Math.Min(10, targetEmployees / 200)),
            CompanyTier.NonProfit => 1,
            _                     => 2
        };

        var regional = new List<StationSpec>(regionalCount);
        for (var i = 0; i < regionalCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            regional.Add(new StationSpec(
                Code: $"REG{i + 1:D2}",
                Name: $"{city} Regional Office",
                StationType: "Regional Office",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 15,
                CapacityMax: 120));
        }

        var yardCount = (regionalCount * 3) / 2;
        var yards = new List<StationSpec>(yardCount);
        for (var i = 0; i < yardCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            yards.Add(new StationSpec(
                Code: $"YARD{i + 1:D3}",
                Name: $"{city} Maintenance Yard",
                StationType: "Maintenance Yard",
                Region: region,
                City: city,
                Address: $"Industrial Area, {city}",
                CapacityMin: 3,
                CapacityMax: 20));
        }

        return new StationLayout(hq, regional, yards);
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.005,
            [4] = 0.040,
            [3] = 0.250,
            [2] = 0.500,
            [1] = 0.205
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (14_000m, 30_000m),
            [4] = (7_000m,  16_000m),
            [3] = (3_500m,  14_000m), // top of band accommodates SENIOR_NET_ENG (rank 3 in source).
            [2] = (2_500m,   7_000m),
            [1] = (2_000m,   4_000m)
        });

    private static readonly IReadOnlyList<string> NetOpsJobs    = ["NET_DIR", "NET_MGR", "NET_ENG", "SENIOR_NET_ENG", "JUNIOR_NET_ENG", "NET_TRAINEE"];
    private static readonly IReadOnlyList<string> CustomerJobs  = ["CUSTOMER_MGR", "CSR", "CUSTOMER_ASSIST", "CUSTOMER_TRAINEE"];
    private static readonly IReadOnlyList<string> ItJobs        = ["CTO", "IT_MGR", "IT_ENG"];
    private static readonly IReadOnlyList<string> SalesJobs     = ["SALES_MGR", "SALES_EXEC", "SALES_ASSIST"];
    private static readonly IReadOnlyList<string> TechJobs      = ["TECH_SUPPORT_MGR", "TECH_SUPPORT_ENG", "FIELD_TECH", "JUNIOR_FIELD_TECH"];
    private static readonly IReadOnlyList<string> FinanceJobs   = [];
    private static readonly IReadOnlyList<string> HrJobs        = [];
    private static readonly IReadOnlyList<string> ExecJobs      = ["CEO"];
    private static readonly IReadOnlyList<string> ProgramsJobs  = ["NET_MGR", "TECH_SUPPORT_MGR"];
    private static readonly IReadOnlyList<string> AdminJobs     = [];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",  "Founder/CEO",       null,      OrgUnitKind.Executive, ExecJobs),
        new("NETOPS",   "Network Operations","FOUNDER", OrgUnitKind.Function,  NetOpsJobs),
        new("CUSTOMER", "Customer Service",  "FOUNDER", OrgUnitKind.Function,  CustomerJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]  = 0.20,
        ["NETOPS"]   = 0.40,
        ["CUSTOMER"] = 0.40
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",     "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("NETOPS",   "Network Operations","EXEC", OrgUnitKind.Function,  NetOpsJobs),
        new("CUSTOMER", "Customer Service",  "EXEC", OrgUnitKind.Function,  CustomerJobs),
        new("IT",       "IT",                "EXEC", OrgUnitKind.Function,  ItJobs),
        new("SALES",    "Sales & Marketing", "EXEC", OrgUnitKind.Function,  SalesJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.05,
        ["NETOPS"]   = 0.30,
        ["CUSTOMER"] = 0.35,
        ["IT"]       = 0.15,
        ["SALES"]    = 0.15
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",         "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("NETOPS",       "Network Operations","EXEC", OrgUnitKind.Function,  NetOpsJobs),
        new("CUSTOMER",     "Customer Service",  "EXEC", OrgUnitKind.Function,  CustomerJobs),
        new("IT",           "IT",                "EXEC", OrgUnitKind.Function,  ItJobs),
        new("SALES",        "Sales & Marketing", "EXEC", OrgUnitKind.Function,  SalesJobs),
        new("TECH_SUPPORT", "Technical Support", "EXEC", OrgUnitKind.Function,  TechJobs),
        new("FINANCE",      "Finance",           "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("HR",           "HR",                "EXEC", OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]         = 0.04,
        ["NETOPS"]       = 0.25,
        ["CUSTOMER"]     = 0.25,
        ["IT"]           = 0.12,
        ["SALES"]        = 0.12,
        ["TECH_SUPPORT"] = 0.15,
        ["FINANCE"]      = 0.04,
        ["HR"]           = 0.03
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("NETOPS",   "Network Operations","EXEC", OrgUnitKind.Function,  NetOpsJobs),
        new("PROGRAMS", "Programs",          "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("ADMIN",    "Admin",             "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.10,
        ["NETOPS"]   = 0.40,
        ["PROGRAMS"] = 0.35,
        ["ADMIN"]    = 0.15
    };

    // SENIOR_NET_ENG is rank 3 (not 4) per the IAM source — preserved for parity.
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        new("CEO",                "Chief Executive Officer",  5, 18_000m, 30_000m, null,           null,        true,  "Master's Degree",   12, "Telecom Leadership, Strategic Planning"),
        new("CTO",                "Chief Technology Officer", 5, 16_000m, 28_000m, "IT",           "CEO",       true,  "Master's Degree",   10, "Technology Leadership, Network Architecture"),
        new("NET_DIR",            "Network Director",         5, 14_000m, 24_000m, "NETOPS",       "CEO",       true,  "Master's Degree",   10, "Network Operations, Infrastructure"),
        new("NET_MGR",            "Network Manager",          4, 9_000m,  15_000m, "NETOPS",       "NET_DIR",   true,  "Bachelor's Degree", 6,  "Network Management, Operations"),
        new("CUSTOMER_MGR",       "Customer Service Manager", 4, 7_000m,  12_000m, "CUSTOMER",     null,        true,  "Bachelor's Degree", 5,  "Customer Service, Support Management"),
        new("SALES_MGR",          "Sales Manager",            4, 8_000m,  14_000m, "SALES",        null,        true,  "Bachelor's Degree", 5,  "Sales Management, Business Development"),
        new("TECH_SUPPORT_MGR",   "Technical Support Manager",4, 8_500m,  14_000m, "TECH_SUPPORT", null,        true,  "Bachelor's Degree", 6,  "Technical Support, Field Services"),
        new("IT_MGR",             "IT Manager",               4, 10_000m, 16_000m, "IT",           "CTO",       true,  "Bachelor's Degree", 6,  "IT Operations, Systems Management"),
        new("NET_ENG",            "Network Engineer",         3, 7_000m,  12_000m, "NETOPS",       "NET_MGR",   false, "Bachelor's Degree", 4,  "Network Engineering, Configuration, Troubleshooting"),
        new("SENIOR_NET_ENG",     "Senior Network Engineer",  3, 8_500m,  14_000m, "NETOPS",       "NET_MGR",   false, "Bachelor's Degree", 5,  "Advanced Network Engineering, Design"),
        new("CSR",                "Customer Service Representative",3,3_500m,6_000m,"CUSTOMER",    "CUSTOMER_MGR",false,"High School",     2,  "Customer Service, Support, Billing"),
        new("TECH_SUPPORT_ENG",   "Technical Support Engineer",3, 6_000m, 10_000m, "TECH_SUPPORT", "TECH_SUPPORT_MGR",false,"Bachelor's Degree",3,"Technical Support, Troubleshooting, Installations"),
        new("FIELD_TECH",         "Field Technician",         3, 5_000m,  9_000m,  "TECH_SUPPORT", "TECH_SUPPORT_MGR",false,"Diploma",     3,  "Field Installations, Maintenance, Repairs"),
        new("SALES_EXEC",         "Sales Executive",          3, 5_000m,  9_000m,  "SALES",        "SALES_MGR", false, "Bachelor's Degree", 2,  "Sales, Account Management, Prospecting"),
        new("IT_ENG",             "IT Engineer",              3, 7_000m,  12_000m, "IT",           "IT_MGR",    false, "Bachelor's Degree", 3,  "IT Systems, Infrastructure, Support"),
        new("JUNIOR_NET_ENG",     "Junior Network Engineer",  2, 4_000m,  7_000m,  "NETOPS",       "NET_ENG",   false, "Diploma",           1,  "Network Support, Basic Configuration"),
        new("CUSTOMER_ASSIST",    "Customer Service Assistant",2,2_500m,  4_500m,  "CUSTOMER",     "CSR",       false, "High School",       1,  "Customer Support, Basic Inquiries"),
        new("JUNIOR_FIELD_TECH",  "Junior Field Technician",  2, 3_000m,  5_500m,  "TECH_SUPPORT", "FIELD_TECH",false, "High School",       1,  "Field Support, Learning"),
        new("SALES_ASSIST",       "Sales Assistant",          2, 3_000m,  5_500m,  "SALES",        "SALES_EXEC",false, "High School",       1,  "Sales Support, Data Entry"),
        new("NET_TRAINEE",        "Network Trainee",          1, 2_000m,  4_000m,  "NETOPS",       "JUNIOR_NET_ENG",false,"Student",       0,  "Learning, Network Support"),
        new("CUSTOMER_TRAINEE",   "Customer Service Trainee", 1, 2_000m,  3_500m,  "CUSTOMER",     "CUSTOMER_ASSIST",false,"High School",  0,  "Learning, Customer Support"),
        // ── Expansion ────────────────────────────────────────────────────────
        new("CCO_TELCO",          "Chief Commercial Officer",   5, 16_000m, 28_000m, "SALES",       null,           true, "Master's Degree",  10, "Commercial Strategy, Revenue"),
        new("CTO_TELCO",          "Chief Technology Officer",   5, 16_000m, 28_000m, "NETOPS",      null,           true, "Master's Degree",  12, "Network Strategy, Architecture"),
        new("HEAD_NETWORK",       "Head of Network Operations", 5, 13_000m, 22_000m, "NETOPS",      "CTO_TELCO",    true, "Master's Degree",  10, "Network Performance, NOC Operations"),
        new("HEAD_CUSTOMER",      "Head of Customer Experience",5, 12_000m, 20_000m, "CUSTOMER",    null,           true, "Master's Degree",  10, "CX Strategy, NPS, Service Quality"),
        new("HEAD_TECH_SUPPORT",  "Head of Technical Support",  5, 11_000m, 18_000m, "TECH_SUPPORT",null,           true, "Master's Degree",  10, "Tier-3 Support, SLAs"),
        new("RF_ENG",             "RF Engineer",                4, 8_000m,  13_500m, "NETOPS",      "HEAD_NETWORK", true, "Bachelor's in EE", 6,  "RF Planning, Site Survey, Optimization"),
        new("CORE_NETWORK_ENG",   "Core Network Engineer",      4, 8_500m,  14_000m, "NETOPS",      "HEAD_NETWORK", true, "Bachelor's in CS", 6,  "Core Network, Signalling, Routing"),
        new("FIBRE_ENG",          "Fibre & Transmission Engineer",4,7_500m, 12_500m, "NETOPS",      "HEAD_NETWORK", true, "Bachelor's in EE", 5,  "Fibre, DWDM, Microwave Links"),
        new("BSS_ENG",            "BSS Engineer",               4, 8_000m,  13_500m, "IT",          null,           true, "Bachelor's in CS", 6,  "Billing, CRM, Order Management"),
        new("OSS_ENG",            "OSS Engineer",               4, 8_000m,  13_500m, "NETOPS",      "HEAD_NETWORK", true, "Bachelor's in CS", 6,  "Network Management, Provisioning"),
        new("REGIONAL_NOC",       "Regional NOC Manager",       4, 7_500m,  12_500m, "NETOPS",      "HEAD_NETWORK", true, "Bachelor's Degree",6,  "Regional Network, Incident Management"),
        new("FIELD_TECH",         "Field Technician",           3, 4_500m,  8_000m,  "NETOPS",      "REGIONAL_NOC", false,"Diploma",         2,  "Site Maintenance, Tower Climbing"),
        new("NOC_OPERATOR",       "NOC Operator",               3, 4_500m,  7_500m,  "NETOPS",      "REGIONAL_NOC", false,"Diploma",         2,  "24/7 Network Monitoring, First Response"),
        new("DATA_ANALYST_TELCO", "Data Analyst",               3, 5_000m,  9_000m,  "IT",          null,           false,"Bachelor's Degree",2, "Network KPIs, Customer Analytics"),
        new("BUSINESS_DEV_TELCO", "Business Development Officer",3,5_500m,  9_500m,  "SALES",       "CCO_TELCO",    false,"Bachelor's Degree",3, "Enterprise Sales, Partnerships"),
        new("ACCOUNT_MGR_TELCO",  "Account Manager",            3, 5_000m,  9_000m,  "SALES",       "CCO_TELCO",    false,"Bachelor's Degree",2, "Enterprise Account Management"),
        new("RETENTION_OFFICER",  "Customer Retention Officer", 3, 4_000m,  7_500m,  "CUSTOMER",    "HEAD_CUSTOMER",false,"Bachelor's Degree",2, "Churn Reduction, Loyalty Programs"),
        new("SOC_ANALYST",        "SOC Analyst",                3, 6_000m,  10_500m, "IT",          null,           false,"Bachelor's in CS", 3,  "Security Monitoring, Incident Triage"),
        new("CALL_CENTER",        "Call Centre Agent",          2, 2_500m,  4_500m,  "CUSTOMER",    "CUSTOMER_ASSIST",false,"High School",  0,  "Inbound Calls, Customer Inquiries"),
        new("SHOP_AGENT",         "Service Centre Agent",       2, 2_500m,  4_500m,  "CUSTOMER",    "CUSTOMER_ASSIST",false,"High School",  1,  "Walk-in Service, SIM Sales"),
        new("FIBRE_INSTALLER",    "Fibre Installer",            2, 3_000m,  5_500m,  "NETOPS",      "FIELD_TECH",   false,"Trade Certificate",1, "Last-mile Fibre Installation"),
        new("NETWORK_INTERN",     "Network Engineering Intern", 1, 1_800m,  3_500m,  "NETOPS",      "FIELD_TECH",   false,"Student",         0, "Learning, Field Engineering Support"),
        new("BSS_INTERN",         "BSS Intern",                 1, 1_800m,  3_500m,  "IT",          "BSS_ENG",      false,"Student",         0, "Learning, BSS Support")
    ];
}
