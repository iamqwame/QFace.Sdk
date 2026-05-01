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
        // Corporate tier uses the curated Big-Four-style 30-station catalogue verbatim:
        // Head Office on Independence Avenue / Ridge in Accra, regional offices in
        // Kumasi / Takoradi / Tamale, client service centres at major commercial hubs,
        // embedded project sites at large client engagements (oil & gas, telcos, banks),
        // and dedicated training / innovation hubs. Other tiers fall back to the
        // procedural city-pool builder so smaller firms land with a reasonable shape.
        if (tier == CompanyTier.Corporate)
        {
            var hqRow = _serviceStations[0];
            var rest = _serviceStations.Skip(1).ToList();
            var branchTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Regional Office", "Client Service Centre", "Innovation Hub"
            };
            return new StationLayout(
                Headquarters: hqRow,
                Branches: rest.Where(s => branchTypes.Contains(s.StationType)).ToList(),
                Satellites: rest.Where(s => !branchTypes.Contains(s.StationType)).ToList());
        }

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

    // Service-line job buckets — drives EligibleJobTitleCodes on each org unit.
    private static readonly IReadOnlyList<string> ExecJobs        = ["CSP-001", "MP-001", "COO-001", "CFO-001", "CHRO-001"];
    private static readonly IReadOnlyList<string> AuditJobs       = ["AP-001", "AD-001", "AM-001", "AS-001", "AJ-001", "P-001"];
    private static readonly IReadOnlyList<string> TaxJobs         = ["TP-001", "TD-001", "TM-001", "TS-001", "TPS-001", "VAT-001"];
    private static readonly IReadOnlyList<string> AdvisoryJobs    = ["AVP-001", "STR-001", "OPS-001", "TECH-001", "MAA-001", "VAL-001", "SBA-001", "BA-001"];
    private static readonly IReadOnlyList<string> RiskJobs        = ["RAM-001", "IAS-001", "FOR-001", "CO-001"];
    private static readonly IReadOnlyList<string> BdJobs          = ["BDM-001", "BID-001", "PROP-001"];
    private static readonly IReadOnlyList<string> FinanceJobs     = ["FM-001", "ACC-001"];
    private static readonly IReadOnlyList<string> PeopleCultJobs  = ["TAL-001", "PEO-001", "KM-001", "TRN-001"];
    private static readonly IReadOnlyList<string> OpsJobs         = ["OA-001", "REC-001"];
    private static readonly IReadOnlyList<string> ItJobs          = ["ITS-001"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",    "Founder/Managing Partner", null,      OrgUnitKind.Executive, ExecJobs),
        new("CONSULTANT", "Consulting",               "FOUNDER", OrgUnitKind.Function,  AdvisoryJobs)
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
        new("AUDIT",    "Audit & Assurance", "EXEC", OrgUnitKind.Function,  AuditJobs),
        new("TAX",      "Tax",               "EXEC", OrgUnitKind.Function,  TaxJobs),
        new("BD",       "Business Development", "EXEC", OrgUnitKind.Function, BdJobs),
        new("FINANCE",  "Finance & Admin",   "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("IT",       "IT Services",       "EXEC", OrgUnitKind.Function,  ItJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.08,
        ["ADVISORY"] = 0.30,
        ["AUDIT"]    = 0.25,
        ["TAX"]      = 0.15,
        ["BD"]       = 0.10,
        ["FINANCE"]  = 0.07,
        ["IT"]       = 0.05
    };

    // Corporate-tier baseline OrgUnits — each carries rich Description / Budget /
    // CostCenter / Purpose / Phone / Email-local-part modelled on KPMG/PwC/Deloitte/EY
    // Ghana office shapes. The {Company} placeholder gets substituted with the actual
    // tenant's company name at row-emit time so the same catalogue reads naturally for
    // any consulting or professional-services tenant. Phone numbers and budget ranges
    // are representative for a top-tier Big-4-class advisory firm in Ghana.
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",      "Office of the Country Senior Partner", null, OrgUnitKind.Executive, ExecJobs,
            Description: "Office of the Country Senior Partner and the executive committee of {Company}",
            BudgetMin: 1_500_000m, BudgetMax: 3_500_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Set and execute {Company}'s Ghana strategy; lead the partnership; manage regulator, ICAG, and global network relationships",
            Phone: "+233 30 XXX XXXX", Email: "executive"),
        new("AUDIT",     "Audit & Assurance",                "EXEC", OrgUnitKind.Function, AuditJobs,
            Description: "Provides external audit and assurance services to {Company}'s clients across financial services, public sector, and corporates",
            BudgetMin: 4_500_000m, BudgetMax: 12_000_000m,
            CostCenter: "CC-AUDIT-001",
            Purpose: "Sustain audit quality, regulatory compliance, and grow market share at {Company}",
            Phone: "+233 30 XXX XXXX", Email: "audit"),
        new("TAX",       "Tax Services",                     "EXEC", OrgUnitKind.Function, TaxJobs,
            Description: "Corporate tax, indirect tax, transfer pricing, and tax controversy services delivered by {Company}'s tax practice",
            BudgetMin: 3_000_000m, BudgetMax: 8_500_000m,
            CostCenter: "CC-TAX-001",
            Purpose: "Deliver tax compliance, planning, and dispute-resolution services to {Company}'s clients across all GRA tax heads",
            Phone: "+233 30 XXX XXXX", Email: "tax"),
        new("ADVISORY",  "Advisory (Strategy / Operations / Tech)", "EXEC", OrgUnitKind.Function, AdvisoryJobs,
            Description: "Management consulting, strategy, operations improvement, and technology transformation services at {Company}",
            BudgetMin: 5_000_000m, BudgetMax: 12_000_000m,
            CostCenter: "CC-ADV-001",
            Purpose: "Help {Company} clients solve their hardest strategic, operational, and digital-transformation problems",
            Phone: "+233 30 XXX XXXX", Email: "advisory"),
        new("RISK",      "Risk Advisory & Assurance",        "EXEC", OrgUnitKind.Function, RiskJobs,
            Description: "Internal audit, forensic services, governance and risk advisory delivered by {Company}'s risk practice",
            BudgetMin: 2_000_000m, BudgetMax: 6_000_000m,
            CostCenter: "CC-RISK-001",
            Purpose: "Support {Company} clients on enterprise risk, internal audit outsourcing, fraud investigations, and SOX-style controls",
            Phone: "+233 30 XXX XXXX", Email: "risk"),
        new("BD",        "Markets & Business Development",   "EXEC", OrgUnitKind.Function, BdJobs,
            Description: "Pursuits, proposals, brand, and account management — {Company}'s revenue engine for new and existing clients",
            BudgetMin: 800_000m, BudgetMax: 1_800_000m,
            CostCenter: "CC-BD-001",
            Purpose: "Win and grow {Company} client relationships through targeted pursuits, marketing, and account-led BD",
            Phone: "+233 30 XXX XXXX", Email: "markets"),
        new("FINANCE",   "Finance & Accounts",               "EXEC", OrgUnitKind.Function, FinanceJobs,
            Description: "Financial reporting, partner profitability, billing, and management accounting for {Company}",
            BudgetMin: 600_000m, BudgetMax: 1_400_000m,
            CostCenter: "CC-FIN-001",
            Purpose: "Run {Company}'s books, manage partner distributions, drive WIP-to-cash, and steward firm capital",
            Phone: "+233 30 XXX XXXX", Email: "finance"),
        new("PEOPLE",    "People & Culture",                 "EXEC", OrgUnitKind.Function, PeopleCultJobs,
            Description: "Talent acquisition, learning, total rewards, and people operations — {Company}'s people function",
            BudgetMin: 700_000m, BudgetMax: 1_600_000m,
            CostCenter: "CC-PPL-001",
            Purpose: "Attract, develop, and retain the talent {Company} needs to deliver to its clients; champion the firm's culture",
            Phone: "+233 30 XXX XXXX", Email: "people"),
        new("OPS",       "Operations & Administration",      "EXEC", OrgUnitKind.Function, OpsJobs,
            Description: "Office services, facilities, reception, and travel for {Company} across Accra and the regional offices",
            BudgetMin: 500_000m, BudgetMax: 1_000_000m,
            CostCenter: "CC-OPS-001",
            Purpose: "Run {Company}'s offices smoothly so client-facing teams can focus on delivery",
            Phone: "+233 30 XXX XXXX", Email: "operations"),
        new("IT",        "Information Technology",           "EXEC", OrgUnitKind.Function, ItJobs,
            Description: "End-user support, productivity tooling, cybersecurity, and audit-tech infrastructure at {Company}",
            BudgetMin: 600_000m, BudgetMax: 1_400_000m,
            CostCenter: "CC-IT-001",
            Purpose: "Run {Company}'s technology platform with high availability, security, and the engagement-tech the practice depends on",
            Phone: "+233 30 XXX XXXX", Email: "it")
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.04,
        ["AUDIT"]    = 0.30,
        ["TAX"]      = 0.15,
        ["ADVISORY"] = 0.22,
        ["RISK"]     = 0.10,
        ["BD"]       = 0.05,
        ["FINANCE"]  = 0.04,
        ["PEOPLE"]   = 0.04,
        ["OPS"]      = 0.03,
        ["IT"]       = 0.03
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",        "Executive",   null,   OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMS",    "Programs",    "EXEC", OrgUnitKind.Function,  AdvisoryJobs),
        new("SERVICES",    "Services",    "EXEC", OrgUnitKind.Function,  AdvisoryJobs),
        new("FUNDRAISING", "Fundraising", "EXEC", OrgUnitKind.Function,  BdJobs),
        new("ADMIN",       "Admin",       "EXEC", OrgUnitKind.Function,  OpsJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]        = 0.12,
        ["PROGRAMS"]    = 0.40,
        ["SERVICES"]    = 0.30,
        ["FUNDRAISING"] = 0.13,
        ["ADMIN"]       = 0.05
    };

    // Cal-Bank-grade enriched job-title catalogue for a Ghana professional-services /
    // consulting firm at KPMG / PwC / Deloitte / EY scale.
    //
    // Salary bands (GHS / month):
    //   rank 1 (Junior / Trainee / Intern):       2,500 -   6,000
    //   rank 2 (Associate / Specialist / Officer): 5,500 -  13,000
    //   rank 3 (Senior / Manager-grade):          12,000 -  28,000
    //   rank 4 (Senior Mgr / Director):           26,000 -  60,000
    //   rank 5 (Partner / MP / Country Senior):   55,000 - 150,000
    //
    // Pay grade ladder S-1 (Analyst) → S-9 (Country Senior Partner).
    // Annual leave: 30 (rank 5) / 27 (rank 4) / 24 (rank 3) / 21 (rank 2) / 21 (rank 1).
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // ── Executive / Partnership ───────────────────────────────────────────────
        new("CSP-001", "Country Senior Partner", 5, 90_000m, 150_000m, "EXEC", null, true,
            "Master's Degree", 20,
            "Firm Strategy, Partnership Leadership, Stakeholder Management, Global Network",
            Description: "Most senior partner at {Company}; chairs the Ghana partnership and represents the firm to regulators, clients, and the global network",
            PayGrade: "S-9",
            Responsibilities: "Set {Company} Ghana strategy; lead the partnership; chair the Country Leadership Team; represent {Company} to ICAG, GRA, BoG, and clients; uphold quality and risk standards across all service lines",
            KeyPerformanceIndicators: "Firm revenue growth, partner profitability, audit quality scores, client NPS, partner attrition, global network compliance",
            AnnualLeaveEntitlementDays: 30),
        new("MP-001", "Managing Partner", 5, 80_000m, 140_000m, "EXEC", "CSP-001", true,
            "Master's Degree", 18,
            "P&L Ownership, Operations Leadership, Partner Coaching, Strategic Planning",
            Description: "Day-to-day operational leader of {Company}, accountable for firm P&L and execution of strategy",
            PayGrade: "S-9",
            Responsibilities: "Run {Company} operations; deliver firm P&L; coach partners; chair the operations committee; champion talent and quality",
            KeyPerformanceIndicators: "Net revenue, EBITDA margin, utilisation, realisation, partner CSAT, talent retention",
            AnnualLeaveEntitlementDays: 30),
        new("COO-001", "Chief Operating Officer", 5, 60_000m, 110_000m, "EXEC", "MP-001", true,
            "Master's Degree", 15,
            "Operations, Shared Services, Vendor Management, Transformation",
            Description: "COO of {Company} — owns shared services, IT, facilities, and operational transformation",
            PayGrade: "S-8",
            Responsibilities: "Run firm-wide operations; lead transformation programmes; manage vendor portfolio; oversee IT, facilities, and admin",
            KeyPerformanceIndicators: "Operating cost ratio, vendor savings, IT uptime, facilities NPS, transformation milestones",
            AnnualLeaveEntitlementDays: 30),
        new("CFO-001", "Chief Financial Officer", 5, 60_000m, 110_000m, "FINANCE", "MP-001", true,
            "Master's Degree", 15,
            "Financial Strategy, Partner Distributions, Treasury, Tax",
            Description: "CFO of {Company}; runs finance, treasury, partner distributions, and statutory reporting",
            PayGrade: "S-8",
            Responsibilities: "Own {Company} finance function; manage partner units and distributions; deliver statutory accounts; treasury and capital management",
            KeyPerformanceIndicators: "Cash conversion, WIP days, AR days, partner distribution accuracy, audit findings",
            AnnualLeaveEntitlementDays: 30),
        new("CHRO-001", "Chief People Officer", 5, 55_000m, 100_000m, "PEOPLE", "MP-001", true,
            "Master's Degree", 15,
            "Talent Strategy, Reward, Culture, Learning",
            Description: "Most senior HR partner — leads talent, reward, culture, and learning at {Company}",
            PayGrade: "S-8",
            Responsibilities: "Set people strategy; lead talent acquisition and L&D; design reward; champion DE&I and culture; partner with the partnership on succession",
            KeyPerformanceIndicators: "Voluntary attrition, time-to-hire, eNPS, learning hours, leadership bench strength",
            AnnualLeaveEntitlementDays: 30),

        // ── Audit & Assurance ─────────────────────────────────────────────────────
        new("AP-001", "Audit Partner", 5, 70_000m, 130_000m, "AUDIT", "CSP-001", true,
            "Chartered Accountant (ICAG / ACCA)", 15,
            "Audit Quality, ISA, IFRS, Banking & Insurance Audit, Public Sector",
            Description: "Signing partner on {Company} audit engagements; accountable for audit quality and client portfolio",
            PayGrade: "S-9",
            Responsibilities: "Sign audit opinions; lead audit engagement teams; manage partner-level client relationships; oversee independence and quality",
            KeyPerformanceIndicators: "Audit quality review scores, engagement realisation, client portfolio revenue, ICAG compliance, peer review outcomes",
            AnnualLeaveEntitlementDays: 30),
        new("P-001", "Partner", 5, 60_000m, 120_000m, "AUDIT", "MP-001", true,
            "Master's Degree", 14,
            "Engagement Leadership, Practice Development, Quality Review",
            Description: "Equity partner at {Company} with cross-service-line responsibilities and a portfolio of major clients",
            PayGrade: "S-9",
            Responsibilities: "Own partner-level clients; sponsor pursuits; oversee quality and risk on engagements; coach senior managers and directors",
            KeyPerformanceIndicators: "Portfolio revenue, realisation, client NPS, mentee progression, pursuits won",
            AnnualLeaveEntitlementDays: 30),
        new("AD-001", "Audit Director", 4, 40_000m, 60_000m, "AUDIT", "AP-001", true,
            "Chartered Accountant (ICAG / ACCA)", 12,
            "Audit Methodology, IFRS, ISA, Engagement Economics",
            Description: "Senior audit leader at {Company} just below partner; runs the largest audit engagements",
            PayGrade: "S-7",
            Responsibilities: "Direct large audit engagements; review manager work; manage engagement economics; deputise for partner on quality",
            KeyPerformanceIndicators: "Engagement margin, on-time filing rate, review note resolution, manager retention, pipeline conversion",
            AnnualLeaveEntitlementDays: 27),
        new("AM-001", "Audit Manager", 4, 26_000m, 42_000m, "AUDIT", "AD-001", true,
            "Chartered Accountant (ICAG / ACCA)", 7,
            "Audit Execution, Team Management, IFRS, ISA",
            Description: "Day-to-day manager of {Company} audit engagements — plans, executes, and reviews fieldwork",
            PayGrade: "S-6",
            Responsibilities: "Plan audits; manage seniors and juniors; review workpapers; clear review notes; manage budgets and client communication",
            KeyPerformanceIndicators: "Engagement budget vs actual, review-note ratio, team utilisation, client CSAT, on-time delivery",
            AnnualLeaveEntitlementDays: 27),
        new("AS-001", "Audit Senior", 3, 14_000m, 22_000m, "AUDIT", "AM-001", false,
            "Bachelor's Degree (Accounting / Finance) + ACCA Part-Qualified", 4,
            "Audit Sampling, Substantive Testing, Workpaper Review, Excel",
            Description: "Audit Senior at {Company} — runs fieldwork on engagements and supervises juniors",
            PayGrade: "S-5",
            Responsibilities: "Lead in-charge fieldwork; supervise juniors; complete complex audit areas; draft management letter points; track engagement progress",
            KeyPerformanceIndicators: "Hours-to-budget ratio, junior coaching scores, workpaper review pass rate, client interaction quality",
            AnnualLeaveEntitlementDays: 24),
        new("AJ-001", "Audit Junior", 1, 3_500m, 5_500m, "AUDIT", "AS-001", false,
            "Bachelor's Degree (Accounting / Finance)", 0,
            "Vouching, Footing, Confirmations, Excel, IDEA / ACL Basics",
            Description: "Entry-level audit team member at {Company}; rotates across engagements as part of the graduate programme",
            PayGrade: "S-1",
            Responsibilities: "Complete substantive procedures under supervision; document workpapers; attend stocktakes and confirmations; build audit fundamentals",
            KeyPerformanceIndicators: "Workpaper quality, hours-to-budget, attendance, ACCA exam progress",
            AnnualLeaveEntitlementDays: 21),

        // ── Tax ───────────────────────────────────────────────────────────────────
        new("TP-001", "Tax Partner", 5, 65_000m, 125_000m, "TAX", "CSP-001", true,
            "Chartered Tax Practitioner (CITG) / ICAG", 14,
            "Corporate Tax, Tax Controversy, Transfer Pricing, GRA Strategy",
            Description: "Signing partner on {Company}'s major tax engagements and tax-controversy mandates",
            PayGrade: "S-9",
            Responsibilities: "Sign tax opinions; lead controversy and TP engagements; build the tax practice; manage GRA and Ministry of Finance relationships",
            KeyPerformanceIndicators: "Tax practice revenue, dispute success rate, opinion turnaround, client NPS, technical publications",
            AnnualLeaveEntitlementDays: 30),
        new("TD-001", "Tax Director", 4, 38_000m, 58_000m, "TAX", "TP-001", true,
            "Chartered Tax Practitioner (CITG)", 10,
            "Tax Planning, Compliance, Transfer Pricing Documentation",
            Description: "Tax Director at {Company} leading complex tax compliance and planning engagements",
            PayGrade: "S-7",
            Responsibilities: "Direct tax compliance and advisory mandates; review TP files; manage GRA audits; coach tax managers",
            KeyPerformanceIndicators: "Engagement margin, GRA audit outcomes, on-time filings, manager development",
            AnnualLeaveEntitlementDays: 27),
        new("TM-001", "Tax Manager", 4, 26_000m, 40_000m, "TAX", "TD-001", true,
            "ICAG / CITG / ACCA", 6,
            "Corporate Tax, VAT, PAYE, Tax Returns, GRA Engagement",
            Description: "Tax Manager at {Company} managing day-to-day delivery on tax compliance and advisory engagements",
            PayGrade: "S-6",
            Responsibilities: "Manage tax engagements end-to-end; review returns and computations; lead client meetings; clear GRA queries",
            KeyPerformanceIndicators: "Realisation, engagement budget, filing accuracy, GRA penalty avoidance, client CSAT",
            AnnualLeaveEntitlementDays: 27),
        new("TS-001", "Tax Senior", 3, 13_000m, 20_000m, "TAX", "TM-001", false,
            "Bachelor's Degree + ACCA / CITG Part-Qualified", 3,
            "Tax Computations, Returns Preparation, GRA Portal, Excel",
            Description: "Tax Senior at {Company} preparing computations, reviewing junior work, and engaging with the GRA",
            PayGrade: "S-5",
            Responsibilities: "Prepare and review corporate tax returns; handle VAT and PAYE; respond to GRA queries; coach tax associates",
            KeyPerformanceIndicators: "Return-filing accuracy, hours-to-budget, GRA query turnaround",
            AnnualLeaveEntitlementDays: 24),
        new("TPS-001", "Transfer Pricing Specialist", 3, 14_000m, 22_000m, "TAX", "TM-001", false,
            "Master's Degree (Tax / Economics)", 4,
            "Transfer Pricing, OECD Guidelines, Benchmarking, Documentation",
            Description: "Transfer pricing specialist at {Company} producing TP documentation and benchmarking studies",
            PayGrade: "S-5",
            Responsibilities: "Prepare TP documentation; run benchmarking studies; respond to GRA TP audits; advise on intercompany pricing",
            KeyPerformanceIndicators: "Documentation defensibility, benchmarking turnaround, audit defence outcomes",
            AnnualLeaveEntitlementDays: 24),
        new("VAT-001", "VAT / Indirect Tax Specialist", 3, 12_000m, 19_000m, "TAX", "TM-001", false,
            "Bachelor's Degree + CITG", 3,
            "VAT, NHIL, GETFund, Customs, Excise",
            Description: "Indirect tax specialist at {Company} covering VAT, NHIL, GETFund and customs/excise queries",
            PayGrade: "S-5",
            Responsibilities: "Advise on indirect-tax classification; review VAT returns; handle GRA VAT audits; train clients on indirect tax compliance",
            KeyPerformanceIndicators: "VAT recovery rate, client penalty avoidance, training delivered",
            AnnualLeaveEntitlementDays: 24),

        // ── Advisory / Consulting ─────────────────────────────────────────────────
        new("AVP-001", "Advisory Partner", 5, 68_000m, 130_000m, "ADVISORY", "CSP-001", true,
            "Master's Degree (MBA preferred)", 14,
            "Strategy, Operations, Digital, Client Relationship Leadership",
            Description: "Partner leading {Company}'s advisory / consulting practice in Ghana",
            PayGrade: "S-9",
            Responsibilities: "Build the advisory practice; sponsor major transformations; manage strategic client relationships; chair quality and risk on advisory engagements",
            KeyPerformanceIndicators: "Practice revenue, partner pipeline, win rate on pursuits, client NPS, manager promotions",
            AnnualLeaveEntitlementDays: 30),
        new("STR-001", "Strategy Consultant (Director)", 4, 32_000m, 52_000m, "ADVISORY", "AVP-001", true,
            "Master's Degree (MBA preferred)", 9,
            "Corporate Strategy, Market Entry, Growth, Operating Models",
            Description: "Director-grade strategy consultant at {Company} leading corporate strategy and growth engagements",
            PayGrade: "S-7",
            Responsibilities: "Lead strategy engagements; design operating models; deliver market-entry studies; coach consultants and analysts",
            KeyPerformanceIndicators: "Engagement margin, repeat business, NPS, intellectual property contributions",
            AnnualLeaveEntitlementDays: 27),
        new("OPS-001", "Operations Consultant (Director)", 4, 30_000m, 50_000m, "ADVISORY", "AVP-001", true,
            "Master's Degree", 9,
            "Process Improvement, Lean Six Sigma, Cost Optimisation, Supply Chain",
            Description: "Director-grade operations consultant at {Company} leading cost-out and process-redesign engagements",
            PayGrade: "S-7",
            Responsibilities: "Lead operations engagements; deliver process redesigns; deliver cost-takeout programmes; coach consultants",
            KeyPerformanceIndicators: "Client savings delivered, engagement margin, NPS, methodology adoption",
            AnnualLeaveEntitlementDays: 27),
        new("TECH-001", "Technology Consultant (Director)", 4, 32_000m, 55_000m, "ADVISORY", "AVP-001", true,
            "Master's Degree (Computer Science / IS)", 9,
            "Digital Transformation, ERP, Cloud, Cybersecurity",
            Description: "Director-grade technology consultant at {Company} leading digital and ERP transformation programmes",
            PayGrade: "S-7",
            Responsibilities: "Lead tech transformation engagements; advise on ERP, cloud, and cyber; manage delivery and vendor partners",
            KeyPerformanceIndicators: "Programme on-time / on-budget delivery, client NPS, partner-tech revenue",
            AnnualLeaveEntitlementDays: 27),
        new("MAA-001", "M&A Analyst", 3, 13_000m, 22_000m, "ADVISORY", "STR-001", false,
            "Bachelor's Degree (Finance / Economics)", 3,
            "Financial Modelling, M&A, Valuation, Due Diligence",
            Description: "M&A analyst at {Company} supporting deal advisory and due diligence engagements",
            PayGrade: "S-5",
            Responsibilities: "Build deal models; run valuation analyses; support financial due diligence; prepare information memoranda",
            KeyPerformanceIndicators: "Model accuracy, turnaround, deal-team feedback, hours-to-budget",
            AnnualLeaveEntitlementDays: 24),
        new("VAL-001", "Valuations Analyst", 3, 12_000m, 20_000m, "ADVISORY", "STR-001", false,
            "Bachelor's Degree (Finance) + CFA Level 2+", 3,
            "Business Valuation, DCF, Comparable Multiples, Purchase Price Allocation",
            Description: "Valuations analyst at {Company} delivering business and asset valuations for clients",
            PayGrade: "S-5",
            Responsibilities: "Build DCF and comparable models; deliver valuation reports; support PPA and IFRS 3 fair-value work",
            KeyPerformanceIndicators: "Report defensibility, turnaround, peer-review pass rate",
            AnnualLeaveEntitlementDays: 24),
        new("SBA-001", "Senior Business Analyst", 3, 13_000m, 21_000m, "ADVISORY", "STR-001", false,
            "Bachelor's Degree", 4,
            "Business Analysis, Requirements, Process Mapping, Workshops",
            Description: "Senior BA at {Company} leading requirements and process work on advisory engagements",
            PayGrade: "S-5",
            Responsibilities: "Lead requirements gathering; run workshops; document target operating models; mentor junior BAs",
            KeyPerformanceIndicators: "Requirements sign-off cycle time, client CSAT, junior BA progression",
            AnnualLeaveEntitlementDays: 24),
        new("BA-001", "Business Analyst", 2, 6_500m, 11_000m, "ADVISORY", "SBA-001", false,
            "Bachelor's Degree", 1,
            "Process Mapping, Excel, PowerPoint, Workshops",
            Description: "Business analyst at {Company} supporting consulting engagements with analysis and documentation",
            PayGrade: "S-3",
            Responsibilities: "Run analysis; document processes and requirements; build models and slides; support workshops",
            KeyPerformanceIndicators: "Deliverable quality, turnaround, hours-to-budget",
            AnnualLeaveEntitlementDays: 21),

        // ── Risk Advisory / Internal Audit / Forensics ────────────────────────────
        new("RAM-001", "Risk Advisory Manager", 4, 27_000m, 42_000m, "RISK", "P-001", true,
            "ICAG / ACCA / CIA", 6,
            "Internal Audit, ERM, SOX, Controls Testing",
            Description: "Risk advisory manager at {Company} leading internal-audit outsource and ERM engagements",
            PayGrade: "S-6",
            Responsibilities: "Lead internal-audit and risk engagements; review controls testing; deliver risk-assessment reports; manage seniors and juniors",
            KeyPerformanceIndicators: "Engagement margin, finding quality, client NPS, repeat-business rate",
            AnnualLeaveEntitlementDays: 27),
        new("IAS-001", "Internal Audit Senior", 3, 13_000m, 21_000m, "RISK", "RAM-001", false,
            "Bachelor's Degree + CIA Part-Qualified", 3,
            "Internal Audit, Controls Testing, Risk Assessment",
            Description: "Internal-audit senior at {Company} executing controls testing and risk-based audit fieldwork",
            PayGrade: "S-5",
            Responsibilities: "Execute IA fieldwork; document findings; supervise juniors; prepare draft reports",
            KeyPerformanceIndicators: "Workpaper quality, finding acceptance rate, hours-to-budget",
            AnnualLeaveEntitlementDays: 24),
        new("FOR-001", "Forensic Accountant", 3, 14_000m, 23_000m, "RISK", "RAM-001", false,
            "ICAG / ACCA + CFE", 4,
            "Fraud Investigation, Forensic Accounting, Data Analytics, Litigation Support",
            Description: "Forensic accountant at {Company} delivering fraud investigations and litigation-support engagements",
            PayGrade: "S-5",
            Responsibilities: "Run forensic engagements; perform data analytics; interview witnesses; prepare expert reports",
            KeyPerformanceIndicators: "Case turnaround, recovery rate, expert-report defensibility",
            AnnualLeaveEntitlementDays: 24),
        new("CO-001", "Compliance Officer", 3, 12_000m, 19_000m, "RISK", "RAM-001", false,
            "Bachelor's Degree (Law / Finance)", 3,
            "AML / CFT, Independence, Engagement Acceptance, Regulatory Compliance",
            Description: "Compliance officer at {Company} running independence checks, engagement acceptance, and AML / KYC",
            PayGrade: "S-5",
            Responsibilities: "Run independence checks; clear engagement acceptance; manage AML / KYC; train staff on compliance",
            KeyPerformanceIndicators: "Independence breach rate, KYC turnaround, training completion",
            AnnualLeaveEntitlementDays: 24),

        // ── People / Change / Training (advisory side) ────────────────────────────
        new("HRC-001", "HR Consultant", 3, 13_000m, 21_000m, "ADVISORY", "AVP-001", false,
            "Master's Degree (HR / Organisational Behaviour)", 4,
            "HR Strategy, Reward Design, Talent, Org Design",
            Description: "HR consultant at {Company} delivering people-and-org engagements for clients",
            PayGrade: "S-5",
            Responsibilities: "Deliver org-design and HR engagements; benchmark reward; design competency frameworks; support clients on HR transformation",
            KeyPerformanceIndicators: "Engagement margin, client NPS, deliverable quality",
            AnnualLeaveEntitlementDays: 24),
        new("CHM-001", "Change Management Consultant", 3, 13_000m, 22_000m, "ADVISORY", "AVP-001", false,
            "Bachelor's Degree + Prosci / CCMP", 3,
            "Change Management, Communications, Stakeholder Engagement, Training",
            Description: "Change management consultant at {Company} leading the people side of transformation programmes",
            PayGrade: "S-5",
            Responsibilities: "Run change-impact assessments; design communications and training; coach client sponsors; measure adoption",
            KeyPerformanceIndicators: "Adoption metrics, training completion, sponsor CSAT",
            AnnualLeaveEntitlementDays: 24),
        new("TRN-001", "Training Specialist", 2, 7_000m, 12_500m, "PEOPLE", "CHRO-001", false,
            "Bachelor's Degree", 2,
            "Learning Design, Facilitation, LMS, Curriculum",
            Description: "Training specialist at {Company} designing and delivering learning programmes for staff and clients",
            PayGrade: "S-3",
            Responsibilities: "Design and deliver learning programmes; manage the LMS; track CPD compliance; run client training when required",
            KeyPerformanceIndicators: "Training NPS, CPD compliance, learning-hours-per-FTE",
            AnnualLeaveEntitlementDays: 21),

        // ── Generic consulting career-track overflow ──────────────────────────────
        new("SM-001", "Senior Manager", 4, 28_000m, 45_000m, "ADVISORY", "AVP-001", true,
            "Master's Degree", 8,
            "Engagement Leadership, Practice Building, Client Management",
            Description: "Senior manager at {Company} just below director — leads complex engagements and develops the practice",
            PayGrade: "S-7",
            Responsibilities: "Lead engagements; manage client relationships; coach managers; contribute to pursuits and practice development",
            KeyPerformanceIndicators: "Realisation, manager retention, sold-work value, client NPS",
            AnnualLeaveEntitlementDays: 27),
        new("M-001", "Manager", 4, 26_000m, 38_000m, "ADVISORY", "SM-001", true,
            "Bachelor's Degree (CA / MBA preferred)", 6,
            "Engagement Management, Team Leadership, Quality Review",
            Description: "Manager at {Company} running consulting and audit engagements end-to-end",
            PayGrade: "S-6",
            Responsibilities: "Manage engagements; review work; manage budgets; coach seniors and associates",
            KeyPerformanceIndicators: "Engagement margin, on-time delivery, team CSAT, client NPS",
            AnnualLeaveEntitlementDays: 27),
        new("SA-001", "Senior Associate", 3, 12_500m, 19_000m, "ADVISORY", "M-001", false,
            "Bachelor's Degree", 3,
            "Analysis, Workstream Ownership, Team Coaching, Excel / PowerPoint",
            Description: "Senior associate at {Company} owning workstreams on engagements and coaching juniors",
            PayGrade: "S-5",
            Responsibilities: "Own engagement workstreams; coach associates; prepare deliverables; lead client interactions on workstream",
            KeyPerformanceIndicators: "Hours-to-budget, deliverable quality, junior development, manager review scores",
            AnnualLeaveEntitlementDays: 24),
        new("ASSOC-001", "Associate", 2, 7_500m, 12_500m, "ADVISORY", "SA-001", false,
            "Bachelor's Degree", 1,
            "Analysis, Research, Modelling, Client Documentation",
            Description: "Associate at {Company} contributing analysis, research, and deliverables on engagements",
            PayGrade: "S-3",
            Responsibilities: "Run analyses; build models and slides; support client meetings; develop technical skills",
            KeyPerformanceIndicators: "Hours-to-budget, deliverable quality, exam progress",
            AnnualLeaveEntitlementDays: 21),
        new("JA-001", "Junior Associate", 1, 4_500m, 6_000m, "ADVISORY", "ASSOC-001", false,
            "Bachelor's Degree", 0,
            "Research, Excel, PowerPoint, Note-Taking",
            Description: "Entry-level associate at {Company}; rotates across service lines as part of the graduate programme",
            PayGrade: "S-1",
            Responsibilities: "Support analyses; gather data; prepare drafts; build core consulting skills",
            KeyPerformanceIndicators: "Deliverable quality, learning velocity, hours-to-budget, exam progress",
            AnnualLeaveEntitlementDays: 21),

        // ── Markets / Business Development ────────────────────────────────────────
        new("BDM-001", "Marketing & BD Manager", 3, 14_000m, 23_000m, "BD", "MP-001", true,
            "Bachelor's Degree (Marketing / Business)", 5,
            "Marketing, Brand, BD, Pursuits, Events",
            Description: "BD and marketing manager at {Company} running pursuits, brand, and demand generation",
            PayGrade: "S-6",
            Responsibilities: "Lead BD and brand programmes; manage pursuits pipeline; deliver events and thought leadership; manage agency partners",
            KeyPerformanceIndicators: "Pursuits win rate, MQL volume, brand-share-of-voice, event ROI",
            AnnualLeaveEntitlementDays: 24),
        new("BID-001", "Bid Manager", 3, 12_000m, 19_000m, "BD", "BDM-001", false,
            "Bachelor's Degree", 4,
            "Bid Management, Proposals, Pricing, RFP Response",
            Description: "Bid manager at {Company} owning proposal lifecycle for major pursuits",
            PayGrade: "S-5",
            Responsibilities: "Run proposal lifecycle; assemble bid teams; coordinate pricing; track pursuits in CRM",
            KeyPerformanceIndicators: "Win rate, on-time submission rate, content-reuse ratio",
            AnnualLeaveEntitlementDays: 24),
        new("PROP-001", "Proposal Writer", 2, 7_000m, 12_000m, "BD", "BID-001", false,
            "Bachelor's Degree (English / Communications)", 2,
            "Proposal Writing, Editing, Storyboarding",
            Description: "Proposal writer at {Company} producing high-quality client-ready proposal documents",
            PayGrade: "S-3",
            Responsibilities: "Write and edit proposals; storyboard responses; maintain content library; ensure brand compliance",
            KeyPerformanceIndicators: "On-time submission, win rate, editorial quality",
            AnnualLeaveEntitlementDays: 21),

        // ── Support — Office / Reception / IT / Finance / People Ops / Knowledge ─
        new("OA-001", "Office Administrator", 2, 6_000m, 10_500m, "OPS", "COO-001", false,
            "Diploma", 2,
            "Office Administration, Vendor Management, Travel, Documents",
            Description: "Office administrator at {Company}; runs day-to-day office operations and vendor coordination",
            PayGrade: "S-3",
            Responsibilities: "Manage office services and vendors; coordinate travel and visas; manage facilities; support engagement logistics",
            KeyPerformanceIndicators: "Vendor SLA, travel-booking turnaround, internal CSAT",
            AnnualLeaveEntitlementDays: 21),
        new("REC-001", "Receptionist", 1, 3_000m, 5_000m, "OPS", "OA-001", false,
            "Diploma", 0,
            "Customer Service, Switchboard, Front Desk",
            Description: "Front-of-house receptionist at {Company}; first point of contact for clients and visitors",
            PayGrade: "S-1",
            Responsibilities: "Welcome clients; manage switchboard and meeting rooms; handle courier and post; maintain visitor logs",
            KeyPerformanceIndicators: "Visitor CSAT, call-handling speed, attendance",
            AnnualLeaveEntitlementDays: 21),
        new("ITS-001", "IT Support", 2, 7_000m, 12_500m, "IT", "COO-001", false,
            "Bachelor's Degree (Computer Science / IS)", 2,
            "Helpdesk, Endpoint Support, M365, Networking",
            Description: "IT support at {Company} providing endpoint, M365, and engagement-tech support to staff",
            PayGrade: "S-3",
            Responsibilities: "Resolve helpdesk tickets; provision laptops and accounts; support audio-visual for client events; maintain endpoint security baselines",
            KeyPerformanceIndicators: "First-call-resolution, ticket SLA, endpoint-compliance rate",
            AnnualLeaveEntitlementDays: 21),
        new("FM-001", "Finance Manager", 4, 26_000m, 40_000m, "FINANCE", "CFO-001", true,
            "ICAG / ACCA", 6,
            "Financial Reporting, IFRS, Management Accounts, Billing",
            Description: "Finance manager at {Company} running day-to-day finance, billing, and management reporting",
            PayGrade: "S-6",
            Responsibilities: "Run finance operations; deliver monthly management accounts; manage billing and AR; support statutory audit",
            KeyPerformanceIndicators: "Close days, AR days, audit-finding count, budget accuracy",
            AnnualLeaveEntitlementDays: 27),
        new("ACC-001", "Accountant", 2, 7_500m, 12_500m, "FINANCE", "FM-001", false,
            "Bachelor's Degree + ACCA Part-Qualified", 2,
            "Bookkeeping, Reconciliations, Payroll, Excel",
            Description: "Accountant at {Company} handling bookkeeping, reconciliations, and finance operations",
            PayGrade: "S-3",
            Responsibilities: "Post journals; run reconciliations; process payroll inputs; support month-end close",
            KeyPerformanceIndicators: "Reconciliation accuracy, close-day adherence, payroll error rate",
            AnnualLeaveEntitlementDays: 21),
        new("TAL-001", "Talent Acquisition Lead", 3, 13_000m, 21_000m, "PEOPLE", "CHRO-001", true,
            "Bachelor's Degree", 4,
            "Talent Acquisition, Campus, Sourcing, Employer Brand",
            Description: "TA lead at {Company} owning end-to-end recruiting across experienced and graduate hires",
            PayGrade: "S-5",
            Responsibilities: "Run experienced and graduate hiring; manage employer brand and campus; lead assessment-centre design",
            KeyPerformanceIndicators: "Time-to-hire, offer-acceptance rate, quality-of-hire, campus pipeline",
            AnnualLeaveEntitlementDays: 24),
        new("PEO-001", "People Operations", 2, 7_000m, 12_000m, "PEOPLE", "CHRO-001", false,
            "Bachelor's Degree", 2,
            "HR Operations, HRIS, Onboarding, Records, Payroll Inputs",
            Description: "People operations specialist at {Company}; runs HRIS, onboarding, and lifecycle administration",
            PayGrade: "S-3",
            Responsibilities: "Run onboarding and offboarding; maintain HRIS and records; manage benefits administration; support payroll",
            KeyPerformanceIndicators: "HRIS data accuracy, onboarding NPS, ticket SLA",
            AnnualLeaveEntitlementDays: 21),
        new("KM-001", "Knowledge Manager", 3, 13_000m, 20_000m, "PEOPLE", "CHRO-001", false,
            "Master's Degree (Library / IS)", 4,
            "Knowledge Management, Curation, SharePoint / Confluence, Taxonomy",
            Description: "Knowledge manager at {Company} curating engagement knowledge, methodologies, and proposals library",
            PayGrade: "S-5",
            Responsibilities: "Curate engagement and pursuit content; design taxonomy; run KM tooling; train teams on KM practice",
            KeyPerformanceIndicators: "Reuse rate, KM platform adoption, content-freshness score",
            AnnualLeaveEntitlementDays: 24)
    ];

    // Curated 30-station catalogue for a Big-4-class professional-services firm in
    // Ghana. HQ on Independence Avenue / Ridge in Accra, regional offices in the
    // major regional capitals, client service centres in commercial hubs, embedded
    // project sites at major client engagements (oil & gas, telcos, banks), plus
    // training and innovation hubs. Email values store ONLY the local part (before @)
    // so the row factory appends the actual company TLD at runtime.
    private static readonly StationSpec[] _serviceStations =
    [
        // 1 HQ
        new("HO-001", "Head Office - Accra (Ridge)", "Head Office", "Greater Accra", "Accra", "12 Independence Avenue, Ridge", 100, 700, "{Company}'s Ghana headquarters housing the Country Senior Partner, partnership, and all national service-line leadership", "+233 30 XXX XXXX", "headoffice"),

        // 3 Regional Offices
        new("RO-ASH-001", "Ashanti Regional Office - Kumasi", "Regional Office", "Ashanti", "Kumasi", "Prempeh II Street, Adum", 30, 90, "{Company} regional office serving Ashanti-Region clients across audit, tax, and advisory", "+233 32 XXX XXXX", "kumasi.office"),
        new("RO-WES-001", "Western Regional Office - Takoradi", "Regional Office", "Western", "Takoradi", "Harbour Road, Market Circle", 25, 80, "{Company} regional office anchoring the oil & gas client portfolio in the Western Region", "+233 31 XXX XXXX", "takoradi.office"),
        new("RO-NOR-001", "Northern Regional Office - Tamale", "Regional Office", "Northern", "Tamale", "Salaga Road, Central Tamale", 20, 60, "{Company} regional office supporting public-sector, donor-funded, and agribusiness engagements in the north", "+233 37 XXX XXXX", "tamale.office"),

        // 6 Client Service Centres
        new("CSC-APC-001", "Airport City Client Service Centre", "Client Service Centre", "Greater Accra", "Accra", "2 Airport City, Accra", 30, 90, "{Company} client service centre at Airport City supporting telco, oil & gas, and corporate-HQ clients", "+233 30 XXX XXXX", "airportcity"),
        new("CSC-CAN-001", "Cantonments Client Service Centre", "Client Service Centre", "Greater Accra", "Accra", "Switchback Road, Cantonments", 25, 80, "{Company} client service centre serving diplomatic, donor, and embassy-linked clients in Cantonments", "+233 30 XXX XXXX", "cantonments"),
        new("CSC-EL-001",  "East Legon Client Service Centre", "Client Service Centre", "Greater Accra", "Accra", "Boundary Road, East Legon", 20, 70, "{Company} client service centre serving the East Legon corporate and HNW client community", "+233 30 XXX XXXX", "eastlegon"),
        new("CSC-TEM-001", "Tema Client Service Centre", "Client Service Centre", "Greater Accra", "Tema", "Community 1, Tema Central", 25, 80, "{Company} client service centre supporting Tema Port operators, manufacturers, and shipping clients", "+233 30 XXX XXXX", "tema"),
        new("CSC-CAP-001", "Cape Coast Client Service Centre", "Client Service Centre", "Central", "Cape Coast", "Commercial Street, Cape Coast", 15, 50, "{Company} client service centre serving Central-Region public-sector and education-sector clients", "+233 33 XXX XXXX", "capecoast"),
        new("CSC-SUN-001", "Sunyani Client Service Centre", "Client Service Centre", "Bono", "Sunyani", "Fiapre Road, Sunyani", 15, 45, "{Company} client service centre supporting agribusiness, cocoa, and public-sector clients in the Bono Region", "+233 35 XXX XXXX", "sunyani"),
        new("CSC-HO-001",  "Ho Client Service Centre", "Client Service Centre", "Volta", "Ho", "Ho-Aflao Road, Ho Central", 12, 40, "{Company} client service centre supporting public-sector and donor-funded engagements in the Volta Region", "+233 36 XXX XXXX", "ho"),

        // 13 Project Sites (embedded teams at large client engagements)
        new("PS-OIL-001", "Tullow Oil Embedded Project Site", "Project Site", "Western", "Takoradi", "Tullow Oil Compound, Takoradi", 8, 30, "{Company} embedded engagement team running internal-audit and tax compliance work at a major upstream oil operator", "+233 31 XXX XXXX", "tullow.site"),
        new("PS-OIL-002", "Jubilee FPSO Project Site", "Project Site", "Western", "Takoradi", "Jubilee Operations Base, Takoradi Port", 6, 20, "{Company} project site supporting reservoir-economics and joint-venture audit work for an offshore Jubilee partner", "+233 31 XXX XXXX", "jubilee.site"),
        new("PS-OIL-003", "GNPC Project Site", "Project Site", "Greater Accra", "Accra", "GNPC Tower, Airport City", 8, 25, "{Company} embedded team delivering corporate-governance and PSA-advisory work at the national petroleum corporation", "+233 30 XXX XXXX", "gnpc.site"),
        new("PS-FS-001",  "Tier-1 Bank Project Site - Accra", "Project Site", "Greater Accra", "Accra", "Independence Avenue, Ridge", 10, 35, "{Company} embedded engagement team delivering year-round audit, IFRS-9, and BoG regulatory advisory at a tier-1 commercial bank", "+233 30 XXX XXXX", "tier1bank.site"),
        new("PS-FS-002",  "Insurance Group Project Site", "Project Site", "Greater Accra", "Accra", "Independence Avenue, Ridge", 6, 20, "{Company} embedded team running actuarial, IFRS-17, and audit work at a tier-1 insurance group", "+233 30 XXX XXXX", "insurance.site"),
        new("PS-FS-003",  "Pension Fund Manager Project Site", "Project Site", "Greater Accra", "Accra", "Liberation Road, Ridge", 5, 18, "{Company} embedded team delivering NPRA-compliance, custody, and audit-readiness support at a leading pension fund manager", "+233 30 XXX XXXX", "pension.site"),
        new("PS-TEL-001", "Telco-A Project Site", "Project Site", "Greater Accra", "Accra", "Independence Avenue, Ridge", 8, 25, "{Company} embedded engagement team delivering revenue-assurance and tax-controversy support at a leading mobile-network operator", "+233 30 XXX XXXX", "telco.a.site"),
        new("PS-TEL-002", "Telco-B Project Site", "Project Site", "Greater Accra", "Accra", "Liberation Road, Airport Residential", 6, 20, "{Company} project site supporting digital-transformation and IT-audit work at a major telco operator", "+233 30 XXX XXXX", "telco.b.site"),
        new("PS-PUB-001", "Ministry of Finance Project Site", "Project Site", "Greater Accra", "Accra", "28th February Road, Ministries", 6, 20, "{Company} embedded team supporting public-financial-management and donor-funded projects at the Ministry of Finance", "+233 30 XXX XXXX", "mofep.site"),
        new("PS-PUB-002", "Ghana Revenue Authority Project Site", "Project Site", "Greater Accra", "Accra", "Off Liberation Road, Ridge", 5, 18, "{Company} project site supporting tax-administration modernisation and revenue-mobilisation engagements with GRA", "+233 30 XXX XXXX", "gra.site"),
        new("PS-MIN-001", "AngloGold Ashanti Project Site - Obuasi", "Project Site", "Ashanti", "Obuasi", "AngloGold Ashanti Mine, Obuasi", 6, 20, "{Company} embedded team delivering audit, tax, and operational-improvement work at a major gold-mining operator", "+233 32 XXX XXXX", "obuasi.site"),
        new("PS-MAN-001", "Manufacturing Group Project Site - Tema", "Project Site", "Greater Accra", "Tema", "Heavy Industrial Area, Tema", 6, 22, "{Company} embedded team running ERP-implementation and finance-transformation work at a tier-1 Tema-based manufacturer", "+233 30 XXX XXXX", "tema.manuf.site"),
        new("PS-AGR-001", "Cocoa Board Project Site - Tema", "Project Site", "Greater Accra", "Tema", "Cocoa House, Tema", 5, 18, "{Company} embedded team supporting Cocobod's commercial-operations and donor-financed advisory engagements", "+233 30 XXX XXXX", "cocobod.site"),

        // 6 Training / Innovation hubs
        new("TC-001", "Learning & Development Centre - Accra", "Training Centre", "Greater Accra", "Accra", "Spintex Road, Accra", 10, 50, "{Company}'s national learning centre — runs onboarding, ACCA / ICAG study sessions, methodology training, and partner-led workshops", "+233 30 XXX XXXX", "learning"),
        new("TC-002", "Audit & Assurance Training Centre", "Training Centre", "Greater Accra", "Accra", "Liberation Road, Ridge", 8, 30, "{Company}'s dedicated audit & assurance training centre running engagement-tech and methodology courses for the audit practice", "+233 30 XXX XXXX", "audittraining"),
        new("IH-001", "Digital Innovation Hub - Accra", "Innovation Hub", "Greater Accra", "Accra", "Accra Digital Centre, Accra", 8, 35, "{Company}'s digital innovation hub — co-locates the technology-consulting practice with start-ups and engineers from the Accra Digital Centre", "+233 30 XXX XXXX", "innovation"),
        new("IH-002", "Cyber Centre of Excellence - Accra", "Innovation Hub", "Greater Accra", "Accra", "Airport Residential, Accra", 6, 25, "{Company}'s cyber centre of excellence supporting cybersecurity, data-protection, and digital-trust engagements", "+233 30 XXX XXXX", "cyber"),
        new("IH-003", "Innovation & ESG Lab - Accra", "Innovation Hub", "Greater Accra", "Accra", "Cantonments, Accra", 5, 20, "{Company}'s ESG and sustainability lab supporting climate-disclosure, ESG-assurance, and impact-advisory engagements", "+233 30 XXX XXXX", "esg"),
        new("IH-004", "Tax Technology Lab - Accra", "Innovation Hub", "Greater Accra", "Accra", "Liberation Road, Ridge", 5, 20, "{Company}'s tax-technology lab building automation and data-analytics tooling for the tax practice", "+233 30 XXX XXXX", "taxtech")
    ];
}
