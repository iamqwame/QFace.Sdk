namespace QimErp.Shared.DemoData.Industry.Profiles;

/// <summary>
/// Salon &amp; beauty retail chain — floor teams, back-office HR, and operations (POS / scheduling).
/// Org unit names align with <see cref="QimErp.Shared.Common.Workflow.Definitions.EmployeeCreateApprovalWorkflowDefinition.SalonBeautyDepartmentNames"/>.
/// </summary>
public sealed class SalonBeautyIndustryProfile : IIndustryProfile
{
    public string Code => "SALON_BEAUTY";
    public string DisplayName => "Salon & Beauty";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "Nail Bar Ghana", "Shea Glow Studios", "Accra Hair Lounge", "Kumasi Beauty House",
        "Glow & Grace Salons", "Silk & Shears", "Radiance Beauty Group", "Urban Curls Studio"
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
            Name: "Flagship Salon - Accra",
            StationType: "Flagship Salon",
            Region: "Greater Accra",
            City: "Accra",
            Address: "Osu, Accra",
            CapacityMin: 15,
            CapacityMax: tier == CompanyTier.Corporate ? 120 : 45);

        var branchCount = tier switch
        {
            CompanyTier.Startup   => 0,
            CompanyTier.SME       => 1,
            CompanyTier.Corporate => Math.Min(4, Math.Max(2, targetEmployees / 40)),
            _                     => 1
        };

        var branches = new List<StationSpec>(branchCount);
        for (var i = 0; i < branchCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            branches.Add(new StationSpec(
                Code: $"SLN{i + 1:D2}",
                Name: $"{city} Salon",
                StationType: "Salon Branch",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 8,
                CapacityMax: 35));
        }

        return new StationLayout(hq, branches, new List<StationSpec>());
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.020,
            [4] = 0.080,
            [3] = 0.280,
            [2] = 0.450,
            [1] = 0.170
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (8_000m, 18_000m),
            [4] = (5_000m, 12_000m),
            [3] = (3_500m,  8_000m),
            [2] = (2_000m,  5_000m),
            [1] = (1_200m,  3_000m)
        });

    private static readonly IReadOnlyList<string> ExecJobs     = ["OWN-001", "GM-001"];
    private static readonly IReadOnlyList<string> AdminJobs    = ["ADM-001", "HRA-001", "REC-001"];
    private static readonly IReadOnlyList<string> OpsJobs      = ["OPS-001", "POS-001", "INV-001"];
    private static readonly IReadOnlyList<string> FloorJobs    = ["FLM-001", "SSY-001", "STY-001", "JNR-001", "APP-001"];
    private static readonly IReadOnlyList<string> RetailJobs   = ["RTL-001"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("OWNER", "Owner", null, OrgUnitKind.Executive, ExecJobs),
        new("ADMIN", "Salon Administration", "OWNER", OrgUnitKind.Function, AdminJobs),
        new("FLOOR", "Salon Floor", "OWNER", OrgUnitKind.Function, FloorJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["OWNER"] = 0.10,
        ["ADMIN"] = 0.20,
        ["FLOOR"] = 0.70
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC", "Executive Office", null, OrgUnitKind.Executive, ExecJobs),
        new("ADMIN", "Salon Administration", "EXEC", OrgUnitKind.Function, AdminJobs),
        new("OPS", "Salon Operations", "EXEC", OrgUnitKind.Function, OpsJobs),
        new("FLOOR", "Salon Floor", "EXEC", OrgUnitKind.Function, FloorJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]  = 0.05,
        ["ADMIN"] = 0.15,
        ["OPS"]   = 0.15,
        ["FLOOR"] = 0.65
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC", "Executive Office", null, OrgUnitKind.Executive, ExecJobs,
            Description: "Owner and general management of {Company}",
            BudgetMin: 400_000m, BudgetMax: 900_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Lead {Company} brand, expansion, and profitability",
            Phone: "+233 30 200 0100", Email: "executive"),
        new("ADMIN", "Salon Administration", "EXEC", OrgUnitKind.Function, AdminJobs,
            Description: "HR, recruitment, and people operations for {Company}",
            BudgetMin: 350_000m, BudgetMax: 750_000m,
            CostCenter: "CC-ADMIN-001",
            Purpose: "Hire, onboard, and support stylists and floor staff across {Company}",
            Phone: "+233 30 200 0200", Email: "administration"),
        new("OPS", "Salon Operations", "EXEC", OrgUnitKind.Function, OpsJobs,
            Description: "POS, scheduling, inventory, and salon systems for {Company}",
            BudgetMin: 300_000m, BudgetMax: 650_000m,
            CostCenter: "CC-OPS-001",
            Purpose: "Run booking systems, retail inventory, and branch tooling at {Company}",
            Phone: "+233 30 200 0300", Email: "operations"),
        new("FLOOR", "Salon Floor", "EXEC", OrgUnitKind.Function, FloorJobs,
            Description: "Stylists, floor managers, and client service teams at {Company}",
            BudgetMin: 1_200_000m, BudgetMax: 2_500_000m,
            CostCenter: "CC-FLOOR-001",
            Purpose: "Deliver client services and meet revenue targets at {Company} salons",
            Phone: "+233 30 200 0400", Email: "floor")
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]  = 0.03,
        ["ADMIN"] = 0.12,
        ["OPS"]   = 0.15,
        ["FLOOR"] = 0.70
    };

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        new("OWN-001", "Owner / Managing Director", 5, 12_000m, 18_000m, "EXEC", null, true, "Diploma / Experience", 10,
            "Salon Operations, P&L, Brand Growth, Staff Leadership",
            "Owns {Company} strategy and salon network performance", "L5",
            "Set brand direction; approve major hires; manage P&L; open new locations",
            "Revenue Growth, Client NPS, Location Profitability", 30),
        new("GM-001", "General Manager", 4, 8_000m, 12_000m, "EXEC", "OWN-001", true, "Bachelor's Degree", 6,
            "Multi-site Operations, Salon Management, KPI Tracking",
            "Runs day-to-day operations across {Company} locations", "L4",
            "Oversee floor managers; monitor KPIs; coordinate admin and ops teams",
            "Same-store Sales, Staff Utilisation, Client Retention", 28),
        new("ADM-001", "Salon Administrator", 3, 5_500m, 8_500m, "ADMIN", null, false, "Diploma", 3,
            "HR Admin, Scheduling Support, Employee Records, Onboarding",
            "Back-office HR support for {Company} stylists and floor staff", "L3",
            "Maintain employee files; support hiring paperwork; coordinate onboarding",
            "Onboarding SLA, Record Accuracy, HR Query Response", 22),
        new("HRA-001", "HR & People Coordinator", 2, 3_500m, 5_500m, "ADMIN", "ADM-001", false, "Diploma", 1,
            "Recruitment Coordination, Leave Admin, Policy Support",
            "Coordinates people processes for {Company}", "L2",
            "Schedule interviews; track leave; assist with policy queries",
            "Time-to-Hire, Leave Processing SLA", 20),
        new("REC-001", "Front Desk / Reception Lead", 2, 3_000m, 4_800m, "ADMIN", "ADM-001", false, "High School", 2,
            "Client Greeting, Appointment Booking, Cash Handling",
            "Leads reception and first client touchpoint at {Company}", "L2",
            "Manage walk-ins; book appointments; handle retail sales at desk",
            "Booking Fill Rate, Retail Attach Rate", 19),
        new("OPS-001", "Salon Operations Coordinator", 3, 5_000m, 7_500m, "OPS", null, false, "Diploma", 3,
            "POS Systems, Inventory, Branch Tooling, Vendor Coordination",
            "Keeps {Company} salon systems and supplies running", "L3",
            "Manage POS accounts; track inventory; support new-hire system access",
            "System Uptime, Stock-out Rate, New Hire Access SLA", 23),
        new("POS-001", "POS & Scheduling Specialist", 2, 3_200m, 5_000m, "OPS", "OPS-001", false, "Diploma", 1,
            "Salon Software, Appointment Rules, User Provisioning",
            "Configures booking and POS access for {Company} staff", "L2",
            "Create staff logins; configure schedules; troubleshoot POS issues",
            "Access Provisioning SLA, Booking Error Rate", 21),
        new("INV-001", "Retail & Inventory Assistant", 2, 2_800m, 4_500m, "OPS", "OPS-001", false, "High School", 1,
            "Stock Control, Product Merchandising, Supplier Orders",
            "Supports retail product lines at {Company}", "L2",
            "Count stock; place orders; maintain product displays",
            "Shrinkage Rate, Stock Availability", 18),
        new("FLM-001", "Salon Floor Manager", 3, 4_500m, 7_000m, "FLOOR", null, false, "Diploma / Certificate", 4,
            "Team Leadership, Client Service, Shift Planning, Sales Coaching",
            "Manages a {Company} salon floor and hiring supervisor for stylists", "L3",
            "Lead floor team; approve stylist schedules; coach service quality; recommend hires",
            "Floor Revenue, Client NPS, Staff Attendance", 25),
        new("SSY-001", "Senior Stylist", 3, 4_000m, 6_500m, "FLOOR", "FLM-001", false, "Certificate", 5,
            "Cutting, Colour, Client Consultation, Mentoring",
            "Senior service provider and mentor on the {Company} floor", "L3",
            "Deliver advanced services; mentor juniors; maintain client book",
            "Revenue per Hour, Rebooking Rate, Mentor Score", 24),
        new("STY-001", "Stylist", 2, 2_500m, 4_500m, "FLOOR", "FLM-001", false, "Certificate", 2,
            "Hair, Nails, or Beauty Services, Client Care, Upselling",
            "Core service provider on the {Company} salon floor", "L2",
            "Deliver booked services; maintain station hygiene; upsell retail",
            "Service Revenue, Client Satisfaction, Retail Attach", 20),
        new("JNR-001", "Junior Stylist", 1, 1_800m, 3_000m, "FLOOR", "SSY-001", false, "Certificate / Trainee", 0,
            "Basic Services, Assists, Client Prep",
            "Trainee stylist building skills at {Company}", "L1",
            "Assist senior stylists; perform basic services under supervision",
            "Training Progress, Client Feedback", 17),
        new("APP-001", "Apprentice", 1, 1_200m, 2_000m, "FLOOR", "FLM-001", false, "Trainee", 0,
            "Salon Hygiene, Client Prep, Learning",
            "Entry-level trainee on the {Company} floor", "L1",
            "Support floor operations; learn core techniques",
            "Attendance, Skill Checklist Completion", 16),
        new("RTL-001", "Retail Sales Associate", 2, 2_200m, 3_800m, "FLOOR", "FLM-001", false, "High School", 1,
            "Product Sales, Client Recommendations, Merchandising",
            "Sells retail products on the {Company} salon floor", "L2",
            "Recommend products; process sales; maintain displays",
            "Retail Revenue, Conversion Rate", 18)
    ];
}
