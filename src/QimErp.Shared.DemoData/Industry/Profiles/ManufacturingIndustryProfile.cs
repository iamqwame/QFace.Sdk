namespace QimErp.Shared.DemoData.Industry.Profiles;

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
        // Corporate tier uses the curated 45-station manufacturing catalogue verbatim — full
        // names, addresses, codes, descriptions, phones, emails, station types modelled on a
        // tier-1 Ghana FMCG / heavy-manufacturer footprint (HQ + plants + distribution centres
        // + sales depots + trade offices). Other tiers fall back to the procedural city-pool
        // builder so smaller operators land with a reasonable shape.
        if (tier == CompanyTier.Corporate)
        {
            var hqRow = _manufacturingStations[0];
            var rest = _manufacturingStations.Skip(1).ToList();
            var branchTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Plant", "Distribution Centre", "Warehouse"
            };
            return new StationLayout(
                Headquarters: hqRow,
                Branches: rest.Where(s => branchTypes.Contains(s.StationType)).ToList(),
                Satellites: rest.Where(s => !branchTypes.Contains(s.StationType)).ToList());
        }

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
            [1] = 0.370
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

    private static readonly IReadOnlyList<string> ExecJobs        = ["CEO-001", "COO-001", "PD-001", "EA-001"];
    private static readonly IReadOnlyList<string> MfgJobs         = ["HOP-001", "PM-001", "SS-001", "LS-001", "PE-001", "ME-001", "MO-001", "PKO-001", "FLO-001", "MXO-001", "CO-001", "PI-001", "PT-001"];
    private static readonly IReadOnlyList<string> QualityJobs     = ["HOQ-001", "QM-001", "QE-001", "QA-001", "QI-001", "LT-001"];
    private static readonly IReadOnlyList<string> MaintJobs       = ["HOM-001", "MTM-001", "ME2-001", "MTE-001", "MTM2-001", "IT2-001"];
    private static readonly IReadOnlyList<string> SupplyJobs      = ["HOSC-001", "PRM-001", "PRO-001", "BUY-001", "PLM-001", "DP-001", "PP-001", "WS-001", "SO-001", "LC-001", "IBC-001", "OBC-001", "FLF-001"];
    private static readonly IReadOnlyList<string> EngJobs         = ["HOE-001", "PRE-001", "ME-001", "AE-001"];
    private static readonly IReadOnlyList<string> RndJobs         = ["HORD-001", "FT-001", "RDS-001"];
    private static readonly IReadOnlyList<string> EhsJobs         = ["HEHS-001", "EHSO-001", "SI-001", "SUS-001"];
    private static readonly IReadOnlyList<string> CommercialJobs  = ["HOC-001", "B2BSM-001", "TSM-001", "TMM-001", "BM2-001", "CSO-001"];
    private static readonly IReadOnlyList<string> FinanceJobs     = ["HOF-001", "PA-001", "CA2-001", "AC-001"];
    private static readonly IReadOnlyList<string> HrJobs          = ["HOHR-001", "HRM-001", "HRO-001", "IRO-001", "TR-001"];
    private static readonly IReadOnlyList<string> ItJobs          = ["IT-001"];
    private static readonly IReadOnlyList<string> AdminJobs       = ["SEC-001", "DR-001", "CL-001"];
    private static readonly IReadOnlyList<string> ProgramsJobs    = ["PM-001", "SS-001"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",    "Founder/CEO", null,      OrgUnitKind.Executive, ExecJobs),
        new("PRODUCTION", "Production",  "FOUNDER", OrgUnitKind.Function,  MfgJobs),
        new("QUALITY",    "Quality",     "FOUNDER", OrgUnitKind.Function,  QualityJobs)
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
        new("PRODUCTION",  "Production",           "EXEC", OrgUnitKind.Function,  MfgJobs),
        new("QC",          "Quality Control",      "EXEC", OrgUnitKind.Function,  QualityJobs),
        new("MAINTENANCE", "Maintenance",          "EXEC", OrgUnitKind.Function,  MaintJobs),
        new("HSE",         "Health, Safety & Env", "EXEC", OrgUnitKind.Function,  EhsJobs),
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

    // Corporate-tier baseline OrgUnits — each carries rich Description / Budget /
    // CostCenter / Purpose / Phone / Email-local-part modelled on a tier-1 Ghana FMCG
    // / heavy-manufacturer (think Kasapreko / Fan Milk / Unilever Ghana / GIHOC scale).
    // The {Company} placeholder gets substituted with the actual tenant's company name
    // at row-emit time so the same catalogue reads naturally for any manufacturer.
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",          "Executive",                       null,   OrgUnitKind.Executive, ExecJobs,
            Description: "Office of the Managing Director and executive leadership team of {Company}",
            BudgetMin: 900_000m, BudgetMax: 1_800_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Set and execute {Company} corporate strategy; lead the executive committee; manage GIPC, FDA, GSA, and Ghana Standards Authority relationships",
            Phone: "+233 30 320 1100", Email: "executive"),
        new("MANUFACTURING", "Production Operations",           "EXEC", OrgUnitKind.Function,  MfgJobs,
            Description: "Operates {Company}'s production lines across all plants — formulation, mixing, filling, packaging, and finished goods",
            BudgetMin: 4_000_000m, BudgetMax: 12_000_000m,
            CostCenter: "CC-MFG-001",
            Purpose: "Deliver agreed volume, quality, and OEE targets at lowest unit cost across {Company} plants while honouring CBA shop-floor agreements",
            Phone: "+233 30 320 1200", Email: "manufacturing"),
        new("QUALITY",       "Quality Assurance & Control",     "EXEC", OrgUnitKind.Function,  QualityJobs,
            Description: "FDA-licensed laboratories, in-process QC, finished-goods release, and ISO 9001 / ISO 22000 system management for {Company}",
            BudgetMin: 600_000m, BudgetMax: 1_400_000m,
            CostCenter: "CC-QA-001",
            Purpose: "Guarantee every batch leaving a {Company} plant meets FDA, GSA, and customer specifications; maintain HACCP and ISO certifications",
            Phone: "+233 30 320 1300", Email: "quality"),
        new("MAINTENANCE",   "Engineering Maintenance",         "EXEC", OrgUnitKind.Function,  MaintJobs,
            Description: "Plant reliability, preventive and predictive maintenance, utilities, and spares management across {Company}'s production assets",
            BudgetMin: 1_200_000m, BudgetMax: 3_000_000m,
            CostCenter: "CC-MTC-001",
            Purpose: "Maximise plant availability and OEE through preventive maintenance, condition monitoring, and rapid breakdown response",
            Phone: "+233 30 320 1400", Email: "maintenance"),
        new("SUPPLY-CHAIN",  "Supply Chain (Procurement, Planning & Logistics)", "EXEC", OrgUnitKind.Function, SupplyJobs,
            Description: "End-to-end supply chain — strategic procurement, S&OP / production planning, raw materials warehousing, and outbound distribution for {Company}",
            BudgetMin: 1_500_000m, BudgetMax: 3_500_000m,
            CostCenter: "CC-SCM-001",
            Purpose: "Secure raw and packaging materials at the right cost; balance demand and supply through S&OP; deliver finished goods on time, in full",
            Phone: "+233 30 320 1500", Email: "supplychain"),
        new("ENGINEERING",   "Process & Project Engineering",   "EXEC", OrgUnitKind.Function,  EngJobs,
            Description: "Process design, capex project delivery, automation, and continuous improvement engineering for {Company}'s manufacturing footprint",
            BudgetMin: 800_000m, BudgetMax: 2_000_000m,
            CostCenter: "CC-ENG-001",
            Purpose: "Deliver capacity expansion projects on-time / on-budget; drive process optimisation, automation, and World Class Manufacturing initiatives",
            Phone: "+233 30 320 1600", Email: "engineering"),
        new("R&D",           "Research & Development",          "EXEC", OrgUnitKind.Function,  RndJobs,
            Description: "Product innovation, formulation, food technology, and pilot-plant trials supporting {Company}'s new product pipeline",
            BudgetMin: 500_000m, BudgetMax: 1_300_000m,
            CostCenter: "CC-RND-001",
            Purpose: "Develop new {Company} products and reformulations that hit consumer needs, regulatory requirements, and target margins",
            Phone: "+233 30 320 1700", Email: "rnd"),
        new("EHS",           "Environment, Health & Safety",    "EXEC", OrgUnitKind.Function,  EhsJobs,
            Description: "Occupational safety, environmental compliance, EPA permits, sustainability reporting, and emergency response for {Company} sites",
            BudgetMin: 350_000m, BudgetMax: 800_000m,
            CostCenter: "CC-EHS-001",
            Purpose: "Achieve zero lost-time injuries; maintain EPA permit compliance; deliver {Company}'s sustainability and decarbonisation roadmap",
            Phone: "+233 30 320 1800", Email: "ehs"),
        new("COMMERCIAL",    "Sales, Trade & Marketing",        "EXEC", OrgUnitKind.Function,  CommercialJobs,
            Description: "B2B and trade sales, key account management, depot operations, brand management, and trade marketing for {Company} brands",
            BudgetMin: 1_500_000m, BudgetMax: 4_000_000m,
            CostCenter: "CC-COM-001",
            Purpose: "Hit volume, value, and market-share targets across {Company} brands; build trade and consumer demand nationwide",
            Phone: "+233 30 320 1900", Email: "commercial"),
        new("FINANCE",       "Finance & Accounts",              "EXEC", OrgUnitKind.Function,  FinanceJobs,
            Description: "Financial reporting, IFRS compliance, GRA tax filings, plant accounting, costing, and treasury operations for {Company}",
            BudgetMin: 700_000m, BudgetMax: 1_500_000m,
            CostCenter: "CC-FIN-001",
            Purpose: "Produce accurate {Company} financial statements; manage working capital and FX; deliver standard cost reporting and variance analysis to the ExCo",
            Phone: "+233 30 320 2000", Email: "finance"),
        new("HR",            "Human Resources & Industrial Relations", "EXEC", OrgUnitKind.Function, HrJobs,
            Description: "Talent acquisition, learning & development, total rewards, payroll, and union / collective bargaining relations for {Company}",
            BudgetMin: 600_000m, BudgetMax: 1_400_000m,
            CostCenter: "CC-HR-001",
            Purpose: "Attract and develop the technical and shop-floor talent {Company} needs; maintain a productive industrial relations climate with the local CBA union",
            Phone: "+233 30 320 2100", Email: "hr"),
        new("IT",            "Information Technology",          "EXEC", OrgUnitKind.Function,  ItJobs,
            Description: "ERP (SAP / Microsoft Dynamics), MES, OT/IT plant networks, cybersecurity, and end-user support for {Company}",
            BudgetMin: 500_000m, BudgetMax: 1_200_000m,
            CostCenter: "CC-IT-001",
            Purpose: "Run {Company}'s ERP and MES platforms with high availability; protect plant control networks; deliver digital initiatives across operations",
            Phone: "+233 30 320 2200", Email: "it")
    ];

    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]          = 0.02,
        ["MANUFACTURING"] = 0.45,
        ["QUALITY"]       = 0.07,
        ["MAINTENANCE"]   = 0.10,
        ["SUPPLY-CHAIN"]  = 0.10,
        ["ENGINEERING"]   = 0.04,
        ["R&D"]           = 0.02,
        ["EHS"]           = 0.03,
        ["COMMERCIAL"]    = 0.08,
        ["FINANCE"]       = 0.04,
        ["HR"]            = 0.03,
        ["IT"]            = 0.02
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",       "Executive",  null,   OrgUnitKind.Executive, ExecJobs),
        new("PRODUCTION", "Production", "EXEC", OrgUnitKind.Function,  MfgJobs),
        new("QUALITY",    "Quality",    "EXEC", OrgUnitKind.Function,  QualityJobs),
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

    // Curated job-title catalogue for a tier-1 Ghana manufacturer (FMCG / beverage /
    // heavy-industrial scale). Names, codes, descriptions, responsibilities, KPIs,
    // pay-grades, and salary bands are IDENTICAL across every tenant seeded with this
    // profile. {Company} placeholder is substituted at row-emit time. Pay-grades:
    // MFG-1 (entry) through MFG-10 (CEO) for staff and management; OPS- for plant-side
    // shift / supervisory; CB- for collective-bargaining (CBA) shop-floor roles.
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // ─── Rank 5 — Executive (CEO / COO / Plant Director / Heads of...) ───────
        new("CEO-001", "Managing Director & CEO", 5, 65_000m, 90_000m, "EXEC", null, true, "Master's Degree (MBA preferred)", 18, "Executive Leadership, Manufacturing Strategy, P&L Management, Stakeholder Engagement, Industrial Relations", "Chief executive accountable for {Company}'s overall strategy, performance, and stakeholder relations", "MFG-10", "Set and execute {Company} strategy; lead the ExCo; chair the management board; engage GIPC, FDA, GSA, EPA and customer principals; deliver growth, profitability, and sustainability targets", "EBITDA Growth, Net Sales Growth, ROCE, Plant OEE, Lost-Time Injury Frequency Rate, Employee Engagement Score", 30),
        new("COO-001", "Chief Operating Officer", 5, 50_000m, 80_000m, "EXEC", "CEO-001", true, "Master's Degree", 15, "Operations Leadership, Multi-Plant Management, Lean Manufacturing, Supply Chain, Programme Delivery", "Executive accountable for end-to-end operations across {Company} plants, supply chain, and quality", "MFG-10", "Run {Company}'s operating system; oversee plants, quality, maintenance, supply chain, and EHS; chair operations review; drive lean and World Class Manufacturing", "Plant OEE, On-Time-In-Full Delivery, Conversion Cost per Tonne, Customer Complaint Rate, Operational Loss Rate", 30),
        new("PD-001", "Plant Director", 5, 45_000m, 75_000m, "MANUFACTURING", "COO-001", true, "Engineering Master's", 14, "Plant Operations, Multi-Line Management, Lean / TPM, Capex Delivery, Industrial Relations", "Senior leader running a {Company} manufacturing plant end-to-end including production, quality, maintenance, and EHS", "MFG-9", "Own plant P&L; deliver volume, quality, OEE, and cost targets; manage CBA and shop-floor relations; lead capex projects; report to the COO", "Plant OEE, Conversion Cost, Customer Service Level, Quality Right-First-Time, LTIFR, Plant Capex Variance", 30),
        new("HOP-001", "Head of Production", 5, 42_000m, 68_000m, "MANUFACTURING", "PD-001", true, "Engineering Bachelor's", 12, "Production Management, Lean Manufacturing, Shift Operations, KPI Management, Coaching", "Leads all {Company} production lines, shift teams, and the daily management system across plants", "MFG-9", "Plan and deliver production schedules; chair daily SIC; drive OEE, yield, and waste targets; develop production talent; coach line managers", "Production Volume vs Plan, OEE %, First-Time Yield, Waste %, Schedule Adherence, Shift Productivity", 30),
        new("HOQ-001", "Head of Quality", 5, 42_000m, 70_000m, "QUALITY", "PD-001", true, "Master's Degree (Food Sci / Chem / Eng)", 12, "Quality Strategy, ISO 9001 / 22000 / HACCP, FDA Regulatory, Lab Management, Audit Programmes", "Leads {Company}'s quality strategy across plants — labs, in-process QC, finished-goods release, and external audits", "MFG-9", "Own quality strategy and management system; chair quality review; manage FDA, GSA and customer audits; drive complaint reduction; lead lab accreditation", "Customer Complaint Rate, Right-First-Time %, FDA Audit Outcome, COA Turnaround, Lab Accreditation Score", 30),
        new("HOM-001", "Head of Maintenance", 5, 42_000m, 70_000m, "MAINTENANCE", "PD-001", true, "Engineering Bachelor's", 12, "Reliability Engineering, TPM, CMMS, Preventive Maintenance, Capex Engineering, Spares Strategy", "Leads {Company}'s engineering maintenance — reliability, preventive maintenance, utilities, and spares strategy", "MFG-9", "Own asset reliability strategy; deliver PM compliance and breakdown reduction; manage spares budget; lead utilities (boilers, compressors, water, power); chair RCM reviews", "Mean Time Between Failures, Mean Time To Repair, PM Compliance %, Maintenance Cost / Tonne, Asset Availability %", 30),
        new("HOSC-001", "Head of Supply Chain", 5, 45_000m, 75_000m, "SUPPLY-CHAIN", "COO-001", true, "Master's Degree (CIPS / CSCP advantage)", 12, "End-to-End Supply Chain, S&OP, Strategic Sourcing, Logistics, Working Capital", "Leads end-to-end supply chain at {Company} — procurement, planning, warehousing, and outbound logistics", "MFG-9", "Own S&OP; deliver service-level and inventory targets; lead strategic sourcing; manage warehousing and distribution; reduce working-capital tied in stock", "Forecast Accuracy, OTIF Delivery, Inventory Days of Cover, Procurement Savings, Logistics Cost / Case", 30),
        new("HOE-001", "Head of Engineering", 5, 42_000m, 70_000m, "ENGINEERING", "COO-001", true, "Engineering Master's", 12, "Process Engineering, Capex Project Management, Automation, World Class Manufacturing, Stage-Gate", "Leads {Company} process and project engineering including capex delivery, automation, and continuous improvement", "MFG-9", "Deliver capex programme on-time / on-budget; lead automation and digitisation roadmap; coach plant CI champions; manage engineering standards", "Capex Delivery vs Plan, Capex Spend Variance, Process Improvement Savings, Project Quality, OEE Uplift", 30),
        new("HORD-001", "Head of R&D / Innovation", 5, 42_000m, 68_000m, "R&D", "COO-001", true, "Master's / PhD (Food Sci / Chem)", 12, "Product Development, Formulation, Food Technology, Sensory Evaluation, Regulatory Affairs", "Leads new product development, reformulation, and pilot-plant trials at {Company}", "MFG-9", "Own innovation pipeline; lead formulation and pilot trials; manage regulatory submissions; coach food technologists; partner with marketing on launch", "% Revenue from New Products, Pipeline Vitality Index, Launch On-Time Rate, R&D Cost per Successful SKU, Patent / Disclosure Count", 30),
        new("HEHS-001", "Head of EHS & Sustainability", 5, 42_000m, 70_000m, "EHS", "COO-001", true, "Bachelor's (OHS / Environmental)", 12, "OHS Management, ISO 45001 / 14001, EPA Compliance, Sustainability Reporting, Emergency Response", "Leads {Company}'s environment, health & safety and sustainability programmes across all sites", "MFG-9", "Own EHS management system; drive Zero-Harm culture; lead EPA and DOL compliance; produce sustainability and ESG reports; chair safety committee", "LTIFR, Total Recordable Injury Rate, EPA Compliance Rating, CO2e per Tonne Produced, Water Use per Tonne, Waste Recovery %", 30),
        new("HOC-001", "Head of Commercial / Sales Director", 5, 45_000m, 75_000m, "COMMERCIAL", "CEO-001", true, "Master's Degree (MBA advantage)", 12, "Sales Leadership, Trade Marketing, Distributor Management, Channel Strategy, Brand Management", "Leads {Company}'s national sales, trade marketing, and brand management across all routes-to-market", "MFG-10", "Own commercial P&L; manage distributor and key-account network; develop trade and consumer brand plans; chair commercial review; deliver volume and value targets", "Net Sales Growth, Volume Share, Numeric Distribution, Trade Margin, Brand Equity Score, NPS", 30),
        new("HOF-001", "Head of Finance / Plant Finance Director", 5, 42_000m, 70_000m, "FINANCE", "CEO-001", true, "Master's / ACCA / ICAG", 12, "Manufacturing Finance, Standard Costing, IFRS, GRA Tax, Treasury, Working Capital", "Leads {Company}'s finance function — reporting, plant accounting, costing, treasury, and tax", "MFG-9", "Own monthly close; deliver IFRS-compliant accounts and management reporting; chair business performance review; manage GRA tax matters and FX exposure", "Reporting Timeliness, Audit Findings, Standard-Cost Variance, Working Capital Days, GRA Compliance Rate", 30),
        new("HOHR-001", "Head of HR", 5, 42_000m, 68_000m, "HR", "CEO-001", true, "Master's / CIHRM", 12, "HR Strategy, Industrial Relations, CBA Negotiation, Total Rewards, Talent Management", "Leads {Company} people strategy, including the union / CBA relationship and shop-floor industrial relations", "MFG-9", "Own people strategy; lead CBA negotiations; manage talent acquisition and L&D; ensure labour-law compliance; champion the {Company} culture", "Staff Turnover, Engagement Score, CBA Negotiation Outcome, Time-to-Hire, Training Hours / FTE, Industrial Disputes", 30),

        // ─── Rank 4 — Senior Management / Engineers ──────────────────────────────
        new("PM-001", "Production Manager", 4, 22_000m, 38_000m, "MANUFACTURING", "HOP-001", true, "Engineering Bachelor's", 8, "Production Planning, Shift Coordination, KPI Management, Lean, Team Leadership", "Manages a production area or major line at {Company}, owning daily output, quality, and shop-floor performance", "MFG-7", "Plan and supervise daily production; chair shift handover; drive line OEE and yield; coach supervisors; ensure CBA compliance; investigate downtime", "Line OEE, First-Time Yield, Schedule Adherence, Shift Productivity, Downtime Hours, Safety Observations", 27),
        new("QM-001", "Quality Manager", 4, 22_000m, 36_000m, "QUALITY", "HOQ-001", true, "Bachelor's Degree (Food Sci / Chem)", 8, "Quality Systems, ISO 22000 / HACCP, Internal Audits, Supplier Quality, CAPA", "Manages day-to-day quality assurance, lab operations, and finished-goods release at a {Company} plant", "MFG-7", "Run plant quality lab; lead release of finished goods; manage internal audits and CAPA; investigate complaints; supervise QA inspectors and analysts", "Right-First-Time %, Customer Complaints, Audit Findings Closure, COA Turnaround, Supplier Quality Index", 27),
        new("MTM-001", "Maintenance Manager", 4, 22_000m, 38_000m, "MAINTENANCE", "HOM-001", true, "Engineering Bachelor's", 8, "Preventive Maintenance, CMMS, Reliability, Spares Management, Mechanical / Electrical Systems", "Manages mechanical and electrical maintenance teams at a {Company} plant", "MFG-7", "Schedule and execute PM plan; lead breakdown response; manage spares stock; supervise technicians; produce reliability reports; drive Root-Cause Analysis", "MTBF, MTTR, PM Compliance, Maintenance Cost, Breakdown Hours, Spares Inventory Accuracy", 27),
        new("PRM-001", "Procurement Manager", 4, 22_000m, 38_000m, "SUPPLY-CHAIN", "HOSC-001", true, "Bachelor's / CIPS", 7, "Strategic Sourcing, Contract Management, Negotiation, Supplier Development, Category Strategy", "Manages strategic sourcing and supplier relationships for {Company}'s direct and indirect spend categories", "MFG-7", "Own assigned spend categories; run RFQs and tenders; negotiate framework contracts; manage supplier scorecards; deliver year-on-year savings", "Procurement Savings %, Contracted Spend %, Supplier OTIF, Supplier Quality Index, Tender Cycle Time", 27),
        new("PLM-001", "Planning Manager", 4, 22_000m, 36_000m, "SUPPLY-CHAIN", "HOSC-001", true, "Bachelor's Degree (CSCP advantage)", 7, "S&OP, Master Production Schedule, Demand Planning, MRP, ERP", "Leads demand and production planning; runs the monthly S&OP cycle for {Company}", "MFG-7", "Own master production schedule; chair S&OP; manage demand forecast; balance plant capacity vs demand; report shortfalls to ExCo", "Forecast Accuracy, MPS Adherence, Stock-Out Rate, S&OP Cycle Compliance, Plan vs Actual Variance", 27),
        new("PRE-001", "Senior Process Engineer", 4, 21_000m, 35_000m, "ENGINEERING", "HOE-001", false, "Engineering Bachelor's", 7, "Process Design, Six Sigma Black Belt, Statistical Process Control, P&IDs, Automation", "Senior engineer leading process improvement and capex projects at {Company}", "MFG-7", "Lead process improvement projects; design new lines and modifications; mentor process engineers; deliver Six Sigma savings; specify automation upgrades", "Process Yield Uplift, Project Savings GHS, Capex Delivered, OEE Uplift, Defect Rate Reduction", 27),
        new("EHSO-001", "EHS Manager", 4, 21_000m, 34_000m, "EHS", "HEHS-001", true, "Bachelor's (OHS / Environmental)", 7, "OHS Management, ISO 45001, Risk Assessment, Incident Investigation, EPA Reporting", "Manages site-level EHS programmes — risk assessments, incident investigation, EPA permits, and contractor safety", "MFG-7", "Run safety walkdowns; investigate incidents; close out EPA and DOL findings; manage contractor safety; train supervisors and operators on OHS", "LTIFR, Near-Miss Reporting Rate, Audit Findings Closed on Time, EPA Permit Compliance, Safety Training Coverage", 27),
        new("B2BSM-001", "B2B Sales Manager", 4, 22_000m, 38_000m, "COMMERCIAL", "HOC-001", true, "Bachelor's / MBA advantage", 7, "B2B Sales, Key Account Management, Industrial Channel, Negotiation, Tender Management", "Manages industrial / institutional / key-account sales for {Company} — large customers, hotels, processors", "MFG-7", "Manage assigned key-account portfolio; negotiate annual contracts; resolve service issues; cross-sell {Company} portfolio; drive volume and margin", "Key Account Revenue, Contract Renewal Rate, Volume Growth, Trade Margin, NPS", 27),
        new("TSM-001", "Trade / Distributor Sales Manager", 4, 22_000m, 36_000m, "COMMERCIAL", "HOC-001", true, "Bachelor's Degree", 7, "Trade Sales, Distributor Management, Route-to-Market, Field Coaching", "Manages {Company}'s distributor network and trade sales force across assigned regions", "MFG-7", "Set distributor targets; coach field sales reps; manage distributor stock and credit; run trade promotions; ensure numeric and weighted distribution growth", "Numeric Distribution, Distributor Volume vs Target, Cash Collection, Stock Cover Days, Promo ROI", 27),
        new("TMM-001", "Trade Marketing Manager", 4, 21_000m, 36_000m, "COMMERCIAL", "HOC-001", true, "Bachelor's Degree", 6, "Trade Marketing, BTL Activation, Merchandising, POS Material, Promo Management", "Designs and executes trade marketing programmes for {Company} brands across modern and general trade", "MFG-7", "Plan trade promotions; manage POS materials; brief field activation teams; track in-store visibility; measure promo ROI; partner with brand managers", "In-Store Visibility Score, Promo ROI, Trade Spend Efficiency, Activation Coverage, Sell-Out Lift", 27),
        new("BM2-001", "Brand Manager", 4, 21_000m, 36_000m, "COMMERCIAL", "HOC-001", true, "Bachelor's Degree", 6, "Brand Management, Consumer Insights, ATL / BTL Campaigns, Innovation, Media Planning", "Leads brand strategy and ATL marketing for one or more {Company} brand portfolios", "MFG-7", "Own brand P&L; develop annual brand plan; brief agencies on creative and media; track brand health; lead innovation projects with R&D and commercial", "Volume Share, Value Share, Brand Equity Score, Innovation Vitality, Marketing ROI", 27),
        new("PA-001", "Plant Accountant / Cost Accountant", 4, 21_000m, 34_000m, "FINANCE", "HOF-001", true, "Bachelor's / Part-ACCA / ICAG", 6, "Standard Costing, IFRS, Variance Analysis, Inventory Accounting, ERP (SAP / Dynamics)", "Owns plant accounting, standard costing, and inventory valuation at a {Company} plant", "MFG-7", "Run monthly plant close; analyse production variances; value WIP and finished goods; reconcile inventory; partner with plant manager on cost initiatives", "Standard Cost Variance, Inventory Accuracy %, Close Cycle Time, Audit Findings, Cost Savings Tracked", 27),
        new("HRM-001", "HR Manager", 4, 21_000m, 36_000m, "HR", "HOHR-001", true, "Bachelor's / CIHRM", 6, "HR Operations, Labour Law, Performance Management, HRIS, Recruitment", "Manages {Company} HR operations at site level — recruitment, payroll inputs, performance, and grievances", "MFG-7", "Run recruitment for staff and shop-floor roles; coordinate performance reviews; partner with line on grievances; manage HRIS data; ensure DOL compliance", "Time-to-Hire, Vacancy Fill Rate, HRIS Data Accuracy, Grievance Resolution Time, Turnover %", 27),
        new("IRO-001", "Industrial Relations Officer / Union Liaison", 4, 20_000m, 34_000m, "HR", "HOHR-001", true, "Bachelor's (HR / Law)", 7, "CBA Negotiation, Labour Law, Grievance Handling, Disciplinary Processes, Conciliation", "Manages {Company}'s relationship with the local union and shop-floor representatives — CBA, grievances, and disputes", "MFG-7", "Lead day-to-day CBA management; handle grievances; chair disciplinary panels; engage NLC and Department of Labour; train supervisors on labour law", "Days Lost to Disputes, Grievance Closure Time, CBA Compliance, Disciplinary Outcome Quality, NLC Cases Won", 27),

        // ─── Rank 3 — Engineers / Specialists / Supervisors ──────────────────────
        new("SS-001", "Shift Supervisor", 3, 9_500m, 16_000m, "MANUFACTURING", "PM-001", true, "Diploma / HND (Engineering)", 4, "Shift Management, SOP Compliance, People Management, KPI Reporting, Safety Leadership", "Runs a 12-hour production shift at {Company}, supervising line supervisors and operators", "OPS-6", "Lead shift handover; allocate operators to lines; maintain SOP compliance; close out shift report; investigate downtime; enforce PPE and safety rules", "Shift Output vs Target, Downtime Hours, Safety Observations, Shift Yield, Handover Quality", 24),
        new("LS-001", "Line Supervisor", 3, 9_000m, 15_000m, "MANUFACTURING", "SS-001", true, "Diploma / HND", 3, "Line Operations, SOP Compliance, Operator Coaching, Changeovers, Daily KPIs", "Supervises operators on a single production line at {Company}, owning hourly KPIs and changeovers", "OPS-5", "Run line daily; manage changeovers and CIP; coach operators on SOPs; raise breakdown calls; record hourly output and waste; maintain 5S", "Line OEE, Hourly Output Adherence, Changeover Time, Waste %, 5S Audit Score", 24),
        new("PE-001", "Process Engineer", 3, 12_000m, 20_000m, "ENGINEERING", "PRE-001", false, "Engineering Bachelor's", 3, "Process Engineering, Six Sigma Green Belt, SPC, Root-Cause Analysis, P&IDs", "Designs and improves manufacturing processes at {Company} plants — yield, quality, and throughput", "MFG-6", "Lead small CI projects; build and analyse SPC charts; investigate process losses; design line modifications; train operators on SOP changes", "Project Savings GHS, OEE Uplift, Defect Rate Reduction, SPC Capability Index, Project Delivery On-Time", 24),
        new("ME-001", "Manufacturing Engineer", 3, 11_000m, 19_000m, "ENGINEERING", "PRE-001", false, "Engineering Bachelor's", 3, "Manufacturing Engineering, Tooling, Equipment Specs, Capex Support, AutoCAD", "Provides engineering support for {Company} production lines — tooling, change parts, equipment specs, and small capex", "MFG-6", "Specify tooling and change parts; support capex commissioning; investigate equipment issues; produce P&IDs and layouts; manage minor projects", "Equipment Uptime, Capex On-Time, Engineering Change Closure Rate, Tooling Availability, Drawing Accuracy", 24),
        new("AE-001", "Automation / Controls Engineer", 3, 12_000m, 21_000m, "ENGINEERING", "PRE-001", false, "Engineering Bachelor's", 3, "PLC Programming, SCADA, Industrial Networks, Robotics, MES Integration", "Maintains and develops PLC, SCADA, and MES systems at {Company} plants", "MFG-6", "Programme and tune PLC / SCADA systems; integrate MES; support automation projects; troubleshoot control faults; document logic changes", "PLC / SCADA Uptime, Automation Project Delivery, Mean Time To Recover, Documentation Compliance", 24),
        new("QE-001", "Quality Engineer", 3, 11_000m, 19_000m, "QUALITY", "QM-001", false, "Bachelor's (Food Sci / Chem)", 3, "Quality Engineering, FMEA, SPC, CAPA, Internal Audits, ISO Standards", "Drives quality engineering, CAPA, and continuous improvement at {Company} plants", "MFG-6", "Lead CAPA investigations; build FMEAs; run internal audits; analyse customer complaints; coach line teams on quality tools", "CAPA Closure Time, Audit Findings Closed, Customer Complaint Reduction, FMEA Coverage, Right-First-Time %", 24),
        new("QA-001", "QA Analyst / Lab Analyst", 3, 7_500m, 13_000m, "QUALITY", "QM-001", false, "HND / Bachelor's (Lab Tech)", 2, "Lab Testing, GMP, GLP, HPLC / GC, Microbiological Testing, Documentation", "Performs in-process, raw material, and finished-goods analytical testing at {Company} labs", "MFG-5", "Conduct chemical and microbiological tests; record results in LIMS; calibrate lab instruments; raise non-conformance reports; support batch release", "Test Turnaround Time, Result Accuracy, LIMS Data Integrity, Calibration Compliance, NCR Rate", 24),
        new("QI-001", "QC Inspector", 3, 6_500m, 11_500m, "QUALITY", "QM-001", false, "Diploma / HND", 2, "In-Process Inspection, Defect Identification, Sampling Plans, GMP, Documentation", "Performs in-process and finished-goods inspection on {Company} production lines", "OPS-5", "Sample and inspect products against specs; identify defects; raise hold tickets; complete inspection records; coach operators on quality standards", "Inspection Coverage, Defects Caught vs Released, Hold-Ticket Closure Time, Documentation Quality", 24),
        new("LT-001", "Lab Technician", 3, 6_500m, 11_500m, "QUALITY", "QM-001", false, "HND / Diploma", 2, "Wet Chemistry, Microbiology Bench Work, Equipment Calibration, GMP Documentation", "Supports {Company} lab operations — sample prep, routine testing, and instrument upkeep", "OPS-5", "Prepare samples; run routine titrations and assays; clean and calibrate instruments; support QA analysts on complex runs; maintain lab logs", "Sample Throughput, Instrument Calibration Status, Turnaround Time, Lab Housekeeping Score", 24),
        new("ME2-001", "Maintenance Engineer", 3, 11_000m, 19_000m, "MAINTENANCE", "MTM-001", false, "Engineering Bachelor's", 3, "Reliability Engineering, RCM, CMMS, Mechanical / Electrical Systems, Vibration Analysis", "Provides engineering support to {Company} maintenance — reliability studies, root-cause analysis, and PM optimisation", "MFG-6", "Lead Root-Cause Analyses; optimise PM tasks; manage condition-monitoring programme; specify spares and rebuilds; mentor technicians", "MTBF, RCA Closure Quality, PM Optimisation Savings, Condition-Monitoring Coverage, Critical Spares Availability", 24),
        new("IT2-001", "Instrument / Calibration Technician", 3, 7_000m, 12_500m, "MAINTENANCE", "MTM-001", false, "HND (Instrumentation)", 3, "Instrumentation, Calibration, Process Control, Loop Tuning, Metrology", "Maintains and calibrates instrumentation across {Company} plants — flow, pressure, temperature, level", "OPS-5", "Calibrate process instruments; tune control loops; troubleshoot instrument faults; maintain calibration records; support SAT / FAT for new instruments", "Calibration Compliance, Instrument Uptime, Loop Tuning Quality, Calibration Records Audit Score", 24),
        new("PRO-001", "Procurement Officer", 3, 8_500m, 14_500m, "SUPPLY-CHAIN", "PRM-001", false, "Bachelor's / Part-CIPS", 2, "Procurement, RFQ, Purchase Orders, Supplier Management, ERP", "Executes purchase orders and supplier follow-up for {Company}'s direct and indirect spend", "MFG-5", "Raise and process POs; expedite deliveries; resolve invoice queries; maintain supplier records; support category managers on tenders", "PO Cycle Time, Supplier OTIF, Invoice Match Rate, ERP Data Accuracy, Savings Captured", 24),
        new("BUY-001", "Buyer", 3, 8_500m, 14_500m, "SUPPLY-CHAIN", "PRM-001", false, "Bachelor's Degree", 2, "Tactical Buying, Negotiation, Vendor Quotation, ERP, Spend Analytics", "Manages day-to-day buying of materials and services for {Company}", "MFG-5", "Source quotes; place tactical orders; negotiate spot deals; maintain vendor pricing; flag savings opportunities to category managers", "PO Throughput, Average Saving per PO, Vendor Compliance, On-Time Delivery, Quote Cycle Time", 24),
        new("DP-001", "Demand Planner", 3, 9_000m, 15_500m, "SUPPLY-CHAIN", "PLM-001", false, "Bachelor's Degree (CSCP advantage)", 3, "Demand Forecasting, Statistical Models, S&OP, Sales Collaboration, Power BI", "Owns demand forecast and sales-input collection for {Company}'s S&OP", "MFG-6", "Build statistical forecasts; collaborate with sales on consensus demand; track forecast bias and accuracy; flag risks at S&OP; produce demand reviews", "Forecast Accuracy %, Forecast Bias, Demand Review Quality, S&OP Inputs On-Time, Stock-Out Rate", 24),
        new("PP-001", "Production Planner", 3, 9_000m, 15_500m, "SUPPLY-CHAIN", "PLM-001", false, "Bachelor's / HND", 3, "Production Planning, MRP, Capacity Planning, ERP, Scheduling", "Builds and maintains the master production schedule for {Company} plants", "MFG-6", "Run MRP; build line-level production plan; sequence campaigns; manage materials availability with procurement; track schedule adherence", "MPS Adherence, Material Availability, Plan Stability, Capacity Utilisation, Schedule Change Rate", 24),
        new("WS-001", "Warehouse Supervisor", 3, 8_500m, 14_500m, "SUPPLY-CHAIN", "HOSC-001", true, "HND / Bachelor's", 3, "Warehouse Operations, WMS, Stock Accuracy, Team Leadership, FEFO / FIFO", "Supervises stores and warehouse operations at a {Company} plant or depot", "OPS-6", "Manage receipts, put-away, picking, and dispatch; maintain stock accuracy; lead cycle counts; supervise stores officers and forklift operators; ensure 5S", "Stock Accuracy %, Cycle Count Variance, Put-Away Cycle Time, Picking Accuracy, OTIF Dispatch", 24),
        new("SO-001", "Stores Officer", 3, 6_500m, 11_500m, "SUPPLY-CHAIN", "WS-001", false, "Diploma / HND", 2, "Stock Management, ERP / WMS, Goods Receipt, FEFO, Documentation", "Receives, stores, and issues raw materials, packaging, and spares at {Company} stores", "OPS-5", "Receive and inspect deliveries; book goods into ERP; issue materials to production; maintain FEFO; support cycle counts; manage damaged-stock register", "Receipt Accuracy, Issue Turnaround, Stock Variance, FEFO Compliance, Damaged Stock %", 24),
        new("LC-001", "Logistics Coordinator", 3, 8_500m, 14_500m, "SUPPLY-CHAIN", "HOSC-001", false, "Bachelor's / HND", 3, "Transport Planning, 3PL Management, Customs / GRA Clearance, Documentation", "Coordinates outbound logistics from {Company} plants and depots to customers and distributors", "MFG-5", "Plan daily dispatches; manage 3PL fleet; track in-transit consignments; resolve delivery exceptions; manage waybills and POD; reconcile freight invoices", "OTIF %, Freight Cost / Case, POD Compliance, Delivery Exceptions, 3PL SLA Adherence", 24),
        new("IBC-001", "Inbound Logistics Coordinator", 3, 8_000m, 14_000m, "SUPPLY-CHAIN", "HOSC-001", false, "Bachelor's / HND", 2, "Inbound Logistics, Import Clearance, GRA / Customs, ICUMS, Freight Forwarding", "Coordinates inbound shipments of raw and packaging materials to {Company} plants", "MFG-5", "Track inbound shipments; manage clearance via ICUMS; coordinate with freight forwarders; book inbound deliveries to plant; resolve clearance issues", "On-Time Clearance, Demurrage Cost, ICUMS Accuracy, Inbound OTIF, Clearance Cycle Time", 24),
        new("OBC-001", "Outbound Logistics Coordinator", 3, 8_000m, 14_000m, "SUPPLY-CHAIN", "HOSC-001", false, "Bachelor's / HND", 2, "Outbound Logistics, 3PL Management, Route Planning, Distributor Service", "Coordinates outbound dispatches from {Company} warehouses to distributors and key accounts", "MFG-5", "Plan daily routing; load trucks; manage seal numbers and waybills; track delivery and POD; resolve customer service exceptions", "OTIF %, Truck Fill %, Seal Compliance, POD Capture, Distributor SLA Adherence", 24),
        new("FT-001", "Food Technologist / Formulation Scientist", 3, 11_000m, 19_000m, "R&D", "HORD-001", false, "Bachelor's (Food Sci / Chem)", 3, "Food Technology, Formulation, Sensory Evaluation, Pilot Plant, Regulatory", "Develops new product formulations and reformulations for {Company} brands", "MFG-6", "Build prototype formulations; run pilot trials; lead sensory panels; document regulatory dossiers; partner with marketing on launches", "Pilot Trial Success Rate, Launch On-Time, Reformulation Savings, Sensory Panel Quality, Regulatory Submission Rate", 24),
        new("RDS-001", "R&D Scientist", 3, 12_000m, 20_000m, "R&D", "HORD-001", false, "Master's / PhD (Food Sci / Chem)", 4, "Research Methodology, Analytical Chemistry, Statistics, IP Management", "Leads applied research and complex projects in {Company}'s innovation pipeline", "MFG-6", "Design and run research studies; publish technical reports; support patent / disclosure filings; mentor junior scientists; collaborate with academia", "Publications / Disclosures, Project Milestone Adherence, Innovation Pipeline Vitality, Mentee Progress", 24),
        new("SI-001", "Safety Inspector", 3, 7_500m, 13_000m, "EHS", "EHSO-001", false, "HND / Bachelor's (OHS)", 3, "Safety Inspections, Risk Assessment, Permit-to-Work, Incident Investigation", "Conducts safety inspections and audits across {Company} plants and contractor sites", "OPS-5", "Run scheduled safety walks; audit permit-to-work compliance; investigate near-misses; track action closure; coach line teams on risk control", "Inspections Completed, Action Closure Rate, Near-Miss Reports, Permit Compliance, Coaching Sessions Delivered", 24),
        new("SUS-001", "Sustainability Officer", 3, 8_500m, 14_500m, "EHS", "HEHS-001", false, "Bachelor's (Environmental / Eng)", 3, "Sustainability Reporting, GHG Accounting, Water / Energy Management, EPA Liaison", "Supports {Company}'s sustainability roadmap — GHG, water, energy, and waste programmes", "MFG-5", "Build site-level GHG inventory; track water and energy use; lead waste-to-recovery initiatives; produce ESG report inputs; engage EPA and external assurance", "CO2e per Tonne, Water Use per Tonne, Waste Recovery %, ESG Report Quality, EPA Compliance Status", 24),
        new("AC-001", "Accountant / Plant Cost Accountant", 3, 9_000m, 15_500m, "FINANCE", "PA-001", false, "Bachelor's / Part-ACCA", 3, "General Ledger, Accruals, IFRS, Cost Accounting, ERP, Excel", "Supports plant accounting at {Company} — month-end close, accruals, and management reporting", "MFG-5", "Post month-end journals; produce variance commentary; support plant accountant on close; reconcile sub-ledgers; partner with operations on KPIs", "Close Cycle Time, Reconciliation Accuracy, Variance Commentary Quality, Audit Findings, Period Close Adherence", 24),
        new("CA2-001", "Cost Accountant", 3, 9_500m, 16_000m, "FINANCE", "PA-001", false, "Bachelor's / Part-ACCA / ICAG", 3, "Standard Costing, Variance Analysis, Bill-of-Material, ERP, Inventory Accounting", "Owns standard cost setting, variance analysis, and BOM accuracy at {Company}", "MFG-5", "Set annual standards; investigate purchase price, usage, and yield variances; maintain BOMs; partner with R&D on new SKUs; report monthly cost performance", "Standard Cost Variance, BOM Accuracy %, Inventory Adjustment Rate, Variance Investigation Closure", 24),
        new("HRO-001", "HR Officer", 3, 7_500m, 13_000m, "HR", "HRM-001", false, "Bachelor's Degree", 2, "HR Operations, Recruitment, HRIS, Employee Relations, Payroll Inputs", "Provides operational HR support to {Company} plants and head office", "MFG-5", "Run recruitment for assigned roles; onboard new joiners; maintain HRIS records; produce payroll inputs; handle first-line employee queries", "Time-to-Hire, Onboarding Quality, HRIS Accuracy, Payroll Input Timeliness, Query Closure Time", 24),
        new("TR-001", "Training & L&D Officer", 3, 7_500m, 13_000m, "HR", "HOHR-001", false, "Bachelor's Degree", 2, "Learning Design, Training Coordination, LMS, Operator Skill Matrix, Facilitation", "Coordinates {Company} training — operator skills, leadership, and compliance courses", "MFG-5", "Maintain operator skill matrices; coordinate training logistics; manage LMS; deliver basic facilitation; track training hours and certifications", "Training Hours / FTE, Skill-Matrix Coverage, Course Completion Rate, Trainer Feedback, LMS Data Accuracy", 24),
        new("CSO-001", "Customer Service Officer", 3, 6_500m, 11_500m, "COMMERCIAL", "B2BSM-001", false, "Bachelor's / HND", 2, "Customer Service, ERP Order Entry, Complaint Handling, Communication", "Manages customer order intake and service exceptions for {Company} key accounts", "MFG-5", "Take and enter customer orders; track order status; handle service complaints; coordinate with logistics on dispatches; produce weekly service reports", "Order Accuracy, Order Lead Time, Complaint Closure Time, Customer Satisfaction Score, OTIF (Customer View)", 24),
        new("IT-001", "IT Officer", 3, 8_500m, 14_500m, "IT", "EXEC", false, "Bachelor's / HND", 2, "IT Support, ERP (SAP / Dynamics), Active Directory, Networking, Service Desk", "Provides ERP, end-user, and infrastructure IT support across {Company} sites", "MFG-5", "Resolve service-desk tickets; support ERP users; manage AD accounts; coordinate with vendors on incidents; maintain hardware inventory", "Ticket Resolution Time, First-Call Resolution, ERP Issue Closure, User Satisfaction, Asset Register Accuracy", 24),

        // ─── Rank 2 — Operators / Technicians / Junior Staff (CBA shop floor) ────
        new("MO-001", "Machine Operator", 2, 3_500m, 6_500m, "MANUFACTURING", "LS-001", false, "WASSCE / Trade Certificate", 1, "Machine Operation, SOP Compliance, Basic Troubleshooting, 5S, Safety Awareness", "Operates production machinery on a {Company} line — set-ups, runs, and basic line maintenance", "CB-3", "Set up and run line equipment to SOP; perform line CIL; record hourly output; identify quality defects; perform autonomous maintenance; follow safety rules", "Line Output Adherence, SOP Compliance, Quality Defect Rate, CIL Completion, Safety Observations", 21),
        new("PKO-001", "Packaging Operator", 2, 3_000m, 5_500m, "MANUFACTURING", "LS-001", false, "WASSCE", 0, "Packaging Operations, Labelling, Carton Erecting, 5S, GMP", "Operates packaging machines and lines (filling, capping, labelling, case-packing) at {Company}", "CB-2", "Run packaging line; clear jams; perform changeovers; check carton and label quality; maintain line 5S; report defects", "Packaging Output, Changeover Time, Label / Carton Defect Rate, 5S Audit Score, Hourly Run Rate", 21),
        new("FLO-001", "Filling / Bottling Operator", 2, 3_200m, 5_800m, "MANUFACTURING", "LS-001", false, "WASSCE / Trade Certificate", 1, "Filling Operations, GMP, CIP, Hygienic Operation, Changeover", "Operates filling and bottling machinery on {Company} liquid lines", "CB-3", "Run filler and capper; perform CIP and changeovers; check fill volumes and torque; maintain hygiene standards; raise quality holds", "Fill Accuracy, Changeover Time, CIP Compliance, Reject Rate, Hourly Run Rate", 21),
        new("MXO-001", "Mixing / Process Operator", 2, 3_500m, 6_500m, "MANUFACTURING", "SS-001", false, "Trade Certificate / HND", 1, "Process Operation, Batch Control, GMP, BMS / SCADA, Hygienic Practice", "Operates mixers, kettles, and process vessels on {Company} formulation areas", "CB-3", "Weigh and dose ingredients; run batches per recipe; monitor process parameters; record batch sheets; perform CIP; raise out-of-spec deviations", "Batch Yield, Recipe Adherence, CIP Compliance, Batch Documentation Quality, Out-of-Spec Rate", 21),
        new("CO-001", "Cleaning / Hygiene Operator", 2, 2_500m, 4_500m, "MANUFACTURING", "LS-001", false, "WASSCE / Basic", 0, "Cleaning, GMP / GHP, Chemical Handling, 5S", "Performs scheduled cleaning and sanitation on {Company} production areas", "CB-2", "Clean lines and floors; sanitise contact surfaces; manage cleaning chemicals safely; maintain cleaning records; support 5S audits", "Cleaning Schedule Adherence, Hygiene Audit Score, Chemical Use Compliance, 5S Audit Score", 21),
        new("PI-001", "Packaging / Material Inspector", 2, 3_500m, 6_000m, "QUALITY", "QI-001", false, "Diploma / WASSCE", 1, "Visual Inspection, Sampling, Quality Standards, Documentation", "Inspects incoming packaging and finished goods on {Company} lines", "CB-3", "Sample and inspect to plan; raise hold tickets on defects; complete inspection logs; support QC inspectors; maintain inspection station 5S", "Inspection Coverage, Defects Caught, Documentation Quality, Hold-Ticket Closure", 21),
        new("MTE-001", "Maintenance Technician (Mechanical)", 2, 4_000m, 7_500m, "MAINTENANCE", "MTM-001", false, "Trade Certificate / HND", 2, "Mechanical Maintenance, Welding, Bearings, Pneumatics, Hydraulics", "Performs mechanical maintenance and breakdown repairs on {Company} production assets", "CB-4", "Run PM tasks; respond to breakdowns; replace bearings, seals, belts; weld and fabricate; support overhauls; record CMMS entries", "PM Compliance, Breakdown Response Time, Repeat Failures, CMMS Data Quality, First-Time Fix Rate", 21),
        new("MTM2-001", "Maintenance Technician (Electrical)", 2, 4_200m, 7_800m, "MAINTENANCE", "MTM-001", false, "Trade Certificate / HND (Electrical)", 2, "Electrical Maintenance, Motor Control, VFDs, PLC Hardware, LV Switchgear", "Performs electrical maintenance and breakdown repairs on {Company} production assets", "CB-4", "Run electrical PM; respond to electrical breakdowns; service motors, VFDs, switchgear; support PLC I/O fault-finding; record CMMS entries", "PM Compliance, Breakdown Response Time, Repeat Electrical Failures, CMMS Quality, Lock-Out / Tag-Out Compliance", 21),
        new("FLF-001", "Forklift Operator", 2, 3_000m, 5_500m, "SUPPLY-CHAIN", "WS-001", false, "Forklift Licence (Class B/C)", 1, "Forklift Operation, Pallet Handling, Warehouse Safety, Stock Movement", "Operates forklifts and material handling equipment in {Company} warehouses", "CB-3", "Move pallets between storage and lines; load and unload trucks; perform daily forklift checks; observe traffic management; maintain stock locations", "Pallets Moved per Hour, Damage Rate, Forklift Inspection Compliance, Traffic Incidents", 21),
        new("DR-001", "Driver", 2, 2_800m, 5_000m, "EXEC", "EXEC", false, "Driving Licence (Class C+)", 2, "Defensive Driving, Fleet Safety, Vehicle Inspection, Logbook Maintenance", "Drives company vehicles for {Company} executives, sales teams, and logistics support", "CB-2", "Drive assigned vehicle safely; perform daily vehicle checks; maintain logbook; collect / deliver staff and parcels; support 3PL where needed", "Safe Driving Hours, Vehicle Inspection Compliance, Logbook Accuracy, Punctuality, Maintenance Adherence", 21),
        new("SEC-001", "Security Officer", 2, 2_500m, 4_500m, "EXEC", "EXEC", false, "WASSCE", 1, "Site Security, Access Control, CCTV Monitoring, Incident Reporting", "Provides site security at {Company} plants and offices", "CB-2", "Control gate access; monitor CCTV; patrol perimeter; record incidents; manage visitor and contractor sign-in; support emergency response", "Incident Response Time, Access Control Compliance, Patrol Adherence, Visitor Log Accuracy", 21),
        new("CL-001", "Cleaner / Janitor", 2, 2_000m, 3_800m, "EXEC", "EXEC", false, "Basic Education", 0, "Office Cleaning, Sanitation, Chemical Safety", "Maintains cleanliness in {Company} offices and amenity areas", "CB-1", "Clean offices, restrooms, and amenity areas to schedule; manage cleaning supplies; report maintenance issues; follow chemical handling rules", "Cleaning Schedule Adherence, Cleanliness Audit Score, Supply Use Efficiency", 21),
        new("EA-001", "Executive Assistant", 2, 4_500m, 8_000m, "EXEC", "CEO-001", false, "Bachelor's / HND", 3, "Executive Coordination, Diary Management, Confidentiality, MS Office, Communication", "Provides executive assistance to {Company}'s MD and ExCo members", "MFG-4", "Manage executive diaries; coordinate travel and meetings; prepare briefings and minutes; handle confidential correspondence; liaise with the board secretariat", "Schedule Efficiency, Briefing Quality, Stakeholder Satisfaction, Confidentiality Compliance", 21),

        // ─── Rank 1 — Trainees / Apprentices / Entry ─────────────────────────────
        new("PT-001", "Production Trainee / Operator Trainee", 1, 1_800m, 3_200m, "MANUFACTURING", "LS-001", false, "WASSCE", 0, "Learning Agility, Attention to Detail, Safety Awareness, Teamwork", "Entry-level shop-floor trainee learning machine operation and {Company} SOPs", "CB-1", "Shadow operators on assigned line; complete operator-skill matrix sign-offs; learn SOPs and safety rules; support 5S; rotate across stations", "Skill-Matrix Sign-Off Rate, Attendance, Training Course Completion, Supervisor Assessment", 21),
        new("APP-001", "Maintenance Apprentice", 1, 1_800m, 3_200m, "MAINTENANCE", "MTM-001", false, "Technical / Trade School", 0, "Mechanical / Electrical Basics, Tool Use, Workshop Safety, Learning Agility", "Apprentice on a structured technical programme at {Company} maintenance", "CB-1", "Rotate through mechanical, electrical, and instrumentation workshops; complete competency log; pass NVTI / trade tests; support technicians", "Competency Log Progress, NVTI / Trade Test Results, Supervisor Assessment, Attendance", 21),
        new("INT-001", "Industrial Attaché / Intern", 1, 1_500m, 2_500m, "EXEC", "HRM-001", false, "Current University / HND Student", 0, "Eagerness to Learn, MS Office, Communication, Numeracy, Professionalism", "Short-term placement for university and HND students gaining practical {Company} manufacturing experience", "MFG-1", "Support assigned team with project tasks; shadow senior staff; complete attachment report; participate in induction; deliver end-of-placement presentation", "Supervisor Assessment, Task Completion Rate, Attendance, Report Quality", 21),
        new("GT-001", "Graduate Trainee", 1, 2_500m, 4_000m, "EXEC", "HOHR-001", false, "Bachelor's Degree (minimum 2:1)", 0, "Analytical Thinking, Communication, Teamwork, MS Office, Learning Agility", "Entry-level rotational programme rotating across {Company} functions over 18-24 months", "MFG-2", "Rotate through assigned departments; complete learning milestones; deliver project work; participate in mentorship; present to ExCo at end of programme", "Rotation Assessment Scores, Project Delivery, Mentor Feedback, Attendance, Final Presentation Quality", 21)
    ];

    // Curated 45-station catalogue for a tier-1 Ghana manufacturer (FMCG / beverage /
    // heavy-industrial scale). HQ in Accra; plants in Tema, Kumasi, Takoradi, Tamale,
    // Sekondi, Cape Coast (real industrial estates); regional distribution centres /
    // warehouses; sales depots and trade offices. Phone numbers and addresses are
    // representative; emails store ONLY the local part (before @) so the row factory
    // appends the actual company TLD at runtime. {Company} placeholder is substituted
    // with the actual tenant name at row-emit time.
    private static readonly StationSpec[] _manufacturingStations =
    [
        // ─── Head Office ─────────────────────────────────────────────────────────
        new("HO-001",   "Head Office - Accra",                       "Head Office",         "Greater Accra", "Accra",      "Ring Road Industrial Area, Accra", 80, 350, "{Company} corporate headquarters housing executive, commercial, finance, HR and IT functions", "+233 30 320 1100", "headoffice"),

        // ─── Plants (6) ──────────────────────────────────────────────────────────
        new("PL-TM-001", "Tema Main Plant",                          "Plant",               "Greater Accra", "Tema",       "Plot 16, Tema Industrial Area Site 4, Tema",   200, 800, "{Company} flagship manufacturing plant — main production lines, R&D pilot plant, and central warehousing", "+233 30 320 2100", "tema.plant"),
        new("PL-TM-002", "Tema Heavy Industrial Plant",              "Plant",               "Greater Accra", "Tema",       "Heavy Industrial Area, Tema",                  150, 500, "{Company} secondary plant covering heavy industrial operations and packaging lines",                       "+233 30 320 2200", "tema.heavy"),
        new("PL-KS-001", "Kumasi Plant",                             "Plant",               "Ashanti",       "Kumasi",     "Kumasi Suame Magazine Industrial Area",        120, 400, "{Company} Kumasi plant serving Ashanti, Bono, and Northern Ghana with regional production capacity",        "+233 32 220 2100", "kumasi.plant"),
        new("PL-TK-001", "Takoradi Plant",                           "Plant",               "Western",       "Takoradi",   "Takoradi Inchaban Industrial Estate, Takoradi", 90, 300, "{Company} Western Region plant covering oil-and-gas-belt customers and Western Region distribution",        "+233 31 220 2100", "takoradi.plant"),
        new("PL-TM-003", "Tamale Plant",                             "Plant",               "Northern",      "Tamale",     "Industrial Area, Tamale",                      80, 250, "{Company} Tamale plant serving Northern, North-East, Savannah, Upper East, and Upper West regions",         "+233 37 220 2100", "tamale.plant"),
        new("PL-SE-001", "Sekondi Plant",                            "Plant",               "Western",       "Sekondi",    "Sekondi Industrial Park, Sekondi",             70, 220, "{Company} Sekondi plant — secondary Western Region production and packaging facility",                      "+233 31 220 2200", "sekondi.plant"),

        // ─── Distribution Centres (4) ────────────────────────────────────────────
        new("DC-AC-001", "Accra Distribution Centre",                "Distribution Centre", "Greater Accra", "Accra",      "Spintex Road, Accra",                          40, 120, "{Company} primary national distribution centre serving Greater Accra and southern depots",                  "+233 30 320 3100", "accra.dc"),
        new("DC-KS-001", "Kumasi Distribution Centre",               "Distribution Centre", "Ashanti",       "Kumasi",     "Kaase Industrial Area, Kumasi",                30,  90, "{Company} regional distribution centre serving Ashanti, Bono, Bono East, and Ahafo regions",                "+233 32 220 3100", "kumasi.dc"),
        new("DC-TK-001", "Takoradi Distribution Centre",             "Distribution Centre", "Western",       "Takoradi",   "Anaji Industrial Area, Takoradi",              25,  70, "{Company} regional distribution centre serving Western and Western North regions",                          "+233 31 220 3100", "takoradi.dc"),
        new("DC-TM-001", "Tamale Distribution Centre",               "Distribution Centre", "Northern",      "Tamale",     "Education Ridge, Tamale",                      20,  60, "{Company} regional distribution centre serving the five northern regions",                                   "+233 37 220 3100", "tamale.dc"),

        // ─── Warehouses (4) ──────────────────────────────────────────────────────
        new("WH-TM-001", "Tema Bonded Warehouse",                    "Warehouse",           "Greater Accra", "Tema",       "Tema Port Free Zone Enclave, Tema",            25,  70, "{Company} bonded warehouse for imported raw materials, packaging, and free-zone clearance",                  "+233 30 320 3300", "tema.bonded"),
        new("WH-CC-001", "Cape Coast Regional Warehouse",            "Warehouse",           "Central",       "Cape Coast", "Cape Coast Trade Park, Cape Coast",            15,  50, "{Company} regional satellite warehouse serving Central Region and Western Region overflow",                 "+233 33 220 3100", "capecoast.wh"),
        new("WH-HO-001", "Ho Regional Warehouse",                    "Warehouse",           "Volta",         "Ho",         "Ho-Aflao Road, Ho",                            12,  40, "{Company} regional satellite warehouse serving Volta and Oti regions",                                       "+233 36 220 3100", "ho.wh"),
        new("WH-SU-001", "Sunyani Regional Warehouse",               "Warehouse",           "Bono",          "Sunyani",    "Fiapre Industrial Layout, Sunyani",            12,  40, "{Company} regional satellite warehouse serving Bono and Bono East regions",                                  "+233 35 220 3100", "sunyani.wh"),

        // ─── Sales Depots (8) ────────────────────────────────────────────────────
        new("SD-AC-001", "Accra Central Sales Depot",                "Sales Depot",         "Greater Accra", "Accra",      "Adabraka Industrial Area, Accra",              10,  35, "{Company} sales depot for distributor pick-ups in central Accra and surrounding markets",                    "+233 30 320 4101", "accra.depot"),
        new("SD-KS-001", "Kumasi Adum Sales Depot",                  "Sales Depot",         "Ashanti",       "Kumasi",     "Adum Commercial Area, Kumasi",                 10,  35, "{Company} sales depot serving distributors and trade in central Kumasi",                                     "+233 32 220 4101", "kumasi.depot"),
        new("SD-TK-001", "Takoradi Sales Depot",                     "Sales Depot",         "Western",       "Takoradi",   "Market Circle, Takoradi",                       8,  30, "{Company} sales depot for Western Region distributors and trade",                                            "+233 31 220 4101", "takoradi.depot"),
        new("SD-TM-001", "Tamale Sales Depot",                       "Sales Depot",         "Northern",      "Tamale",     "Aboabo Market, Tamale",                         8,  30, "{Company} sales depot serving Northern Region distributors and traders",                                     "+233 37 220 4101", "tamale.depot"),
        new("SD-CC-001", "Cape Coast Sales Depot",                   "Sales Depot",         "Central",       "Cape Coast", "Kotokuraba Market Road, Cape Coast",            8,  25, "{Company} sales depot for Central Region distributors and high-trade outlets",                               "+233 33 220 4101", "capecoast.depot"),
        new("SD-HO-001", "Ho Sales Depot",                           "Sales Depot",         "Volta",         "Ho",         "Ho Central Market, Ho",                         6,  20, "{Company} sales depot for Volta Region distributors and the Ho-Hohoe corridor",                              "+233 36 220 4101", "ho.depot"),
        new("SD-KF-001", "Koforidua Sales Depot",                    "Sales Depot",         "Eastern",       "Koforidua",  "Hospital Road, Koforidua",                      6,  20, "{Company} sales depot serving Eastern Region distributors and the Accra-Kumasi highway corridor",            "+233 34 220 4101", "koforidua.depot"),
        new("SD-SU-001", "Sunyani Sales Depot",                      "Sales Depot",         "Bono",          "Sunyani",    "Sunyani Central Market, Sunyani",               6,  20, "{Company} sales depot serving Bono Region distributors and surrounding districts",                            "+233 35 220 4101", "sunyani.depot"),

        // ─── Trade Offices / Satellites (15) ─────────────────────────────────────
        new("TO-AC-001", "Accra Airport City Trade Office",          "Trade Office",        "Greater Accra", "Accra",      "Airport City, Accra",                           4,  12, "{Company} trade office covering modern-trade key accounts at Airport City and East Legon",                   "+233 30 320 5101", "airportcity.trade"),
        new("TO-AC-002", "Madina Trade Office",                      "Trade Office",        "Greater Accra", "Accra",      "Madina Market, Accra",                          3,  10, "{Company} trade office covering Madina, Adenta, and surrounding general-trade markets",                      "+233 30 320 5102", "madina.trade"),
        new("TO-AC-003", "Kasoa Trade Office",                       "Trade Office",        "Central",       "Kasoa",      "Kasoa Old Barrier, Kasoa",                      3,  10, "{Company} trade office for the fast-growing Kasoa-Awutu Senya commercial corridor",                          "+233 33 220 5101", "kasoa.trade"),
        new("TO-AC-004", "Spintex Trade Office",                     "Trade Office",        "Greater Accra", "Accra",      "Spintex Road, Accra",                           3,  10, "{Company} trade office serving Spintex modern-trade outlets and industrial corridor traders",                "+233 30 320 5103", "spintex.trade"),
        new("TO-AC-005", "Tema Community 1 Trade Office",            "Trade Office",        "Greater Accra", "Tema",       "Community 1, Tema",                             3,  10, "{Company} trade office for Tema general-trade and industrial customers",                                     "+233 30 320 5104", "temac1.trade"),
        new("TO-AS-001", "Kumasi Asafo Trade Office",                "Trade Office",        "Ashanti",       "Kumasi",     "Asafo Market, Kumasi",                          3,  10, "{Company} trade office covering Asafo and surrounding Kumasi general-trade markets",                          "+233 32 220 5101", "asafo.trade"),
        new("TO-AS-002", "Obuasi Trade Office",                      "Trade Office",        "Ashanti",       "Obuasi",     "Obuasi Central Market, Obuasi",                 2,   8, "{Company} trade office for Obuasi mining community and surrounding markets",                                  "+233 32 220 5102", "obuasi.trade"),
        new("TO-AS-003", "Ejisu Trade Office",                       "Trade Office",        "Ashanti",       "Ejisu",      "Ejisu Market, Ejisu",                           2,   8, "{Company} trade office for Ejisu, Juaben, and surrounding rural distributors",                               "+233 32 220 5103", "ejisu.trade"),
        new("TO-WE-001", "Tarkwa Trade Office",                      "Trade Office",        "Western",       "Tarkwa",     "Tarkwa Market Road, Tarkwa",                    2,   8, "{Company} trade office for Tarkwa mining and Western Region commercial outlets",                              "+233 31 220 5101", "tarkwa.trade"),
        new("TO-EA-001", "Nkawkaw Trade Office",                     "Trade Office",        "Eastern",       "Nkawkaw",    "Accra-Kumasi Highway, Nkawkaw",                 2,   8, "{Company} trade office on the Nkawkaw highway servicing trade along the Accra-Kumasi corridor",              "+233 34 220 5101", "nkawkaw.trade"),
        new("TO-EA-002", "Akim Oda Trade Office",                    "Trade Office",        "Eastern",       "Akim Oda",   "Akim Oda Market, Akim Oda",                     2,   8, "{Company} trade office for Akim Oda and surrounding Birim Central rural markets",                            "+233 34 220 5102", "akimoda.trade"),
        new("TO-VO-001", "Hohoe Trade Office",                       "Trade Office",        "Volta",         "Hohoe",      "Hohoe Market, Hohoe",                           2,   8, "{Company} trade office for Hohoe and the Volta Region eastern corridor",                                     "+233 36 220 5101", "hohoe.trade"),
        new("TO-BO-001", "Techiman Trade Office",                    "Trade Office",        "Bono East",     "Techiman",   "Techiman Market, Techiman",                     2,   8, "{Company} trade office covering Techiman's major regional market and Bono East distributors",                "+233 35 220 5101", "techiman.trade"),
        new("TO-NO-001", "Bolgatanga Trade Office",                  "Trade Office",        "Upper East",    "Bolgatanga", "Bolgatanga Central Market, Bolgatanga",         2,   8, "{Company} trade office for Upper East Region distributors and cross-border trade with Burkina Faso",         "+233 37 220 5101", "bolgatanga.trade"),
        new("TO-NO-002", "Wa Trade Office",                          "Trade Office",        "Upper West",    "Wa",         "Wa Central Market, Wa",                         2,   8, "{Company} trade office for Upper West Region distributors and rural community trade",                         "+233 39 220 5101", "wa.trade"),
        new("TO-AC-006", "Ashaiman Trade Office",                    "Trade Office",        "Greater Accra", "Ashaiman",   "Ashaiman Market, Ashaiman",                     2,   8, "{Company} trade office serving Ashaiman's dense general-trade and informal-market traders",                   "+233 30 320 5105", "ashaiman.trade"),
        new("TO-AC-007", "Lapaz Trade Office",                       "Trade Office",        "Greater Accra", "Accra",      "Lapaz Main Road, Accra",                        2,   8, "{Company} trade office for Lapaz, Achimota, and the western Accra trade belt",                                "+233 30 320 5106", "lapaz.trade"),
        new("TO-AS-004", "Konongo Trade Office",                     "Trade Office",        "Ashanti",       "Konongo",    "Konongo Market, Konongo",                       2,   8, "{Company} trade office for Konongo and the Asante-Akim general-trade corridor",                                "+233 32 220 5104", "konongo.trade"),
        new("TO-AS-005", "Mampong Trade Office",                     "Trade Office",        "Ashanti",       "Mampong",    "Mampong Market, Mampong",                       2,   8, "{Company} trade office serving Mampong, Effiduase, and the northern Ashanti corridor",                          "+233 32 220 5105", "mampong.trade"),
        new("TO-CE-001", "Winneba Trade Office",                     "Trade Office",        "Central",       "Winneba",    "Winneba Junction, Winneba",                     2,   8, "{Company} trade office for Winneba, Swedru, and the Effutu coastal trade",                                     "+233 33 220 5102", "winneba.trade"),
        new("TO-EA-003", "Suhum Trade Office",                       "Trade Office",        "Eastern",       "Suhum",      "Suhum Market, Suhum",                           2,   8, "{Company} trade office for Suhum and the Akuapem South commercial belt",                                       "+233 34 220 5103", "suhum.trade"),
        new("TO-VO-002", "Aflao Border Trade Office",                "Trade Office",        "Volta",         "Aflao",      "Aflao Border Post, Aflao",                      2,   8, "{Company} trade office at the Aflao border serving cross-border trade with Togo and the Volta southern corridor", "+233 36 220 5102", "aflao.trade")
    ];
}
