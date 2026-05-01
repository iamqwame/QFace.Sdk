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
        // Corporate tier uses the curated software-company station catalogue verbatim.
        // HQ = first entry; "Engineering Centre" / "Customer Operations Hub" become
        // branches; smaller "Co-working Hub" satellites round out the regional spread.
        // Other tiers fall back to the procedural city-pool builder so smaller and
        // non-profit software shops land with a sensible shape.
        if (tier == CompanyTier.Corporate)
        {
            var hqRow = _softwareStations[0];
            var rest = _softwareStations.Skip(1).ToList();
            var branchTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Engineering Centre", "Customer Operations Hub"
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

    private static readonly IReadOnlyList<string> ExecJobs        = ["CEO-001", "COO-001", "CFO-001", "CTO-001"];
    private static readonly IReadOnlyList<string> EngJobs         = ["VPE-001", "EM-001", "TL-001", "STAFF-001", "SE-001", "SE-002", "SE-003", "JR-001", "MOB-IOS-001", "MOB-AND-001", "FE-001", "BE-001", "FS-001"];
    private static readonly IReadOnlyList<string> ProductJobs     = ["VPP-001", "PM-001", "PM-002", "APM-001"];
    private static readonly IReadOnlyList<string> DesignJobs      = ["VPD-001", "PD-001", "UXR-001"];
    private static readonly IReadOnlyList<string> DataJobs        = ["HD-001", "DE-001", "ML-001", "DS-001", "DA-001", "BIA-001"];
    private static readonly IReadOnlyList<string> InfraJobs       = ["HDO-001", "SRE-001", "DO-001", "CE-001", "PE-001"];
    private static readonly IReadOnlyList<string> SecurityJobs    = ["HSEC-001", "SEC-001"];
    private static readonly IReadOnlyList<string> QaJobs          = ["QAL-001", "QAS-001", "QA-001", "TAE-001"];
    private static readonly IReadOnlyList<string> GtmJobs         = ["SEN-001", "CSL-001", "CSM-001", "DRE-001", "SA-001", "IE-001", "TW-001"];
    private static readonly IReadOnlyList<string> PeopleJobs      = ["HMR-PM-001", "ER-001", "PO-001"];
    private static readonly IReadOnlyList<string> FinanceJobs     = ["FM-001", "AC-001"];
    private static readonly IReadOnlyList<string> OpsJobs         = ["OM-001", "IT-001"];
    private static readonly IReadOnlyList<string> ProgramsJobs    = ["PM-001", "PM-002", "APM-001"];

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
        new("CEO",     "Chief Executive",   null,  OrgUnitKind.Executive, ExecJobs),
        new("ENG",     "Engineering",       "CEO", OrgUnitKind.Function,  EngJobs),
        new("PRODUCT", "Product",           "CEO", OrgUnitKind.Function,  ProductJobs),
        new("DESIGN",  "Design",            "CEO", OrgUnitKind.Function,  DesignJobs),
        new("QA",      "Quality Assurance", "ENG", OrgUnitKind.Function,  QaJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["CEO"]     = 0.05,
        ["ENG"]     = 0.50,
        ["PRODUCT"] = 0.15,
        ["DESIGN"]  = 0.20,
        ["QA"]      = 0.10
    };

    // Corporate-tier baseline OrgUnits — each carries rich Description / Budget /
    // CostCenter / Purpose / Phone / Email-local-part appropriate for a Ghana
    // software / fintech company at Hubtel / mPharma / Zeepay scale. The {Company}
    // placeholder gets substituted with the actual tenant's company name at row-emit
    // time. Phone numbers and budget ranges are representative for a venture-backed
    // tech company headquartered in Accra running a multi-product platform.
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC", "Executive Office", null, OrgUnitKind.Executive, ExecJobs,
            Description: "Office of the CEO and executive leadership team of {Company}",
            BudgetMin: 1_500_000m, BudgetMax: 3_000_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Set and execute {Company} corporate strategy, manage investor relationships, drive board governance and capital strategy",
            Phone: "+233 30 222 0100", Email: "executive"),
        new("ENGINEERING", "Engineering", "EXEC", OrgUnitKind.Function, EngJobs,
            Description: "Builds and operates {Company}'s product platform, mobile and web applications, and core services",
            BudgetMin: 4_000_000m, BudgetMax: 8_000_000m,
            CostCenter: "CC-ENG-001",
            Purpose: "Ship reliable software at high cadence; sustain platform reliability and developer productivity at {Company}",
            Phone: "+233 30 222 0200", Email: "engineering"),
        new("PRODUCT", "Product Management", "EXEC", OrgUnitKind.Function, ProductJobs,
            Description: "Owns product strategy, roadmap, discovery, and prioritisation across {Company}'s product lines",
            BudgetMin: 1_200_000m, BudgetMax: 2_500_000m,
            CostCenter: "CC-PRD-001",
            Purpose: "Discover and ship product bets that grow {Company}'s user base, retention, and monetisation",
            Phone: "+233 30 222 0300", Email: "product"),
        new("DESIGN", "Design & User Research", "EXEC", OrgUnitKind.Function, DesignJobs,
            Description: "Crafts {Company}'s product experience, design system, brand, and user research practice",
            BudgetMin: 600_000m, BudgetMax: 1_400_000m,
            CostCenter: "CC-DSN-001",
            Purpose: "Deliver experiences that are useful, usable, and unmistakably {Company}; codify the design system and research insight pipeline",
            Phone: "+233 30 222 0400", Email: "design"),
        new("DATA", "Data & Analytics", "EXEC", OrgUnitKind.Function, DataJobs,
            Description: "Owns {Company}'s data platform, ML models, business intelligence, and analytics for product and ops",
            BudgetMin: 1_000_000m, BudgetMax: 2_200_000m,
            CostCenter: "CC-DAT-001",
            Purpose: "Turn {Company}'s operational data into product, growth, and risk insight; run production ML systems",
            Phone: "+233 30 222 0500", Email: "data"),
        new("INFRA-PLATFORM", "Infrastructure & Platform", "EXEC", OrgUnitKind.Function, InfraJobs,
            Description: "Operates {Company}'s cloud, observability, CI/CD, internal developer platform, and SRE practice",
            BudgetMin: 1_500_000m, BudgetMax: 3_000_000m,
            CostCenter: "CC-INF-001",
            Purpose: "Keep {Company} running 24/7 with strong SLOs; provide a self-service platform engineers love to use",
            Phone: "+233 30 222 0600", Email: "platform"),
        new("SECURITY", "Security & Compliance", "EXEC", OrgUnitKind.Function, SecurityJobs,
            Description: "Owns {Company}'s application security, infrastructure security, incident response, and regulatory compliance",
            BudgetMin: 800_000m, BudgetMax: 1_800_000m,
            CostCenter: "CC-SEC-001",
            Purpose: "Protect {Company} customers, data, and systems; achieve and maintain SOC 2, PCI-DSS, and BoG/Data Protection compliance",
            Phone: "+233 30 222 0700", Email: "security"),
        new("GO-TO-MARKET", "Go-To-Market", "EXEC", OrgUnitKind.Function, GtmJobs,
            Description: "Sales engineering, marketing, customer success, technical writing, and developer relations for {Company}",
            BudgetMin: 1_200_000m, BudgetMax: 3_000_000m,
            CostCenter: "CC-GTM-001",
            Purpose: "Win, onboard, and grow {Company} customers; build the developer community around the {Company} platform",
            Phone: "+233 30 222 0800", Email: "gtm"),
        new("PEOPLE", "People Operations", "EXEC", OrgUnitKind.Function, PeopleJobs,
            Description: "Talent acquisition, people operations, learning, and workplace experience at {Company}",
            BudgetMin: 500_000m, BudgetMax: 1_200_000m,
            CostCenter: "CC-PPL-001",
            Purpose: "Attract and retain world-class engineering, product, and design talent; grow {Company} from startup to scale-up culture",
            Phone: "+233 30 222 0900", Email: "people"),
        new("FINANCE", "Finance & Accounting", "EXEC", OrgUnitKind.Function, FinanceJobs,
            Description: "Financial planning, accounting, treasury, payroll, and investor reporting for {Company}",
            BudgetMin: 400_000m, BudgetMax: 900_000m,
            CostCenter: "CC-FIN-001",
            Purpose: "Maintain accurate {Company} financials, manage runway and capital plans, support fundraising and audit",
            Phone: "+233 30 222 1000", Email: "finance"),
        new("OPS", "IT & Workplace Operations", "EXEC", OrgUnitKind.Function, OpsJobs,
            Description: "Internal IT, endpoint management, identity, and workplace tooling support for {Company} employees",
            BudgetMin: 300_000m, BudgetMax: 700_000m,
            CostCenter: "CC-OPS-001",
            Purpose: "Keep {Company} employees productive on every device, in every office, with secure and reliable IT services",
            Phone: "+233 30 222 1100", Email: "itops")
    ];

    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]            = 0.03,
        ["ENGINEERING"]     = 0.40,
        ["PRODUCT"]         = 0.08,
        ["DESIGN"]          = 0.05,
        ["DATA"]            = 0.07,
        ["INFRA-PLATFORM"]  = 0.10,
        ["SECURITY"]        = 0.04,
        ["GO-TO-MARKET"]    = 0.13,
        ["PEOPLE"]          = 0.04,
        ["FINANCE"]         = 0.03,
        ["OPS"]             = 0.03
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive",   null,   OrgUnitKind.Executive, ExecJobs),
        new("ENG",      "Engineering", "EXEC", OrgUnitKind.Function,  EngJobs),
        new("PRODUCT",  "Product",     "EXEC", OrgUnitKind.Function,  ProductJobs),
        new("PROGRAMS", "Programs",    "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("HR",       "HR & Admin",  "EXEC", OrgUnitKind.Function,  PeopleJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.10,
        ["ENG"]      = 0.40,
        ["PRODUCT"]  = 0.20,
        ["PROGRAMS"] = 0.20,
        ["HR"]       = 0.10
    };

    // Curated ~50-role catalogue for a Ghana software / fintech company. Codes follow
    // role-prefix conventions (SE = Software Engineer, EM = Engineering Manager, PM =
    // Product Manager, DS = Data Scientist, etc.). PayGrade uses L1..L8 for the IC
    // engineering ladder, M1..M6 for managers, EX for the C-suite. Salaries in GHS/month
    // anchor to the rank-level bands defined in the brief.
    // {Company} placeholder is substituted at row-emit time so the same catalogue reads
    // naturally for any tenant.
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // ── Executive (rank 5) ──────────────────────────────────────────────────
        new("CEO-001", "Chief Executive Officer", 5, 80_000m, 120_000m, "EXEC", null, true, "Master's Degree", 15, "Executive Leadership, Corporate Strategy, Fundraising, Investor Relations, P&L Management", "Chief executive accountable for {Company}'s overall strategy, performance, and stakeholder relations", "EX", "Set and execute {Company} strategy; lead executive team; manage investor and board relationships; own company-wide P&L; champion culture and hiring", "Annual Recurring Revenue, Net Revenue Retention, Burn Multiple, Headcount Plan vs Actual", 30),
        new("CTO-001", "Chief Technology Officer", 5, 70_000m, 110_000m, "ENGINEERING", "CEO-001", true, "Master's Degree", 15, "Technology Strategy, Distributed Systems, Engineering Leadership, Architecture, Cloud, Security", "Executive accountable for {Company}'s technology strategy, engineering org, platform architecture, and technical talent", "EX", "Define {Company} technical strategy and architecture; lead engineering, infrastructure, and security orgs; partner on product roadmap; own platform reliability and security posture", "Platform Uptime SLO, Engineering Velocity, Security Incident Rate, Eng Attrition, Architecture Health Score", 30),
        new("COO-001", "Chief Operating Officer", 5, 65_000m, 105_000m, "EXEC", "CEO-001", true, "Master's Degree", 15, "Operations Leadership, Cross-Functional Execution, Process Design, Vendor Management, Scaling", "Executive responsible for day-to-day execution across {Company} go-to-market, customer operations, and people functions", "EX", "Operationalise the {Company} strategy; align GTM, CS, people, and finance; own quarterly OKRs; manage vendor and partner relationships; scale operating cadence", "Operating Plan Attainment, Quarterly OKR Score, Customer Operations SLA, GTM Productivity Ratio", 30),
        new("CFO-001", "Chief Finance Officer", 5, 65_000m, 105_000m, "FINANCE", "CEO-001", true, "Master's / ACCA / ICAG", 15, "Financial Strategy, FP&A, Fundraising, Treasury, IFRS Reporting, Investor Relations, Audit", "Executive responsible for {Company}'s financial strategy, planning, accounting, treasury, and investor reporting", "EX", "Lead financial planning and analysis; manage cash, treasury, and runway; produce IFRS financial statements; support fundraising and due diligence; oversee tax, payroll, and audit", "Months of Runway, Burn Variance to Plan, Audit Outcome, Forecast Accuracy, Fundraise Closure Time", 30),
        new("VPE-001", "VP Engineering", 5, 60_000m, 95_000m, "ENGINEERING", "CTO-001", true, "Master's Degree", 12, "Engineering Leadership, Org Scaling, Delivery, Performance Management, Coaching, Technical Strategy", "Senior leader running {Company}'s engineering org day to day, owning delivery, headcount, and engineering culture", "EX", "Lead engineering managers and tech leads; own quarterly delivery commitments; manage hiring plan and engineering budget; coach managers; partner with product and design on roadmap", "On-Time Delivery Rate, Engineering NPS, Time-to-First-Commit, Manager Span Health, Critical Bug Escape Rate", 30),
        new("VPP-001", "VP Product", 5, 60_000m, 95_000m, "PRODUCT", "CEO-001", true, "Master's Degree", 12, "Product Strategy, Roadmapping, Discovery, Pricing, Analytics, Stakeholder Management", "Senior leader owning {Company}'s product strategy, roadmap, and product management organisation", "EX", "Set product strategy and roadmap; lead PMs and product analysts; own product KPIs; collaborate with design, engineering, and GTM; manage portfolio prioritisation", "Activation Rate, Feature Adoption, Product-Qualified Leads, Roadmap Hit Rate, Pricing Realisation", 30),
        new("VPD-001", "VP Design", 5, 60_000m, 90_000m, "DESIGN", "CEO-001", true, "Master's / Design Degree", 12, "Design Leadership, Design Systems, User Research, Brand, Service Design, Coaching", "Senior leader owning product design, design systems, brand, and user research at {Company}", "EX", "Lead the design team across product, brand, and research; own and evolve the {Company} design system; champion customer-centric design; raise the bar on craft", "Design System Adoption, Usability Test Pass Rate, Time-to-Design, NPS on Design Quality, Designer Retention", 30),
        new("HD-001", "Head of Data", 5, 60_000m, 90_000m, "DATA", "CTO-001", true, "Master's Degree", 12, "Data Platform, Analytics, ML/AI, Data Governance, Team Leadership, BI Strategy", "Leads {Company}'s data platform, analytics, and machine learning function", "EX", "Build and run {Company}'s data warehouse and ML platform; lead data engineers, scientists, and analysts; partner with product and finance on metrics; own data governance and privacy", "Data Pipeline Reliability, Model SLO Attainment, Self-Serve Analytics Adoption, Data Quality Score, Time-to-Insight", 30),
        new("HDO-001", "Head of DevOps & Platform", 5, 60_000m, 92_000m, "INFRA-PLATFORM", "CTO-001", true, "Master's Degree", 12, "SRE, Cloud Architecture, Kubernetes, Observability, Internal Developer Platform, Cost Optimisation", "Leads {Company}'s cloud infrastructure, SRE, internal developer platform, and reliability practice", "EX", "Run multi-region cloud platform; lead SRE and platform engineering; own SLOs and incident response; drive infrastructure cost efficiency; build self-service developer tooling", "Platform Uptime, Mean Time to Recovery, Cloud Cost per Active User, Deployment Frequency, Change Failure Rate", 30),
        new("HSEC-001", "Head of Security", 5, 60_000m, 92_000m, "SECURITY", "CTO-001", true, "Master's / CISSP / CISM", 12, "Application Security, Cloud Security, Incident Response, Compliance, Threat Modelling, Risk Management", "Owns application, infrastructure, and corporate security plus regulatory compliance for {Company}", "EX", "Run the security programme end-to-end; lead AppSec, SecOps, and GRC; achieve and maintain SOC 2, PCI-DSS, and BoG compliance; lead incident response; train all engineers on secure coding", "Security Incident Severity Rate, Compliance Audit Outcome, Mean Time to Patch Critical CVE, Phishing Test Pass Rate, Pen Test Findings Closure", 30),

        // ── Senior Management (rank 4) ──────────────────────────────────────────
        new("EM-001", "Engineering Manager", 4, 38_000m, 60_000m, "ENGINEERING", "VPE-001", true, "Bachelor's in CS", 8, "People Management, Agile Delivery, 1:1 Coaching, Performance Reviews, Technical Leadership, Roadmap Planning", "Manages a 6-10 person product engineering team at {Company}, owning delivery, growth, and culture", "M3", "Run weekly 1:1s and team rituals; own quarterly delivery commitments; coach engineers on growth; partner with PM and design; manage hiring loop and onboarding", "Sprint Velocity Stability, Team Engagement Score, Promotion Rate, Bug Escape Rate, Hiring Funnel Health", 27),
        new("TL-001", "Tech Lead", 4, 35_000m, 55_000m, "ENGINEERING", "EM-001", false, "Bachelor's in CS", 7, "Software Architecture, Code Review, Mentoring, Cross-Team Collaboration, Distributed Systems", "Senior IC providing technical direction for a {Company} engineering squad without people-management duties", "L7", "Set technical direction for the squad; lead design reviews; mentor engineers on craft; drive cross-squad architecture decisions; own critical code paths", "Design Doc Quality, Cross-Squad Architecture Wins, Code Review Throughput, Mentee Promotion Rate, Production Incidents Owned", 27),
        new("STAFF-001", "Staff Engineer", 4, 40_000m, 65_000m, "ENGINEERING", "VPE-001", false, "Bachelor's in CS", 9, "System Design, Distributed Systems, Performance, Mentoring, Technical Strategy, Cross-Org Influence", "Org-wide senior IC at {Company} who drives multi-team technical strategy and tackles {Company}'s hardest engineering problems", "L8", "Lead multi-quarter technical initiatives; author RFCs; review platform-wide architecture; mentor staff and senior engineers; advise leadership on tech strategy", "Cross-Team Initiative Impact, RFC Acceptance Rate, Mentee Career Progression, Production Reliability Improvement, Strategic Bet Outcomes", 27),
        new("PM-001", "Senior Product Manager", 4, 35_000m, 58_000m, "PRODUCT", "VPP-001", false, "Bachelor's Degree", 7, "Product Discovery, Roadmapping, Pricing, Analytics, A/B Testing, Stakeholder Management, SQL", "Senior PM owning a major {Company} product surface end-to-end with full P&L responsibility for the area", "L7", "Own a product surface roadmap and KPIs; run discovery and customer interviews; partner with engineering, design, and data; ship A/B tests; report monthly to leadership", "Activation Rate Lift, Feature Adoption, Quarterly Revenue Impact, Experiment Velocity, Customer NPS Delta", 27),
        new("HMR-PM-001", "Engineering Recruiter", 4, 32_000m, 55_000m, "PEOPLE", null, false, "Bachelor's Degree", 6, "Technical Recruiting, Sourcing, Interview Design, Employer Branding, Compensation Benchmarking", "Owns engineering and product hiring at {Company}, from sourcing through offer", "M2", "Source senior engineering and product candidates; design and run interview loops; manage offer negotiations; partner with hiring managers; own employer branding for {Company}", "Time-to-Fill, Offer Acceptance Rate, Source-of-Hire Quality, Interview Loop Health, Diversity of Pipeline", 27),

        // ── Engineering IC ladder (rank 3-2-1) ──────────────────────────────────
        new("SE-001", "Senior Software Engineer", 3, 25_000m, 35_000m, "ENGINEERING", "EM-001", false, "Bachelor's in CS", 5, "C# / TypeScript / Go, System Design, REST APIs, SQL, Cloud, Code Review, Mentoring", "Experienced IC at {Company} owning significant features end-to-end and mentoring more junior engineers", "L5", "Design and implement medium-to-large features; mentor mid and junior engineers; lead design reviews on the squad; on-call for owned services; partner with PM on scoping", "Feature Delivery on Plan, Code Review Quality, Production Incident Ownership, Mentee Growth, Tech Debt Reduction", 24),
        new("SE-002", "Software Engineer II", 3, 18_000m, 28_000m, "ENGINEERING", "SE-001", false, "Bachelor's in CS", 3, "C# / TypeScript / Python, REST APIs, SQL, Git, Cloud Basics, Testing", "Mid-level engineer at {Company} delivering well-scoped features with limited supervision", "L4", "Implement features against well-defined specs; write unit and integration tests; participate in code review and design discussions; on-call for owned services", "Feature Delivery on Plan, Code Review Participation, Bug Resolution Time, Test Coverage Maintained", 24),
        new("SE-003", "Software Engineer I", 2, 11_000m, 17_000m, "ENGINEERING", "SE-002", false, "Bachelor's in CS", 1, "Programming Fundamentals, Git, REST APIs, SQL, Testing, One Production Language", "Early-career engineer at {Company} delivering small to medium features with mentorship", "L3", "Implement small to medium features under guidance; write tests; participate in code review; learn the codebase and developer workflow; pair with senior engineers", "Feature Completion Rate, Code Review Cycle Time, Onboarding Milestone Progress, Test Coverage Added", 21),
        new("JR-001", "Junior Engineer", 1, 5_500m, 8_000m, "ENGINEERING", "SE-003", false, "Bachelor's in CS", 0, "Programming Fundamentals, Git, Pair Programming, Basic Web/Mobile", "Entry-level engineer at {Company} building skills through pair programming and mentorship", "L1", "Pair-program with senior engineers; close starter and bug-fix tickets; learn the {Company} codebase, deployment, and review workflow; complete the engineering bootcamp curriculum", "Onboarding Bootcamp Score, Tickets Closed per Sprint, Code Review Iteration Count, Mentor Feedback Rating", 21),
        new("MOB-IOS-001", "iOS Engineer", 3, 22_000m, 33_000m, "ENGINEERING", "EM-001", false, "Bachelor's in CS", 3, "Swift, SwiftUI, iOS SDK, REST/GraphQL, Combine, App Store Submission, XCTest", "Builds and ships {Company}'s iOS application with focus on performance, accessibility, and crash-free experience", "L4", "Implement iOS features in Swift/SwiftUI; integrate with backend APIs; manage App Store releases; maintain crash-free rate; partner with design on iOS-native UX", "App Store Crash-Free Rate, Release Cadence, App Store Rating, Cold-Start Time, Feature Delivery on Plan", 24),
        new("MOB-AND-001", "Android Engineer", 3, 22_000m, 33_000m, "ENGINEERING", "EM-001", false, "Bachelor's in CS", 3, "Kotlin, Jetpack Compose, Android SDK, REST/GraphQL, Coroutines, Play Store Release, JUnit", "Builds and ships {Company}'s Android application across the wide range of devices in Ghana and West Africa", "L4", "Implement Android features in Kotlin/Compose; integrate with backend APIs; manage Play Store releases; tune for low-end and offline devices; partner with design on Material UX", "Play Store Crash-Free Rate, ANR Rate, Release Cadence, Cold-Start Time, Feature Delivery on Plan", 24),
        new("FE-001", "Frontend Engineer", 3, 20_000m, 30_000m, "ENGINEERING", "EM-001", false, "Bachelor's in CS", 3, "TypeScript, React, Next.js, CSS, Web Performance, Accessibility, Testing", "Builds {Company}'s web product surfaces with focus on performance, accessibility, and design-system fidelity", "L4", "Implement product surfaces in React/Next.js; collaborate closely with design on UX; maintain the {Company} component library; tune Core Web Vitals; write component and end-to-end tests", "Core Web Vitals Pass Rate, Component Library Adoption, Accessibility Score, Feature Delivery on Plan, Test Coverage", 24),
        new("BE-001", "Backend Engineer", 3, 22_000m, 33_000m, "ENGINEERING", "EM-001", false, "Bachelor's in CS", 3, "C# / Go / Java, REST/gRPC, PostgreSQL, Redis, Kafka, Distributed Systems, Cloud", "Builds and operates {Company}'s backend services, APIs, and data integrations", "L4", "Design and implement backend services and APIs; own database schemas and migrations; instrument services for observability; participate in on-call; integrate with payment, telco, and regulatory partners", "Service Uptime SLO, p95 Latency, Incident Ownership, API Stability, Feature Delivery on Plan", 24),
        new("FS-001", "Full-Stack Engineer", 3, 21_000m, 32_000m, "ENGINEERING", "EM-001", false, "Bachelor's in CS", 3, "TypeScript, React, Node.js / C#, SQL, REST APIs, Testing, Cloud Basics", "Generalist engineer at {Company} delivering end-to-end features across web frontend and backend services", "L4", "Ship end-to-end features across frontend and backend; own database changes for owned features; write tests across the stack; participate in design and code review; on-call rotation", "Feature Delivery on Plan, Cross-Stack Code Review Quality, Bug Escape Rate, Test Coverage", 24),

        // ── Data, Infra, Security IC ────────────────────────────────────────────
        new("DE-001", "Data Engineer", 3, 22_000m, 33_000m, "DATA", "HD-001", false, "Bachelor's in CS", 3, "Python, SQL, dbt, Airflow, Spark, Kafka, Data Modelling, Cloud Warehousing", "Builds and operates {Company}'s data pipelines, warehouse models, and streaming infrastructure", "L4", "Build batch and streaming pipelines; own dbt warehouse models; instrument data quality tests; partner with analysts on dataset design; manage cost and reliability of the warehouse", "Pipeline Reliability, Data Freshness SLO, Warehouse Cost per TB, Data Quality Test Pass Rate, Model Documentation Coverage", 24),
        new("ML-001", "Machine Learning Engineer", 3, 24_000m, 35_000m, "DATA", "HD-001", false, "Master's Degree", 4, "Python, PyTorch / TensorFlow, MLOps, Feature Stores, Model Serving, Experiment Design, Cloud", "Builds and ships production ML models for {Company} fraud, risk, recommendation, and credit-scoring use cases", "L4", "Train and ship production ML models; own feature pipelines and model serving; design offline and online experiments; partner with product on use-case framing; monitor model performance in production", "Model SLO Attainment, Online A/B Win Rate, Time-to-Production for New Model, Feature Pipeline Reliability, Experiment Velocity", 24),
        new("DS-001", "Data Scientist", 3, 22_000m, 33_000m, "DATA", "HD-001", false, "Master's Degree", 3, "Python, SQL, Statistics, Causal Inference, A/B Testing, Visualisation, Stakeholder Communication", "Drives {Company}'s product, growth, and risk decisions through statistical analysis, modelling, and experimentation", "L4", "Design and analyse A/B tests; build forecasting and segmentation models; partner with product and finance on KPIs; communicate insights to non-technical stakeholders; own analytics for assigned product surface", "Experiment Decision Quality, Stakeholder Insight Adoption, Analysis Turnaround, Forecast Accuracy, Documented Insights Shipped", 24),
        new("DA-001", "Data Analyst", 2, 12_000m, 18_000m, "DATA", "DS-001", false, "Bachelor's Degree", 2, "SQL, Python or R, Tableau / Looker, Statistics, Communication, Spreadsheets", "Produces dashboards, ad-hoc analysis, and reporting for {Company} product, ops, and leadership stakeholders", "L3", "Build self-serve dashboards in Looker or Tableau; respond to ad-hoc analytics requests; maintain core metrics definitions; train stakeholders on self-serve analytics; document datasets", "Dashboard Adoption, Analytics Request SLA, Metric Definition Accuracy, Self-Serve Adoption Rate", 21),
        new("BIA-001", "BI Analyst", 2, 12_000m, 18_000m, "DATA", "DS-001", false, "Bachelor's Degree", 2, "SQL, Looker / Power BI, Excel, Financial Modelling, Storytelling", "Owns {Company} executive and finance reporting, KPI dashboards, and management information packs", "L3", "Maintain executive KPI dashboards; produce monthly and quarterly business reviews; partner with finance on board reporting; own data definitions for governance metrics", "Reporting Pack Timeliness, Dashboard Accuracy, Finance Stakeholder Satisfaction, Data Definition Coverage", 21),
        new("SRE-001", "Site Reliability Engineer", 3, 24_000m, 35_000m, "INFRA-PLATFORM", "HDO-001", false, "Bachelor's in CS", 4, "Linux, Kubernetes, Terraform, Prometheus, Grafana, Incident Response, SLO Engineering, Go / Python", "Owns reliability of {Company}'s production services through SLOs, observability, and incident response", "L5", "Define and track SLOs for owned services; lead post-incident reviews; build production tooling and runbooks; own on-call quality; partner with product engineering on reliability improvements", "Service SLO Attainment, MTTR, Toil Hours per Week, On-Call Page Volume, Post-Incident Action Closure", 24),
        new("DO-001", "DevOps Engineer", 3, 20_000m, 30_000m, "INFRA-PLATFORM", "HDO-001", false, "Bachelor's in CS", 3, "CI/CD, Docker, Kubernetes, Terraform, Linux, Bash, GitHub Actions / Azure DevOps", "Builds and maintains {Company}'s CI/CD pipelines, build infrastructure, and deployment tooling", "L4", "Maintain CI/CD pipelines; manage build and deployment infrastructure; troubleshoot pipeline failures; automate developer workflows; partner with security on supply-chain controls", "Deploy Frequency, Pipeline Reliability, Build Time Reduction, Developer Self-Serve Adoption", 24),
        new("CE-001", "Cloud Engineer", 3, 22_000m, 33_000m, "INFRA-PLATFORM", "HDO-001", false, "Bachelor's in CS", 3, "AWS / Azure / GCP, Terraform, Networking, IAM, Cost Optimisation, Security Best Practices", "Designs, provisions, and optimises {Company}'s cloud infrastructure and networking footprint", "L4", "Provision cloud resources via Terraform; design VPC, networking, and IAM patterns; tune cloud cost; partner with security on cloud posture; own cloud account hygiene", "Cloud Cost per Active User, IaC Coverage, Cloud Security Findings Closed, Provisioning SLA", 24),
        new("PE-001", "Platform Engineer", 3, 24_000m, 35_000m, "INFRA-PLATFORM", "HDO-001", false, "Bachelor's in CS", 4, "Kubernetes, Backstage / IDP, Go / Python, Developer Experience, API Design, Service Mesh", "Builds {Company}'s internal developer platform, paved-path tooling, and self-service developer workflows", "L5", "Build and operate the {Company} internal developer platform; design golden paths for new services; own platform APIs and CLI; gather and act on developer feedback; reduce time-to-first-deploy", "Time-to-First-Deploy, Developer NPS, Paved-Path Adoption, Platform SLO, Onboarding Curriculum Completion", 24),
        new("SEC-001", "Security Engineer", 3, 24_000m, 35_000m, "SECURITY", "HSEC-001", false, "Bachelor's in CS", 4, "AppSec, Threat Modelling, Cloud Security, SAST/DAST, Incident Response, Cryptography, Secure SDLC", "Hands-on security engineer at {Company} doing threat modelling, AppSec reviews, and incident response", "L5", "Run threat models on new services; review secure design and code; triage and respond to security incidents; tune SAST/DAST tooling; train engineers on secure coding patterns", "Critical Vuln Time-to-Patch, AppSec Review Coverage, Phishing Test Pass Rate, Incident Severity Rate, Training Completion", 24),

        // ── QA ───────────────────────────────────────────────────────────────────
        new("QAL-001", "QA Lead", 4, 32_000m, 50_000m, "ENGINEERING", "EM-001", true, "Bachelor's Degree", 7, "Test Strategy, Automation, Performance Testing, Mobile Testing, Coaching, Release Management", "Leads QA strategy, automation, and release readiness across {Company}'s product surfaces", "M3", "Define QA strategy and standards; lead QA engineers; own release readiness sign-off; build automation frameworks; partner with eng managers on quality KPIs", "Bug Escape Rate, Release Readiness On-Time Rate, Automation Coverage Growth, QA Engineer Engagement Score", 27),
        new("QAS-001", "Senior QA Engineer", 3, 20_000m, 30_000m, "ENGINEERING", "QAL-001", false, "Bachelor's Degree", 4, "Test Automation, Selenium / Playwright, API Testing, Performance Testing, SQL, Mobile Testing", "Senior QA at {Company} owning automation, exploratory testing, and quality coaching for a product squad", "L5", "Design and maintain automated test suites; lead exploratory testing; coach engineers on testing practices; own release sign-off for assigned squad; triage production bugs", "Automation Suite Reliability, Escaped Bug Count, Test Coverage Growth, Release Sign-Off Timeliness", 24),
        new("QA-001", "QA Engineer", 2, 11_000m, 17_000m, "ENGINEERING", "QAS-001", false, "Bachelor's Degree", 2, "Manual Testing, Test Cases, Bug Reporting, API Testing, SQL, Test Automation Basics", "Mid-level QA at {Company} executing manual and automated testing across product features", "L3", "Write and execute test plans; report and triage bugs; maintain regression test suites; participate in release readiness; expand automation coverage", "Bug Detection Rate, Test Plan Execution On-Time, Regression Pass Rate, Documented Test Cases", 21),
        new("TAE-001", "Test Automation Engineer", 3, 18_000m, 28_000m, "ENGINEERING", "QAL-001", false, "Bachelor's in CS", 3, "Playwright / Selenium / Cypress, CI/CD, Programming, API Testing, Performance Testing", "Specialist focused on building and scaling {Company}'s automated test infrastructure across web, mobile, and API", "L4", "Build automation frameworks across web, mobile, and API; integrate test suites into CI/CD; tune flaky tests; coach engineers on writing maintainable tests", "Automation Suite Pass Rate, Flake Rate, CI Test Time, Engineer Test-Authoring Adoption", 24),

        // ── Product ─────────────────────────────────────────────────────────────
        new("PM-002", "Product Manager", 3, 20_000m, 30_000m, "PRODUCT", "PM-001", false, "Bachelor's Degree", 3, "Product Discovery, Roadmapping, Analytics, A/B Testing, SQL, Stakeholder Management", "Owns a focused {Company} product area, partnering with eng, design, and data to ship and measure outcomes", "L4", "Run discovery for assigned product area; maintain roadmap and backlog; partner with eng and design on delivery; analyse product metrics; ship and measure A/B tests", "Activation Rate, Feature Adoption, Experiment Velocity, Roadmap Hit Rate, Stakeholder NPS", 24),
        new("APM-001", "Associate Product Manager", 2, 11_000m, 17_000m, "PRODUCT", "PM-002", false, "Bachelor's Degree", 1, "Product Thinking, Analytics, Communication, Spreadsheets, Spec Writing, SQL Basics", "Early-career PM at {Company} owning small features and supporting senior PMs on discovery", "L3", "Write specs for small features; support senior PMs on discovery and research; own one product surface metric; run customer interviews; document product decisions", "Spec Quality, Feature Adoption for Owned Features, Customer Interview Volume, Decision Documentation Coverage", 21),

        // ── Design ──────────────────────────────────────────────────────────────
        new("PD-001", "Product Designer", 3, 18_000m, 28_000m, "DESIGN", "VPD-001", false, "Design Bachelor's", 3, "Figma, Interaction Design, Visual Design, Prototyping, Design Systems, Accessibility", "Designs {Company}'s product surfaces end-to-end from problem framing to high-fidelity delivery", "L4", "Design product surfaces from research through high-fidelity; contribute to the {Company} design system; run usability tests; partner with PM and engineering on delivery; ensure accessibility", "Usability Test Pass Rate, Design System Contribution, Time-to-Design, Accessibility Score, Engineer Handoff Quality", 24),
        new("UXR-001", "UX Researcher", 3, 18_000m, 28_000m, "DESIGN", "VPD-001", false, "Master's Degree", 3, "Qualitative Research, Quantitative Research, Survey Design, User Interviewing, Synthesis, Stakeholder Communication", "Designs and runs user research at {Company}, generating insight that drives product and design decisions", "L4", "Plan and run qualitative and quantitative research studies; synthesise findings into shareable insights; coach designers and PMs on research; maintain a research repository; partner with data on mixed-methods studies", "Research Study Throughput, Insight Adoption, Stakeholder Satisfaction, Repository Coverage, Research Quality Rubric", 24),

        // ── People & Ops ────────────────────────────────────────────────────────
        new("PO-001", "People Operations Specialist", 2, 11_000m, 17_000m, "PEOPLE", null, false, "Bachelor's Degree", 2, "HR Operations, Onboarding, HRIS, Labour Law, Payroll Coordination, Employee Experience", "Runs day-to-day people operations at {Company} from onboarding through offboarding", "L3", "Onboard new hires end-to-end; maintain HRIS and employee records; coordinate payroll inputs; manage benefits and leave; field employee questions on policy", "Onboarding NPS, HRIS Data Accuracy, Payroll Cycle Accuracy, Policy Query SLA, Offboarding Completion Rate", 21),
        new("OM-001", "Office Manager", 2, 9_000m, 15_000m, "OPS", null, false, "Bachelor's Degree", 3, "Office Operations, Vendor Management, Facilities, Travel Coordination, Procurement, Event Logistics", "Runs day-to-day operations of {Company}'s offices and workplace experience", "L3", "Run office operations and vendor relationships; coordinate travel and visitor logistics; organise team events; manage procurement and supplies; maintain a great workplace experience", "Office NPS, Vendor SLA Compliance, Procurement Cycle Time, Event Execution Score", 21),

        // ── Go-To-Market ────────────────────────────────────────────────────────
        new("SEN-001", "Sales Engineer", 3, 22_000m, 33_000m, "GO-TO-MARKET", "COO-001", false, "Bachelor's in CS", 3, "Solution Engineering, APIs, SQL, Demos, Customer Communication, Technical Writing", "Partners with {Company}'s sales team on technical demos, integration scoping, and customer evaluations", "L4", "Run technical demos and proofs-of-concept; scope customer integrations; respond to RFPs and security questionnaires; partner with product on customer feedback; train new sales reps on the {Company} platform", "POC Win Rate, Time-to-First-Integration, RFP Win Rate, Sales Team Satisfaction, Customer Technical NPS", 24),
        new("CSL-001", "Customer Success Lead", 4, 32_000m, 52_000m, "GO-TO-MARKET", "COO-001", true, "Bachelor's Degree", 7, "Customer Success Strategy, Account Management, Renewal Forecasting, Coaching, Cross-Functional Leadership", "Leads {Company}'s customer success team, owning customer health, retention, and expansion outcomes", "M3", "Lead the CS team; own renewal and expansion targets; partner with product on customer feedback loops; design CS playbooks; manage executive customer relationships", "Net Revenue Retention, Gross Retention, CS Team Productivity, NPS, Expansion ARR", 27),
        new("CSM-001", "Customer Success Manager", 3, 18_000m, 28_000m, "GO-TO-MARKET", "CSL-001", false, "Bachelor's Degree", 3, "Account Management, Onboarding, Customer Communication, Renewals, Product Knowledge, Empathy", "Owns a portfolio of {Company} customers, driving onboarding, adoption, retention, and expansion", "L4", "Onboard new customers; run quarterly business reviews; track adoption and risk signals; manage renewal and upsell motions; surface customer feedback to product", "Net Revenue Retention, QBR Completion Rate, Adoption Score, Time-to-Value, Customer NPS", 24),
        new("DRE-001", "Developer Relations Engineer", 3, 22_000m, 33_000m, "GO-TO-MARKET", "COO-001", false, "Bachelor's in CS", 4, "Public Speaking, Sample Apps, Technical Writing, Community Building, APIs, SDKs", "Builds and grows {Company}'s developer community through content, sample apps, events, and documentation", "L4", "Write and ship sample apps and tutorials; speak at meetups and conferences; maintain SDKs and developer documentation; run community events; gather and prioritise developer feedback", "API Active Developers, Tutorial Completion Rate, SDK Adoption, Community Event NPS, Developer NPS", 24),
        new("SA-001", "Solutions Architect", 4, 35_000m, 55_000m, "GO-TO-MARKET", "COO-001", false, "Bachelor's in CS", 8, "Enterprise Architecture, Integration Patterns, APIs, Cloud, Customer Workshops, Technical Strategy", "Senior technical advisor partnering with {Company}'s largest customers on integration strategy and architecture", "L7", "Architect end-to-end customer integrations; lead solution design workshops; partner with sales on enterprise deals; influence the {Company} product roadmap with customer needs; mentor implementation engineers", "Enterprise Deal Win Rate, Customer Architecture Health, Time-to-Production for Enterprise, Roadmap Inputs Adopted", 27),
        new("IE-001", "Implementation Engineer", 3, 18_000m, 28_000m, "GO-TO-MARKET", "SA-001", false, "Bachelor's in CS", 3, "REST/GraphQL APIs, SQL, Scripting, Customer Communication, Project Management, Debugging", "Implements {Company} integrations into customer environments and supports go-live activities", "L4", "Implement customer integrations end-to-end; troubleshoot integration issues; coordinate go-live; document customer-specific configurations; partner with CS on hand-off", "Time-to-Integration, Go-Live On-Time Rate, Customer Integration Health, Documentation Completeness", 24),
        new("TW-001", "Technical Writer", 3, 16_000m, 26_000m, "GO-TO-MARKET", "COO-001", false, "Bachelor's Degree", 3, "Technical Writing, API Documentation, Developer Experience, Markdown, Information Architecture, Editing", "Writes and maintains {Company}'s product, API, and developer documentation", "L4", "Author API and product documentation; maintain information architecture; partner with engineers on docs-as-code workflow; run docs reviews; track documentation health", "Doc Coverage, Doc Freshness, Developer Satisfaction with Docs, Search-to-Resolution Rate", 24),

        // ── Internal Ops & Support ──────────────────────────────────────────────
        new("IT-001", "IT Support Engineer", 2, 9_000m, 15_000m, "OPS", "OM-001", false, "Bachelor's Degree", 2, "Endpoint Management, Identity (SSO/MDM), Networking, Helpdesk, Hardware, Customer Service", "Provides IT support for {Company} employees across endpoints, identity, and workplace tooling", "L3", "Triage and resolve IT helpdesk tickets; manage device onboarding and offboarding; administer SSO and MDM; maintain office network; partner with security on endpoint posture", "Ticket Resolution SLA, Onboarding On-Time Rate, Endpoint Compliance Rate, Employee NPS", 21),
        new("FM-001", "Finance Manager", 4, 35_000m, 55_000m, "FINANCE", "CFO-001", true, "Bachelor's / ACCA", 7, "Financial Planning, Accounting, Tax, Treasury, IFRS Reporting, Audit, Stakeholder Management", "Leads {Company}'s accounting, financial reporting, and finance operations team", "M3", "Run monthly and annual close; manage payables, receivables, and treasury; coordinate audit; produce IFRS financial statements; manage finance and accounting team", "Close Cycle Time, Audit Adjustments, Cash Forecast Accuracy, Finance Team Engagement", 27),
        new("AC-001", "Accountant", 2, 11_000m, 17_000m, "FINANCE", "FM-001", false, "Bachelor's / Part ACCA", 2, "Bookkeeping, IFRS, Reconciliation, Payables, Receivables, Tax Filing, Excel", "Maintains {Company}'s ledgers, reconciliations, and statutory filings", "L3", "Post journals and reconcile accounts; manage AP and AR cycles; prepare VAT, PAYE, and SSNIT filings; support monthly close; respond to audit requests", "Close On-Time Rate, Reconciliation Accuracy, Statutory Filing Timeliness, Audit Findings", 21),
        new("ER-001", "People Operations Lead", 3, 20_000m, 30_000m, "PEOPLE", null, false, "Bachelor's Degree", 4, "Employee Relations, Performance Management, HRIS, Labour Law, Coaching, Policy Design", "Senior people-ops generalist designing {Company}'s HR programmes, policies, and employee experience", "L4", "Design and run performance review cycles; manage employee relations cases; maintain HR policies; coach managers on people issues; own engagement surveys", "Engagement Score, Performance Cycle Completion, Policy Compliance, Manager Coaching NPS", 24)
    ];

    // Curated software-company station catalogue. Codes follow type-prefix conventions
    // (HQ, EC = Engineering Centre, COH = Customer Operations Hub, CWH = Co-working Hub).
    // Phone numbers and addresses use real Ghana streets and area codes; emails store ONLY
    // the local part so the row factory appends the actual company TLD at runtime.
    private static readonly StationSpec[] _softwareStations =
    [
        new("HQ-001", "Head Office - Airport City Accra", "Head Office", "Greater Accra", "Accra", "Octagon, Independence Avenue, Airport City", 100, 600, "{Company} corporate headquarters housing executive, product, GTM, finance, and people functions", "+233 30 222 1000", "headoffice"),
        new("EC-EL-001", "Engineering Centre - East Legon", "Engineering Centre", "Greater Accra", "Accra", "23 Boundary Road, East Legon", 60, 250, "{Company} primary engineering campus hosting platform, mobile, and product engineering squads", "+233 30 222 1010", "engineering.eastlegon"),
        new("EC-AC-001", "Engineering Centre - Airport City", "Engineering Centre", "Greater Accra", "Accra", "Atlantic Tower, Airport City", 60, 220, "{Company} engineering centre co-located with HQ for tight product, design, and engineering collaboration", "+233 30 222 1011", "engineering.airportcity"),
        new("EC-CT-001", "Engineering Centre - Cantonments", "Engineering Centre", "Greater Accra", "Accra", "14 Switchback Road, Cantonments", 50, 180, "{Company} engineering centre focused on data, infrastructure, and security engineering teams", "+233 30 222 1012", "engineering.cantonments"),
        new("EC-KSI-001", "Engineering Centre - Kumasi", "Engineering Centre", "Ashanti", "Kumasi", "Prempeh II Street, Adum", 30, 120, "{Company} regional engineering centre tapping into the Ashanti tech talent pool from KNUST and Kumasi", "+233 32 202 1010", "engineering.kumasi"),
        new("EC-TKD-001", "Engineering Centre - Takoradi", "Engineering Centre", "Western", "Takoradi", "Harbour Road, Market Circle", 20, 80, "{Company} engineering centre serving the Western Region developer community and oil-and-gas integrations", "+233 31 202 1010", "engineering.takoradi"),
        new("COH-OSU-001", "Customer Operations Hub - Osu", "Customer Operations Hub", "Greater Accra", "Accra", "Oxford Street, Osu", 30, 120, "{Company} customer operations and support hub serving Greater Accra customers and merchants", "+233 30 222 1100", "support.osu"),
        new("COH-SPX-001", "Customer Operations Hub - Spintex", "Customer Operations Hub", "Greater Accra", "Accra", "Spintex Road, Accra", 25, 100, "{Company} merchant onboarding and customer success hub for the Spintex industrial corridor", "+233 30 222 1101", "support.spintex"),
        new("COH-TEM-001", "Customer Operations Hub - Tema", "Customer Operations Hub", "Greater Accra", "Tema", "Community 1, Tema Central", 25, 100, "{Company} customer operations hub serving Tema port operators, industrialists, and merchants", "+233 30 322 1100", "support.tema"),
        new("COH-KSI-001", "Customer Operations Hub - Kumasi", "Customer Operations Hub", "Ashanti", "Kumasi", "Asokwa Industrial Area, Kumasi", 20, 90, "{Company} customer operations hub serving Ashanti merchants and corporate clients", "+233 32 202 1100", "support.kumasi"),
        new("COH-TKD-001", "Customer Operations Hub - Takoradi", "Customer Operations Hub", "Western", "Takoradi", "Harbour Road, Takoradi", 15, 70, "{Company} customer operations hub serving Western Region merchants and oil-and-gas customers", "+233 31 202 1100", "support.takoradi"),
        new("COH-TAM-001", "Customer Operations Hub - Tamale", "Customer Operations Hub", "Northern", "Tamale", "Salaga Road, Central Tamale", 12, 50, "{Company} customer operations hub serving Northern Region merchants, agribusiness, and government clients", "+233 37 202 1100", "support.tamale"),
        new("COH-CAP-001", "Customer Operations Hub - Cape Coast", "Customer Operations Hub", "Central", "Cape Coast", "Commercial Street, Cape Coast", 10, 40, "{Company} customer operations hub serving Central Region merchants and the university community", "+233 33 202 1100", "support.capecoast"),
        new("COH-HO-001", "Customer Operations Hub - Ho", "Customer Operations Hub", "Volta", "Ho", "Ho-Aflao Road, Ho Central", 10, 40, "{Company} customer operations hub serving Volta and Oti Region merchants and customers", "+233 36 202 1100", "support.ho"),
        new("COH-KOF-001", "Customer Operations Hub - Koforidua", "Customer Operations Hub", "Eastern", "Koforidua", "Hospital Road, Koforidua", 10, 40, "{Company} customer operations hub serving Eastern Region merchants and corporate clients", "+233 34 202 1100", "support.koforidua"),
        new("CWH-SUN-001", "Co-working Hub - Sunyani", "Co-working Hub", "Bono", "Sunyani", "Fiapre Road, Sunyani Central", 5, 25, "{Company} co-working satellite for engineers and customer-facing staff in the Bono Region", "+233 35 202 1200", "coworking.sunyani"),
        new("CWH-BOL-001", "Co-working Hub - Bolgatanga", "Co-working Hub", "Upper East", "Bolgatanga", "Zuarungu Road, Bolgatanga", 5, 20, "{Company} co-working satellite for Upper East field staff and remote engineers", "+233 37 202 1200", "coworking.bolgatanga"),
        new("CWH-WA-001", "Co-working Hub - Wa", "Co-working Hub", "Upper West", "Wa", "Wa Main Road, Wa Central", 5, 20, "{Company} co-working satellite for Upper West field staff and customer success team", "+233 39 202 1200", "coworking.wa"),
        new("CWH-HOH-001", "Co-working Hub - Hohoe", "Co-working Hub", "Volta", "Hohoe", "Hohoe Market Road, Hohoe", 5, 20, "{Company} co-working satellite for Volta Region field operations and customer success", "+233 36 202 1200", "coworking.hohoe"),
        new("CWH-TCH-001", "Co-working Hub - Techiman", "Co-working Hub", "Bono East", "Techiman", "Techiman Market Road, Techiman", 5, 20, "{Company} co-working satellite serving the Bono East merchant community", "+233 35 209 1200", "coworking.techiman"),
        new("CWH-OBU-001", "Co-working Hub - Obuasi", "Co-working Hub", "Ashanti", "Obuasi", "Main Street, Obuasi", 5, 20, "{Company} co-working satellite serving Obuasi mining community customers and field staff", "+233 32 209 1200", "coworking.obuasi"),
        new("CWH-NKW-001", "Co-working Hub - Nkawkaw", "Co-working Hub", "Eastern", "Nkawkaw", "Accra-Kumasi Highway, Nkawkaw", 5, 20, "{Company} co-working satellite for the Accra-Kumasi corridor field team", "+233 34 202 1200", "coworking.nkawkaw"),
        new("CWH-KAS-001", "Co-working Hub - Kasoa", "Co-working Hub", "Central", "Kasoa", "Kasoa Market Road, Kasoa", 5, 20, "{Company} co-working satellite serving the fast-growing Kasoa commercial corridor", "+233 33 202 1200", "coworking.kasoa"),
        new("CWH-ASH-001", "Co-working Hub - Ashaiman", "Co-working Hub", "Greater Accra", "Ashaiman", "Ashaiman Market Road, Ashaiman", 5, 20, "{Company} co-working satellite serving Ashaiman merchants and customer field operations", "+233 30 322 1200", "coworking.ashaiman"),
        new("CWH-MAD-001", "Co-working Hub - Madina", "Co-working Hub", "Greater Accra", "Accra", "Madina Market Road, Madina", 5, 20, "{Company} co-working satellite for engineers and customer success staff in northern Accra", "+233 30 222 1200", "coworking.madina")
    ];
}
