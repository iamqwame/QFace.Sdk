using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

public sealed class SoftwareIndustryProfile : IIndustryProfile
{
    public string Code => "SOFTWARE";
    public string DisplayName => "Software & Technology";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "Hubtel", "Expresspay", "mPharma", "IT Consortium", "Asoriba",
        "Bsystems", "Theta Labs", "Soft Tribe", "Ascentech",
        "DreamOval", "Rancard Solutions", "Persol Systems"
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
            Address: "Airport City, Accra",
            CapacityMin: 30,
            CapacityMax: tier == CompanyTier.Corporate ? 600 : 200);

        var officeCount = tier switch
        {
            CompanyTier.Startup   => 0,
            CompanyTier.SME       => 1,
            CompanyTier.Corporate => Math.Min(3, Math.Max(1, targetEmployees / 250)),
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
                Code: $"OFF{i + 1:D2}",
                Name: $"{city} Office",
                StationType: "Branch Office",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 10,
                CapacityMax: 60));
        }

        return new StationLayout(hq, offices, new List<StationSpec>());
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.010,
            [4] = 0.080,
            [3] = 0.450,
            [2] = 0.350,
            [1] = 0.110
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (15_000m, 35_000m),
            [4] = (7_000m,  20_000m),
            [3] = (4_000m,  15_000m),
            [2] = (2_500m,   6_000m),
            [1] = (1_500m,   3_000m)
        });

    private static readonly IReadOnlyList<string> EngJobs      = ["CTO", "VP_ENG", "PRINCIPAL_ENG", "ENG_MGR", "SENIOR_ENG", "LEAD_ENG", "SWE", "DATA_ENG", "JUNIOR_ENG", "INTERN_ENG"];
    private static readonly IReadOnlyList<string> ProductJobs  = ["VP_PRODUCT", "SENIOR_PM", "PM"];
    private static readonly IReadOnlyList<string> DesignJobs   = ["SENIOR_DESIGNER", "DESIGNER", "JUNIOR_DESIGNER", "INTERN_DESIGNER"];
    private static readonly IReadOnlyList<string> QaJobs       = ["QA_ENG", "QA_ANALYST"];
    private static readonly IReadOnlyList<string> DevOpsJobs   = ["DEVOPS_ENG"];
    private static readonly IReadOnlyList<string> ExecJobs     = ["CEO"];
    private static readonly IReadOnlyList<string> SalesJobs    = [];
    private static readonly IReadOnlyList<string> HrJobs       = [];
    private static readonly IReadOnlyList<string> ProgramsJobs = ["PM", "SENIOR_PM"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",  "Founder/CEO", null,      OrgUnitKind.Executive, ExecJobs),
        new("ENGINEER", "Engineering", "FOUNDER", OrgUnitKind.Function,  EngJobs),
        new("DESIGNER", "Design",      "FOUNDER", OrgUnitKind.Function,  DesignJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]  = 0.15,
        ["ENGINEER"] = 0.60,
        ["DESIGNER"] = 0.25
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("CEO",     "Chief Executive", null,  OrgUnitKind.Executive, ExecJobs),
        new("ENG",     "Engineering",     "CEO", OrgUnitKind.Function,  EngJobs),
        new("PRODUCT", "Product",         "CEO", OrgUnitKind.Function,  ProductJobs),
        new("DESIGN",  "Design",          "CEO", OrgUnitKind.Function,  DesignJobs),
        new("QA",      "Quality Assurance","ENG",OrgUnitKind.Function,  QaJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["CEO"]     = 0.05,
        ["ENG"]     = 0.50,
        ["PRODUCT"] = 0.15,
        ["DESIGN"]  = 0.20,
        ["QA"]      = 0.10
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",    "Executive",        null,   OrgUnitKind.Executive, ExecJobs),
        new("ENG",     "Engineering",      "EXEC", OrgUnitKind.Function,  EngJobs),
        new("PRODUCT", "Product",          "EXEC", OrgUnitKind.Function,  ProductJobs),
        new("DESIGN",  "Design",           "EXEC", OrgUnitKind.Function,  DesignJobs),
        new("QA",      "QA & Testing",     "ENG",  OrgUnitKind.Function,  QaJobs),
        new("DEVOPS",  "DevOps",           "ENG",  OrgUnitKind.Function,  DevOpsJobs),
        new("SALES",   "Sales & Marketing","EXEC", OrgUnitKind.Function,  SalesJobs),
        new("HR",      "HR & Admin",       "EXEC", OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]    = 0.05,
        ["ENG"]     = 0.35,
        ["PRODUCT"] = 0.15,
        ["DESIGN"]  = 0.10,
        ["QA"]      = 0.12,
        ["DEVOPS"]  = 0.08,
        ["SALES"]   = 0.10,
        ["HR"]      = 0.05
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive",   null,   OrgUnitKind.Executive, ExecJobs),
        new("ENG",      "Engineering", "EXEC", OrgUnitKind.Function,  EngJobs),
        new("PRODUCT",  "Product",     "EXEC", OrgUnitKind.Function,  ProductJobs),
        new("PROGRAMS", "Programs",    "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("HR",       "HR & Admin",  "EXEC", OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.10,
        ["ENG"]      = 0.40,
        ["PRODUCT"]  = 0.20,
        ["PROGRAMS"] = 0.20,
        ["HR"]       = 0.10
    };

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        new("CEO",             "Chief Executive Officer", 5, 20_000m, 35_000m, null,      null,      true,  "Master's Degree", 10, "Strategic Planning, Leadership, Business Development"),
        new("CTO",             "Chief Technology Officer",5, 18_000m, 30_000m, "ENG",     "CEO",     true,  "Master's Degree", 10, "Technical Leadership, Architecture, Innovation"),
        new("VP_ENG",          "VP Engineering",          5, 15_000m, 25_000m, "ENG",     "CTO",     true,  "Master's Degree", 8,  "Engineering Leadership, Team Management"),
        new("VP_PRODUCT",      "VP Product",              5, 15_000m, 25_000m, "PRODUCT", "CEO",     true,  "Master's Degree", 8,  "Product Strategy, Roadmap Planning"),
        new("PRINCIPAL_ENG",   "Principal Engineer",      4, 12_000m, 20_000m, "ENG",     "VP_ENG",  true,  "Bachelor's Degree", 7, "System Architecture, Technical Design"),
        new("ENG_MGR",         "Engineering Manager",     4, 10_000m, 18_000m, "ENG",     "VP_ENG",  true,  "Bachelor's Degree", 6, "Team Management, Agile, Technical Leadership"),
        new("SENIOR_PM",       "Senior Product Manager",  4, 10_000m, 18_000m, "PRODUCT", "VP_PRODUCT", true, "Bachelor's Degree", 6, "Product Strategy, Analytics, Roadmap"),
        new("SENIOR_ENG",      "Senior Engineer",         4, 8_000m,  15_000m, "ENG",     "ENG_MGR", false, "Bachelor's Degree", 5, "C#, JavaScript, SQL, System Design"),
        new("LEAD_ENG",        "Lead Engineer",           4, 9_000m,  16_000m, "ENG",     "ENG_MGR", true,  "Bachelor's Degree", 5, "Technical Leadership, Code Review, Mentoring"),
        new("SENIOR_DESIGNER", "Senior Designer",         4, 7_000m,  12_000m, "DESIGN",  null,      false, "Bachelor's Degree", 5, "UX/UI Design, Design Systems, User Research"),
        new("SWE",             "Software Engineer",       3, 5_000m,  10_000m, "ENG",     "SENIOR_ENG", false, "Bachelor's Degree", 2, "C#, JavaScript, SQL, Git"),
        new("PM",              "Product Manager",         3, 8_000m,  15_000m, "PRODUCT", "SENIOR_PM", true, "Bachelor's Degree", 3, "Product Strategy, Agile, Analytics"),
        new("DESIGNER",        "UX/UI Designer",          3, 4_000m,  8_000m,  "DESIGN",  "SENIOR_DESIGNER", false, "Bachelor's Degree", 2, "Figma, User Research, Prototyping"),
        new("QA_ENG",          "QA Engineer",             3, 4_000m,  8_000m,  "QA",      null,      false, "Bachelor's Degree", 2, "Testing, Test Automation, Bug Tracking"),
        new("DEVOPS_ENG",      "DevOps Engineer",         3, 6_000m,  12_000m, "DEVOPS",  null,      false, "Bachelor's Degree", 3, "AWS, Docker, Kubernetes, CI/CD"),
        new("DATA_ENG",        "Data Engineer",           3, 6_000m,  11_000m, "ENG",     "SENIOR_ENG", false, "Bachelor's Degree", 3, "Python, SQL, ETL, Data Pipelines"),
        new("JUNIOR_ENG",      "Junior Engineer",         2, 3_000m,  6_000m,  "ENG",     "SWE",     false, "Bachelor's Degree", 1, "Programming Fundamentals, Git"),
        new("JUNIOR_DESIGNER", "Junior Designer",         2, 2_500m,  5_000m,  "DESIGN",  "DESIGNER", false, "Bachelor's Degree", 1, "Design Tools, Basic UX Principles"),
        new("QA_ANALYST",      "QA Analyst",              2, 3_000m,  6_000m,  "QA",      "QA_ENG",  false, "High School", 1, "Manual Testing, Bug Reporting"),
        new("INTERN_ENG",      "Intern Engineer",         1, 1_500m,  3_000m,  "ENG",     "JUNIOR_ENG", false, "Student", 0, "Learning, Basic Programming"),
        new("INTERN_DESIGNER", "Intern Designer",         1, 1_500m,  3_000m,  "DESIGN",  "JUNIOR_DESIGNER", false, "Student", 0, "Learning, Design Basics"),
        // ── Expansion ────────────────────────────────────────────────────────
        new("CTO_SW",          "Chief Technology Officer",  5, 18_000m, 30_000m, "ENG",     null,         true,  "Master's in CS",         12, "Technology Strategy, Architecture"),
        new("CPO",             "Chief Product Officer",     5, 16_000m, 28_000m, "PRODUCT", null,         true,  "Master's Degree",        12, "Product Strategy, Roadmap"),
        new("VP_ENG",          "VP Engineering",            5, 14_000m, 24_000m, "ENG",     "CTO_SW",     true,  "Master's in CS",         10, "Engineering Leadership, Org Scaling"),
        new("HEAD_DESIGN",     "Head of Design",            5, 12_000m, 20_000m, "DESIGN",  null,         true,  "Design Master's",        10, "Design Strategy, UX Vision"),
        new("HEAD_DEVOPS",     "Head of DevOps",            5, 12_000m, 20_000m, "ENG",     "VP_ENG",     true,  "Master's in CS",         8,  "Infrastructure, CI/CD, SRE"),
        new("HEAD_DATA",       "Head of Data",              5, 12_000m, 20_000m, "ENG",     "VP_ENG",     true,  "Master's Degree",        8,  "Data Platform, ML Strategy"),
        new("ENG_MGR",         "Engineering Manager",       4, 9_000m,  15_000m, "ENG",     "VP_ENG",     true,  "Bachelor's in CS",       6,  "Team Leadership, Delivery"),
        new("PRODUCT_MGR",     "Product Manager",           4, 8_500m,  14_000m, "PRODUCT", "CPO",        true,  "Bachelor's Degree",      5,  "Discovery, Backlog, Stakeholders"),
        new("DESIGN_MGR",      "Design Manager",            4, 8_000m,  13_500m, "DESIGN",  "HEAD_DESIGN",true,  "Design Bachelor's",      5,  "Design Reviews, Mentoring"),
        new("PRINCIPAL_ENG",   "Principal Engineer",        4, 12_000m, 20_000m, "ENG",     "ENG_MGR",    false, "Bachelor's in CS",       8,  "Architecture, Technical Direction"),
        new("STAFF_ENG",       "Staff Engineer",            4, 10_000m, 17_000m, "ENG",     "ENG_MGR",    false, "Bachelor's in CS",       7,  "Cross-team Engineering, Mentoring"),
        new("SENIOR_BACKEND",  "Senior Backend Engineer",   3, 8_000m,  13_000m, "ENG",     "ENG_MGR",    false, "Bachelor's in CS",       4,  "Distributed Systems, APIs, Databases"),
        new("SENIOR_FRONTEND", "Senior Frontend Engineer",  3, 8_000m,  13_000m, "ENG",     "ENG_MGR",    false, "Bachelor's in CS",       4,  "React, TypeScript, UX Implementation"),
        new("FULLSTACK",       "Full-Stack Engineer",       3, 6_500m,  11_000m, "ENG",     "ENG_MGR",    false, "Bachelor's in CS",       3,  "Backend + Frontend, End-to-End Features"),
        new("DEVOPS",          "DevOps Engineer",           3, 7_000m,  12_000m, "ENG",     "HEAD_DEVOPS",false, "Bachelor's in CS",       3,  "Kubernetes, Terraform, Cloud Platforms"),
        new("SRE",             "Site Reliability Engineer", 3, 7_500m,  12_500m, "ENG",     "HEAD_DEVOPS",false, "Bachelor's in CS",       3,  "Observability, Incident Response"),
        new("DATA_ENG",        "Data Engineer",             3, 7_000m,  12_000m, "ENG",     "HEAD_DATA",  false, "Bachelor's in CS",       3,  "Pipelines, Warehouse, Streaming"),
        new("ML_ENG",          "Machine Learning Engineer", 3, 8_000m,  13_500m, "ENG",     "HEAD_DATA",  false, "Master's Degree",        4,  "Models, Training Pipelines, MLOps"),
        new("QA_ENGINEER",     "QA Engineer",               3, 5_000m,  9_000m,  "ENG",     "ENG_MGR",    false, "Bachelor's Degree",      2,  "Test Automation, Regression"),
        new("UX_DESIGNER_SW",  "UX Designer",               3, 6_000m,  10_500m, "DESIGN",  "DESIGN_MGR", false, "Design Bachelor's",      3,  "User Research, Wireframes, Prototyping"),
        new("UI_DESIGNER",     "UI Designer",               3, 5_500m,  9_500m,  "DESIGN",  "DESIGN_MGR", false, "Design Bachelor's",      2,  "Visual Design, Design Systems"),
        new("TECHNICAL_WRITER","Technical Writer",          3, 4_500m,  8_000m,  "PRODUCT", "PRODUCT_MGR",false, "Bachelor's Degree",      2,  "Documentation, API Reference"),
        new("SCRUM_MASTER",    "Scrum Master",              3, 5_500m,  9_500m,  "ENG",     "ENG_MGR",    false, "Bachelor's + CSM",       3,  "Agile Coaching, Ceremonies"),
        new("JUNIOR_DEVOPS",   "Junior DevOps Engineer",    2, 4_500m,  7_500m,  "ENG",     "DEVOPS",     false, "Bachelor's in CS",       1,  "Build, Deploy, Monitoring Support"),
        new("SUPPORT_ENG",     "Customer Support Engineer", 2, 4_000m,  7_000m,  "ENG",     null,         false, "Bachelor's Degree",      1,  "Tier-2 Support, Bug Triage"),
        new("INTERN_ENG",      "Engineering Intern",        1, 1_800m,  3_500m,  "ENG",     "FULLSTACK",  false, "Student",                0,  "Learning, Pair Programming"),
        new("INTERN_PM",       "Product Intern",            1, 1_800m,  3_500m,  "PRODUCT", "PRODUCT_MGR",false, "Student",                0,  "Learning, Discovery Support")
    ];
}
