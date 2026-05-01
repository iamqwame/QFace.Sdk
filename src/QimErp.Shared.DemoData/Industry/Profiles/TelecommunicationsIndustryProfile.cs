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
        // Corporate tier uses the curated _telcoStations catalogue verbatim — full
        // names, addresses, codes, descriptions, phones, emails, station types covering
        // a tier-1 Ghana telco footprint (HQ, regional offices, switching/data centres,
        // service centres, customer care centres, dealer outlets, field operations hubs).
        if (tier == CompanyTier.Corporate)
        {
            var hqRow = _telcoStations[0];
            var rest = _telcoStations.Skip(1).ToList();
            var branchTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Regional Office", "Switching Centre", "Data Centre", "MTSO", "Service Centre"
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

    // Job-title bundles by org-unit. {Company} placeholder substituted at row-emit time.
    private static readonly IReadOnlyList<string> ExecJobs        = ["CEO", "COO", "CTO", "CMO", "CCO", "CXO_TELCO", "CFO_TELCO", "CHRO_TELCO", "CIO"];
    private static readonly IReadOnlyList<string> NetworksJobs    = ["VP_NET", "HEAD_RAN", "HEAD_CORE", "HEAD_TX", "HEAD_SVC_OPS", "HEAD_FIELD_OPS", "HEAD_SPECTRUM", "HEAD_CAPACITY", "HEAD_VENDOR", "SOL_ARCH", "SR_NET_ENG", "NET_ENG_RAN", "NET_ENG_CORE", "NET_ENG_TX", "NOC_ENG", "FIELD_ENG", "SITE_ENG", "TOWER_CLIMBER", "OFC_ENG", "IPMPLS_ENG", "OSS_ENG_T", "RAN_TRAINEE"];
    private static readonly IReadOnlyList<string> ItJobs          = ["VP_IT", "HEAD_IT", "HEAD_CYBER", "BSS_ENG_T", "CYBER_ANALYST", "IT_SUPPORT_T", "IT_INTERN"];
    private static readonly IReadOnlyList<string> CommercialJobs  = ["VP_SALES", "VP_MKT", "B2B_SALES_MGR", "ENT_ACCT_MGR", "SME_ACCT_MGR", "RETAIL_MGR", "RETAIL_SUP", "SALES_AGENT", "FIELD_SALES_REP", "DISTRIBUTOR_MGR", "TRADE_MKT_OFFICER", "COMM_ANALYST", "PRICING_ANALYST", "SR_PM_POSTPAID", "SR_PM_PREPAID", "SR_PM_DATA", "SR_PM_ENT", "SR_PM_ROAMING", "SR_PM_DEVICES", "BRAND_MGR", "PERF_MKT_MGR"];
    private static readonly IReadOnlyList<string> CxJobs          = ["VP_CX", "HEAD_CX", "CC_TEAM_LEAD", "CC_AGENT_VOICE", "CC_AGENT_CHAT", "CC_AGENT_SOCIAL", "BACKOFFICE_OFFICER", "RETENTION_OFFICER_T"];
    private static readonly IReadOnlyList<string> MoMoJobs        = ["MM_MGR", "MM_OPS", "FRAUD_ANALYST"];
    private static readonly IReadOnlyList<string> FinanceJobs     = ["FIN_MGR", "FIN_ANALYST", "RISK_COMP_OFFICER"];
    private static readonly IReadOnlyList<string> HrJobs          = ["HR_OFFICER", "TALENT_MGR", "LD_SPECIALIST"];
    private static readonly IReadOnlyList<string> RegulatoryJobs  = ["REG_AFFAIRS_MGR"];
    private static readonly IReadOnlyList<string> ProcurementJobs = ["PROC_MGR", "LOG_OFFICER"];
    private static readonly IReadOnlyList<string> ProgramsJobs    = ["HEAD_FIELD_OPS", "HEAD_SVC_OPS"];
    private static readonly IReadOnlyList<string> AdminJobs       = ["HR_OFFICER", "PROC_MGR"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",  "Founder/CEO",       null,      OrgUnitKind.Executive, ExecJobs),
        new("NETOPS",   "Network Operations","FOUNDER", OrgUnitKind.Function,  NetworksJobs),
        new("CUSTOMER", "Customer Service",  "FOUNDER", OrgUnitKind.Function,  CxJobs)
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
        new("NETOPS",   "Network Operations","EXEC", OrgUnitKind.Function,  NetworksJobs),
        new("CUSTOMER", "Customer Service",  "EXEC", OrgUnitKind.Function,  CxJobs),
        new("IT",       "IT",                "EXEC", OrgUnitKind.Function,  ItJobs),
        new("SALES",    "Sales & Marketing", "EXEC", OrgUnitKind.Function,  CommercialJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.05,
        ["NETOPS"]   = 0.30,
        ["CUSTOMER"] = 0.35,
        ["IT"]       = 0.15,
        ["SALES"]    = 0.15
    };

    // Corporate-tier baseline OrgUnits — each carries rich Description / Budget /
    // CostCenter / Purpose / Phone / Email-local-part suitable for a tier-1 Ghana telco
    // (NCA-licensed Mobile Network Operator scale: MTN Ghana, Telecel Ghana, AirtelTigo).
    // The {Company} placeholder gets substituted with the actual tenant's company name at
    // row-emit time so the same catalogue reads naturally for any telco tenant.
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC", "Executive", null, OrgUnitKind.Executive, ExecJobs,
            Description: "Office of the CEO and executive leadership of {Company} — sets corporate strategy and engages NCA, NITA, MoCD, and shareholders",
            BudgetMin: 1_500_000m, BudgetMax: 4_000_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Set and execute {Company} group strategy; chair the executive committee; manage NCA spectrum and licensing relationships; drive shareholder value",
            Phone: "+233 30 730 0000", Email: "executive"),
        new("NETWORKS", "Networks & Engineering", "EXEC", OrgUnitKind.Function, NetworksJobs,
            Description: "Operates {Company}'s radio access, core, and transmission networks across Ghana — owns availability, quality, and capacity end-to-end",
            BudgetMin: 8_000_000m, BudgetMax: 25_000_000m,
            CostCenter: "CC-NET-001",
            Purpose: "Sustain best-in-class network availability, drop-call rate, and data throughput at {Company}; deliver 4G/5G coverage roadmap and capex efficiency",
            Phone: "+233 30 730 0100", Email: "networks"),
        new("IT", "Information Technology", "EXEC", OrgUnitKind.Function, ItJobs,
            Description: "Runs {Company}'s BSS, OSS, billing, CRM, charging, data centres, and enterprise IT estate — the digital plumbing behind every product",
            BudgetMin: 4_000_000m, BudgetMax: 12_000_000m,
            CostCenter: "CC-IT-001",
            Purpose: "Operate {Company} IT and BSS/OSS platforms with high availability; deliver digital transformation, billing accuracy, and integration capability",
            Phone: "+233 30 730 0200", Email: "it"),
        new("CYBERSECURITY", "Cybersecurity", "EXEC", OrgUnitKind.Function, ItJobs,
            Description: "Protects {Company}'s subscriber data, signalling networks, and digital channels — SOC, threat intel, fraud, and BCP",
            BudgetMin: 1_500_000m, BudgetMax: 4_000_000m,
            CostCenter: "CC-CYBER-001",
            Purpose: "Defend {Company} infrastructure, customer data, and Mobile Money rails against cyber threats, SIM fraud, and signalling abuse",
            Phone: "+233 30 730 0250", Email: "cybersecurity"),
        new("COMMERCIAL", "Commercial", "EXEC", OrgUnitKind.Function, CommercialJobs,
            Description: "Owns the {Company} top line — consumer, enterprise (B2B), SME, devices, roaming, and product lifecycle across postpaid, prepaid, and data",
            BudgetMin: 3_000_000m, BudgetMax: 8_000_000m,
            CostCenter: "CC-COMM-001",
            Purpose: "Grow {Company} subscriber base, ARPU, and data monetisation across consumer and enterprise segments; own pricing and product P&L",
            Phone: "+233 30 730 0300", Email: "commercial"),
        new("MARKETING", "Marketing & Brand", "EXEC", OrgUnitKind.Function, CommercialJobs,
            Description: "Manages the {Company} brand, above-the-line and below-the-line campaigns, sponsorships, digital marketing, and trade marketing nationwide",
            BudgetMin: 2_000_000m, BudgetMax: 6_000_000m,
            CostCenter: "CC-MKT-001",
            Purpose: "Build and protect the {Company} brand; drive acquisition, awareness, and retention through high-ROI campaigns and trade activation",
            Phone: "+233 30 730 0400", Email: "marketing"),
        new("CUSTOMER-EXPERIENCE", "Customer Experience", "EXEC", OrgUnitKind.Function, CxJobs,
            Description: "Runs {Company} contact centres, service centres, social care, and back-office care operations — the voice of the customer end-to-end",
            BudgetMin: 1_500_000m, BudgetMax: 5_000_000m,
            CostCenter: "CC-CX-001",
            Purpose: "Deliver best-in-class NPS and first-call resolution across all {Company} customer touchpoints — voice, chat, social, retail, and back-office",
            Phone: "+233 30 730 0500", Email: "customercare"),
        new("MOBILE-MONEY", "Mobile Money", "EXEC", OrgUnitKind.Function, MoMoJobs,
            Description: "Operates {Company}'s mobile money business — wallet, merchant payments, agent network, remittances, and BoG-licensed payments service",
            BudgetMin: 2_000_000m, BudgetMax: 6_000_000m,
            CostCenter: "CC-MM-001",
            Purpose: "Grow {Company} Mobile Money active wallets, transaction volume, and float; maintain BoG PSP compliance and AML/CFT integrity",
            Phone: "+233 30 730 0600", Email: "mobilemoney"),
        new("FINANCE", "Finance", "EXEC", OrgUnitKind.Function, FinanceJobs,
            Description: "Owns {Company}'s financial reporting, capex governance, treasury, tax, and management accounting — incl. NCA levy and CST compliance",
            BudgetMin: 1_000_000m, BudgetMax: 3_000_000m,
            CostCenter: "CC-FIN-001",
            Purpose: "Produce accurate {Company} financial statements; manage capex and opex governance; ensure tax, GRA, and NCA levy compliance",
            Phone: "+233 30 730 0700", Email: "finance"),
        new("HR", "Human Resources", "EXEC", OrgUnitKind.Function, HrJobs,
            Description: "Talent acquisition, learning and development, total rewards, and employee relations — the {Company} people function",
            BudgetMin: 800_000m, BudgetMax: 2_000_000m,
            CostCenter: "CC-HR-001",
            Purpose: "Attract, develop, and retain the engineering, commercial, and digital talent {Company} needs; champion the {Company} culture",
            Phone: "+233 30 730 0800", Email: "hr"),
        new("REGULATORY-AFFAIRS", "Regulatory Affairs", "EXEC", OrgUnitKind.Function, RegulatoryJobs,
            Description: "Manages {Company}'s relationships with the NCA, NITA, MoCD, DPC, and parliament; spectrum, licensing, QoS, and policy advocacy",
            BudgetMin: 600_000m, BudgetMax: 1_500_000m,
            CostCenter: "CC-REG-001",
            Purpose: "Secure and protect {Company} licences, spectrum, and regulatory permissions; ensure QoS compliance and represent {Company} in policy fora",
            Phone: "+233 30 730 0900", Email: "regulatory"),
        new("PROCUREMENT", "Procurement & Supply Chain", "EXEC", OrgUnitKind.Function, ProcurementJobs,
            Description: "Sources network gear, IT, devices, marketing, and indirect spend; manages vendor master, logistics, and warehousing for {Company}",
            BudgetMin: 700_000m, BudgetMax: 1_500_000m,
            CostCenter: "CC-PROC-001",
            Purpose: "Deliver value, integrity, and continuity in {Company} sourcing; manage strategic OEM relationships (Ericsson, Nokia, Huawei, ZTE) and 3PLs",
            Phone: "+233 30 730 1000", Email: "procurement")
    ];

    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]                = 0.03,
        ["NETWORKS"]            = 0.22,
        ["IT"]                  = 0.10,
        ["CYBERSECURITY"]       = 0.03,
        ["COMMERCIAL"]          = 0.14,
        ["MARKETING"]           = 0.05,
        ["CUSTOMER-EXPERIENCE"] = 0.22,
        ["MOBILE-MONEY"]        = 0.10,
        ["FINANCE"]             = 0.04,
        ["HR"]                  = 0.03,
        ["REGULATORY-AFFAIRS"]  = 0.02,
        ["PROCUREMENT"]         = 0.02
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("NETOPS",   "Network Operations","EXEC", OrgUnitKind.Function,  NetworksJobs),
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

    // ── Job titles ─────────────────────────────────────────────────────────────
    // ~55 enriched roles for a Ghana tier-1 MNO. Pay grades T-1 (entry) → T-10 (CEO).
    // Salaries follow the rank → band mapping in the spec (rank 1: 2k–5k … rank 5: 48k–110k).
    // Rank 5 = C-Suite, Rank 4 = VPs / Heads, Rank 3 = Managers / Senior Engineers,
    // Rank 2 = Engineers / Officers / Agents, Rank 1 = Trainees / Interns.
    // {Company} placeholder is substituted with the actual tenant's company name at row-emit.
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // ── Rank 5 — C-Suite (10 roles) ───────────────────────────────────────
        new("CEO", "Chief Executive Officer", 5, 65_000m, 110_000m, "EXEC", null, true, "Master's Degree", 18, "Telco Strategy, P&L Leadership, NCA Relations, Investor Relations, M&A",
            "Group CEO accountable for {Company}'s overall strategy, performance, and stakeholder relations across Ghana", "T-10",
            "Set and execute {Company} strategy; lead executive committee; manage NCA and government relations; drive subscriber growth, ARPU, and EBITDA",
            "Service Revenue, EBITDA Margin, Subscriber Market Share, Data ARPU, Net Promoter Score", 30),
        new("COO", "Chief Operating Officer", 5, 55_000m, 95_000m, "EXEC", "CEO", true, "Master's Degree", 15, "Telco Operations, Network Strategy, Customer Operations, Process Excellence",
            "Executive overseeing day-to-day {Company} operations across networks, customer experience, and field services", "T-9",
            "Drive operational excellence across {Company}; lead network and CX integration; chair operations committee; manage opex efficiency programmes",
            "Network Availability, NPS, Cost-to-Serve, Capex/Opex Variance, Operational SLA Compliance", 30),
        new("CTO", "Chief Technology Officer", 5, 55_000m, 95_000m, "NETWORKS", "CEO", true, "Master's Degree", 15, "Network Architecture, 4G/5G Strategy, Vendor Management, Spectrum Strategy",
            "Executive leading {Company} network technology strategy, architecture, and the 4G/5G evolution roadmap", "T-9",
            "Define {Company} technology roadmap; oversee RAN, core, transmission, and IT architecture; manage OEM relationships; lead spectrum strategy",
            "Network Availability %, Drop Call Rate, Data Throughput, 4G/5G Coverage, Capex Efficiency", 30),
        new("CMO", "Chief Marketing Officer", 5, 50_000m, 90_000m, "MARKETING", "CEO", true, "Master's Degree", 14, "Brand Management, Digital Marketing, Consumer Insight, Sponsorships, Campaign ROI",
            "Executive responsible for {Company} brand, marketing, communications, and consumer engagement across Ghana", "T-9",
            "Own {Company} brand health; lead ATL/BTL marketing; drive acquisition campaigns; manage sponsorships; oversee performance and trade marketing",
            "Brand Health Index, Customer Acquisition Cost, Campaign ROI, Share of Voice, Top-of-Mind Awareness", 30),
        new("CCO", "Chief Commercial Officer", 5, 55_000m, 100_000m, "COMMERCIAL", "CEO", true, "Master's Degree", 15, "Commercial Strategy, Revenue Management, Sales Leadership, Pricing, Channel Strategy",
            "Executive owning {Company}'s top line — consumer, enterprise, SME, devices, and product P&L", "T-9",
            "Lead {Company} commercial strategy; drive sales across consumer and enterprise; manage pricing and product portfolio; oversee distribution and trade",
            "Service Revenue, Gross Adds, ARPU, Enterprise Revenue, Product Margin, Market Share", 30),
        new("CXO_TELCO", "Chief Customer Officer", 5, 48_000m, 85_000m, "CUSTOMER-EXPERIENCE", "CEO", true, "Master's Degree", 14, "Customer Experience Strategy, NPS, Service Design, Contact Centre Operations, Journey Mapping",
            "Executive accountable for end-to-end {Company} customer experience across all channels — voice, digital, retail, social", "T-9",
            "Set {Company} CX strategy; own NPS and CSAT; lead contact centres, retail care, and complaints handling; drive journey-led service design",
            "Net Promoter Score, First Call Resolution, CSAT, Complaint Resolution Time, Churn Rate", 30),
        new("CFO_TELCO", "Chief Finance Officer", 5, 55_000m, 100_000m, "FINANCE", "CEO", true, "Master's Degree / ACCA / ICAG", 15, "Financial Strategy, Capex Governance, Treasury, Tax, IFRS, NCA Levy Compliance",
            "Executive responsible for {Company} financial strategy, reporting, capex governance, and treasury", "T-9",
            "Oversee {Company} financial reporting; manage capex and opex governance; lead treasury and tax; ensure NCA levy and GRA compliance",
            "EBITDA Margin, Capex/Revenue, Free Cash Flow, Days Sales Outstanding, Audit Findings", 30),
        new("CHRO_TELCO", "Chief Human Resources Officer", 5, 48_000m, 85_000m, "HR", "CEO", true, "Master's Degree", 14, "HR Strategy, Talent Management, Organisational Design, Total Rewards, Labour Law",
            "Executive leading {Company} people strategy, talent management, and organisational development", "T-9",
            "Drive {Company} HR strategy; lead talent acquisition and retention; manage total rewards; champion culture; oversee Academy and L&D",
            "Staff Turnover, Engagement Score, Time-to-Hire, Succession Coverage, Training ROI", 30),
        new("CIO", "Chief Information Officer", 5, 50_000m, 90_000m, "IT", "CEO", true, "Master's Degree", 15, "IT Strategy, BSS/OSS Architecture, Digital Transformation, Cybersecurity, Cloud",
            "Executive leading {Company} IT strategy, BSS/OSS, digital channels, and enterprise IT", "T-9",
            "Define {Company} IT and digital roadmap; oversee BSS/OSS, billing, CRM, and digital channels; manage IT capex and vendor partnerships",
            "BSS/OSS Uptime, Billing Accuracy, Digital Channel Adoption, IT Project Delivery, Cyber Incident Rate", 30),
        new("VP_NET", "Vice President, Networks", 5, 48_000m, 85_000m, "NETWORKS", "CTO", true, "Master's Degree", 13, "Network Operations, RAN/Core/Transport, NOC, Vendor Management",
            "Senior executive leading {Company} network operations, NOC, and field engineering across Ghana", "T-8",
            "Manage end-to-end {Company} network performance; oversee NOC, RAN, core, transmission, and field operations; drive availability and quality KPIs",
            "Network Availability, Drop Call Rate, Site Uptime, MTTR, Capex Delivery", 30),

        // ── Rank 4 — VPs & Heads (16 roles) ───────────────────────────────────
        new("VP_IT", "Vice President, IT", 4, 30_000m, 50_000m, "IT", "CIO", true, "Master's Degree", 12, "IT Operations, BSS/OSS, Service Management, ITIL, Vendor Management",
            "Senior leader running {Company} IT operations, BSS/OSS engineering, and IT service management", "T-7",
            "Run {Company} IT operations; manage BSS/OSS engineering; ensure billing accuracy and digital channel uptime; oversee IT service management",
            "BSS Uptime, Billing Accuracy, IT MTTR, Project Delivery, IT Cost per Subscriber", 27),
        new("VP_SALES", "Vice President, Sales", 4, 30_000m, 50_000m, "COMMERCIAL", "CCO", true, "Master's Degree", 12, "Sales Leadership, Distribution, Channel Strategy, Trade, Enterprise Sales",
            "Senior leader running {Company} sales across consumer, enterprise, SME, and indirect channels", "T-7",
            "Lead {Company} sales force; manage distribution and trade; drive enterprise and SME revenue; own retail and dealer network performance",
            "Service Revenue, Gross Adds, Distribution Reach, Enterprise Revenue, Channel Productivity", 27),
        new("VP_MKT", "Vice President, Marketing", 4, 28_000m, 48_000m, "MARKETING", "CMO", true, "Master's Degree", 12, "Marketing Leadership, Brand Strategy, Digital Marketing, Performance Marketing",
            "Senior leader running {Company} marketing operations, brand campaigns, and performance marketing", "T-7",
            "Lead {Company} marketing strategy execution; oversee brand, digital, and performance marketing teams; manage agency and media partners",
            "Brand Health, CPA, Campaign ROI, Digital Conversions, Media Spend Efficiency", 27),
        new("VP_CX", "Vice President, Customer Experience", 4, 28_000m, 48_000m, "CUSTOMER-EXPERIENCE", "CXO_TELCO", true, "Master's Degree", 12, "CX Operations, Contact Centre Leadership, Journey Design, NPS Programmes",
            "Senior leader running {Company} contact centres, retail care, and customer experience programmes", "T-7",
            "Run {Company} contact centre operations; lead retail and back-office care; drive NPS and FCR programmes; manage workforce and quality",
            "NPS, FCR, AHT, CSAT, Service Level (80/20), Care Cost per Call", 27),
        new("HEAD_RAN", "Head of Radio Access", 4, 26_000m, 45_000m, "NETWORKS", "VP_NET", true, "Bachelor's in EE", 10, "RAN Engineering, 4G/5G, Spectrum, RF Optimisation, Drive Tests",
            "Leads {Company}'s radio access network — 2G/3G/4G/5G — across coverage, capacity, and quality", "T-7",
            "Own {Company} RAN performance; lead RF planning and optimisation; manage RAN capex; coordinate with vendors on rollout and modernisation",
            "Coverage %, Drop Call Rate, RAN Availability, Throughput, RAN Capex Efficiency", 27),
        new("HEAD_CORE", "Head of Core Network", 4, 26_000m, 45_000m, "NETWORKS", "VP_NET", true, "Bachelor's in CS / EE", 10, "Core Network, EPC, IMS, HLR/HSS, Signalling, Voice & Data Core",
            "Leads {Company}'s mobile core network — packet core, IMS, voice, and signalling", "T-7",
            "Own {Company} core network architecture and operations; manage EPC, IMS, HLR/HSS; coordinate VoLTE and 5G core evolution",
            "Core Availability, Signalling Success Rate, VoLTE Quality, MTTR, Capacity Headroom", 27),
        new("HEAD_TX", "Head of Transmission", 4, 24_000m, 42_000m, "NETWORKS", "VP_NET", true, "Bachelor's in EE", 10, "Transmission, Fibre, Microwave, DWDM, IP/MPLS, Site Engineering",
            "Leads {Company}'s transmission and fibre network — backhaul, metro, and long-haul", "T-7",
            "Own {Company} transmission strategy; manage backhaul, metro fibre, and microwave; lead DWDM and IP/MPLS rollout",
            "Transmission Availability, Fibre Cuts MTTR, Backhaul Capacity, Latency, Capex Efficiency", 27),
        new("HEAD_IT", "Head of IT", 4, 25_000m, 44_000m, "IT", "VP_IT", true, "Bachelor's Degree", 10, "BSS/OSS Operations, Service Management, ITIL, Cloud, Application Support",
            "Heads {Company} BSS/OSS engineering, application support, and core IT platforms", "T-7",
            "Run {Company} BSS/OSS platforms; manage billing, CRM, charging; coordinate IT vendors; ensure availability and integration",
            "BSS Uptime, Billing Accuracy, IT MTTR, Integration Delivery, Application SLA", 27),
        new("HEAD_CYBER", "Head of Cybersecurity", 4, 26_000m, 46_000m, "CYBERSECURITY", "CIO", true, "Master's Degree", 10, "Cybersecurity Strategy, SOC, Threat Intelligence, ISO 27001, Signalling Security",
            "Leads {Company}'s cybersecurity, SOC, threat intelligence, and signalling network defence", "T-7",
            "Run {Company} SOC; manage cyber strategy; defend signalling networks and Mobile Money rails; engage NCA on cyber compliance",
            "Mean Time to Detect, Mean Time to Respond, Critical Vuln Closure, Phishing Click Rate, Audit Findings", 27),
        new("HEAD_SVC_OPS", "Head of Service Operations", 4, 24_000m, 42_000m, "NETWORKS", "VP_NET", true, "Bachelor's Degree", 10, "NOC Management, Incident Management, Service Assurance, ITIL",
            "Leads {Company}'s NOC and service operations — 24x7 incident management and service assurance", "T-7",
            "Run {Company} NOC across 24x7 shifts; lead major incident management; drive MTTR reduction; own service assurance KPIs",
            "MTTR, Major Incident Count, NOC Service Level, Customer-Impacting Incidents, Ticket Closure Rate", 27),
        new("HEAD_FIELD_OPS", "Head of Field Operations", 4, 24_000m, 42_000m, "NETWORKS", "VP_NET", true, "Bachelor's Degree", 10, "Field Operations, Tower Maintenance, Fleet Management, Health & Safety, Vendor Management",
            "Leads {Company}'s field operations — tower maintenance, site access, fuel, and field engineering crews", "T-7",
            "Run {Company} field maintenance crews across all regions; manage towercos, fuel vendors, and site security; ensure HSE compliance",
            "Site Uptime, Field MTTR, Fuel Cost per Site, HSE Incidents, Vendor SLA Compliance", 27),
        new("HEAD_SPECTRUM", "Head of Spectrum & Frequency Planning", 4, 24_000m, 44_000m, "NETWORKS", "VP_NET", true, "Master's in EE", 10, "Spectrum Strategy, Frequency Planning, NCA Liaison, Refarming, ITU Standards",
            "Leads {Company}'s spectrum strategy, refarming, and NCA spectrum compliance", "T-7",
            "Define {Company} spectrum strategy; manage NCA spectrum applications and renewals; lead refarming initiatives; optimise spectral efficiency",
            "Spectrum Utilisation, Refarming Delivery, NCA Compliance, Spectrum Cost per MHz/POP", 27),
        new("HEAD_CAPACITY", "Head of Capacity Planning", 4, 24_000m, 40_000m, "NETWORKS", "VP_NET", true, "Bachelor's in EE / CS", 9, "Capacity Planning, Traffic Forecasting, Network Dimensioning, Analytics",
            "Leads {Company}'s capacity planning across RAN, core, and transmission — forecasts and dimensioning", "T-7",
            "Forecast {Company} traffic and capacity demand; dimension RAN, core, transmission; produce capacity plans and capex business cases",
            "Capacity Headroom, Forecast Accuracy, Congestion Hours, Capex Plan Adherence", 27),
        new("HEAD_VENDOR", "Head of Vendor Management", 4, 24_000m, 40_000m, "PROCUREMENT", "VP_NET", true, "Bachelor's Degree", 9, "Vendor Management, Contract Negotiation, OEM Relationships, SLA Management",
            "Manages strategic OEM and managed services partnerships for {Company} — Ericsson, Nokia, Huawei, ZTE, IBM",
            "T-7",
            "Own {Company} key vendor relationships; negotiate frame agreements; manage SLAs and penalties; drive vendor performance reviews",
            "Vendor SLA Compliance, Cost Savings, Contract Renewal Cycle, OEM Escalation Closure", 27),
        new("HEAD_CX", "Head of Customer Experience Operations", 4, 24_000m, 40_000m, "CUSTOMER-EXPERIENCE", "VP_CX", true, "Bachelor's Degree", 9, "Contact Centre Ops, WFM, Quality, Knowledge Management, Service Recovery",
            "Heads {Company} contact centre operations, workforce management, and service recovery", "T-7",
            "Run {Company} contact centre WFM and quality; manage shift rosters; drive AHT/FCR; lead service recovery and complaints management",
            "AHT, FCR, Quality Score, Adherence, Service Recovery Time", 27),
        new("MM_MGR", "Mobile Money Manager", 4, 26_000m, 44_000m, "MOBILE-MONEY", "CCO", true, "Bachelor's Degree", 8, "Mobile Money, Agent Network, Wallet Operations, AML/CFT, BoG Compliance",
            "Manages {Company}'s mobile money business — wallet, merchant, agents, and BoG compliance", "T-7",
            "Run {Company} Mobile Money operations; grow active wallets and transaction volume; manage agent network; ensure BoG and AML compliance",
            "Active Wallets, MoMo Revenue, Transaction Volume, Agent Liquidity, AML/CFT Compliance", 27),

        // ── Rank 4 — Senior Product / Specialist (10 roles) ───────────────────
        new("SR_PM_POSTPAID", "Senior Product Manager, Postpaid", 4, 26_000m, 42_000m, "COMMERCIAL", "CCO", true, "Bachelor's Degree", 8, "Product Management, Postpaid Tariffs, Customer Lifecycle, Pricing",
            "Owns {Company}'s postpaid product portfolio — tariffs, lifecycle, retention, and high-value segment", "T-6",
            "Manage {Company} postpaid P&L; design tariffs and bundles; drive postpaid retention and ARPU; lead high-value segment programmes",
            "Postpaid Net Adds, Postpaid ARPU, Postpaid Churn, Bundle Take-up", 27),
        new("SR_PM_PREPAID", "Senior Product Manager, Prepaid", 4, 25_000m, 40_000m, "COMMERCIAL", "CCO", true, "Bachelor's Degree", 8, "Prepaid Products, Bonus Engines, Recharge Patterns, Mass Market",
            "Owns {Company}'s prepaid product portfolio — bonuses, validity, recharge mechanics, mass-market plans", "T-6",
            "Manage {Company} prepaid P&L; design bonus engines and validity schemes; drive recharge frequency; oversee mass-market promotions",
            "Prepaid ARPU, Recharge Frequency, Bonus Conversion, Active Subscribers, Gross Adds", 27),
        new("SR_PM_DATA", "Senior Product Manager, Data", 4, 26_000m, 42_000m, "COMMERCIAL", "CCO", true, "Bachelor's Degree", 8, "Mobile Data, Bundles, Pricing, Data Monetisation, Streaming Partnerships",
            "Owns {Company}'s data product portfolio — bundles, pricing, partnerships, and data monetisation", "T-6",
            "Manage {Company} data P&L; design data bundles; structure partnerships (streaming, social); drive data ARPU and usage",
            "Data Revenue, Data ARPU, MB per User, Bundle Penetration, Data Net Adds", 27),
        new("SR_PM_ENT", "Senior Product Manager, Enterprise", 4, 26_000m, 42_000m, "COMMERCIAL", "CCO", true, "Bachelor's Degree", 8, "Enterprise Products, Connectivity, Cloud, IoT, Vertical Solutions",
            "Owns {Company}'s enterprise product portfolio — connectivity, cloud, IoT, and vertical solutions", "T-6",
            "Manage {Company} enterprise product P&L; build connectivity, cloud, IoT propositions; partner with sales on solutioning",
            "Enterprise Revenue, Solution Margin, Pipeline Conversion, Logo Acquisition", 27),
        new("SR_PM_ROAMING", "Senior Product Manager, Roaming", 4, 24_000m, 38_000m, "COMMERCIAL", "CCO", true, "Bachelor's Degree", 8, "Roaming, Inter-Operator Tariffs, Wholesale, GSMA Roaming Agreements",
            "Owns {Company}'s roaming and wholesale roaming partnerships", "T-6",
            "Manage {Company} roaming portfolio; negotiate IOTs with international partners; design retail roaming bundles; grow inbound and outbound revenue",
            "Roaming Revenue, IOT Cost, Roamer ARPU, Roaming Partner Coverage", 27),
        new("SR_PM_DEVICES", "Senior Product Manager, Devices", 4, 24_000m, 38_000m, "COMMERCIAL", "CCO", true, "Bachelor's Degree", 8, "Device Portfolio, OEM Partnerships, Bundling, Smartphone Penetration",
            "Owns {Company}'s device strategy — handsets, MiFi, IoT devices, and smartphone bundles", "T-6",
            "Manage {Company} device portfolio; negotiate OEM commercial terms; design device + plan bundles; drive smartphone penetration",
            "Device Revenue, Smartphone Penetration, Device-Plan Attach Rate, Device Margin", 27),
        new("BRAND_MGR", "Brand Manager", 4, 24_000m, 38_000m, "MARKETING", "VP_MKT", true, "Bachelor's Degree", 7, "Brand Strategy, Campaign Development, Agency Management, Sponsorships",
            "Owns {Company} brand health, creative campaigns, and master-brand sponsorships", "T-6",
            "Develop {Company} brand campaigns; manage creative agencies; oversee sponsorship execution; track brand health metrics",
            "Brand Health Index, Top-of-Mind Awareness, Campaign Effectiveness, Sponsorship ROI", 27),
        new("PERF_MKT_MGR", "Performance Marketing Manager", 4, 24_000m, 38_000m, "MARKETING", "VP_MKT", true, "Bachelor's Degree", 7, "Performance Marketing, Paid Social, Search, Programmatic, Attribution",
            "Drives {Company}'s digital acquisition through paid search, social, programmatic, and partnerships", "T-6",
            "Run {Company} paid digital channels; optimise CPA across funnel; manage attribution; partner with product on landing pages and offers",
            "CPA, Paid Conversions, ROAS, CTR, Funnel Velocity", 27),
        new("B2B_SALES_MGR", "B2B Sales Manager", 4, 26_000m, 42_000m, "COMMERCIAL", "VP_SALES", true, "Bachelor's Degree", 8, "B2B Sales, Solution Selling, Pipeline Management, Government Accounts",
            "Manages {Company}'s B2B sales team across enterprise, government, and large corporates", "T-6",
            "Lead {Company} enterprise sales team; manage pipeline and forecast; close strategic accounts; drive solution attach rate",
            "Enterprise Revenue, Pipeline Coverage, Win Rate, Solution Attach Rate, Logo Wins", 27),
        new("REG_AFFAIRS_MGR", "Regulatory Affairs Manager", 4, 26_000m, 44_000m, "REGULATORY-AFFAIRS", "CEO", true, "Master's Degree", 8, "Telecom Regulation, NCA Compliance, Policy Advocacy, Licensing",
            "Manages {Company}'s NCA, NITA, MoCD, and DPC relationships; licence and QoS compliance", "T-6",
            "Own {Company} NCA submissions; manage QoS compliance reporting; lead policy advocacy; coordinate licence renewals and spectrum auctions",
            "Licence Compliance, NCA QoS Score, Penalty Avoidance, Policy Influence Index", 27),
        new("PROC_MGR", "Procurement Manager", 4, 24_000m, 38_000m, "PROCUREMENT", "CFO_TELCO", true, "Bachelor's Degree", 7, "Strategic Sourcing, Tender Management, Vendor Evaluation, Contract Management",
            "Manages {Company}'s strategic sourcing, tender process, and vendor evaluation", "T-6",
            "Lead {Company} sourcing for network, IT, marketing, and indirect spend; run tenders; evaluate proposals; manage contracts and SLAs",
            "Cost Savings, Tender Cycle Time, Vendor Compliance, Contract Renewal Timeliness", 27),
        new("FIN_MGR", "Finance Manager", 4, 26_000m, 44_000m, "FINANCE", "CFO_TELCO", true, "Bachelor's Degree / ACCA", 8, "Financial Reporting, Management Accounting, Capex Governance, IFRS",
            "Manages {Company}'s financial reporting, capex governance, and management accounts", "T-6",
            "Lead {Company} month-end close; produce management accounts; manage capex governance and tracking; support audit and tax",
            "Reporting Timeliness, Capex Variance, Audit Findings, Cost-to-Revenue Ratio", 27),
        new("TALENT_MGR", "Talent Manager", 4, 24_000m, 38_000m, "HR", "CHRO_TELCO", true, "Bachelor's Degree", 7, "Talent Acquisition, Employer Brand, Succession Planning, Workforce Planning",
            "Leads {Company} talent acquisition, employer brand, and succession planning", "T-6",
            "Run {Company} talent strategy; manage recruitment pipeline; build employer brand; lead succession and workforce planning",
            "Time-to-Hire, Quality of Hire, Offer Acceptance, Succession Coverage, Employer Brand Score", 27),
        new("RETAIL_MGR", "Retail Manager", 4, 24_000m, 36_000m, "COMMERCIAL", "VP_SALES", true, "Bachelor's Degree", 7, "Retail Operations, Service Centre Management, Visual Merchandising, Sales Coaching",
            "Manages {Company}'s flagship service centres and retail network across Ghana", "T-6",
            "Run {Company} owned retail; oversee service centre operations; coach store managers; drive retail revenue and NPS",
            "Retail Revenue, Footfall Conversion, Service Centre NPS, Store Compliance Score", 27),

        // ── Rank 3 — Managers / Senior Engineers / Specialists (15 roles) ─────
        new("SR_NET_ENG", "Senior Network Engineer", 3, 11_000m, 22_000m, "NETWORKS", "HEAD_RAN", false, "Bachelor's in EE / CS", 7, "RAN/Core/Transport Engineering, Optimisation, Capacity, Vendor Tools",
            "Senior engineer across {Company}'s RAN, core, and transport — escalation point and design authority", "NE-001",
            "Lead complex {Company} network engineering; mentor junior engineers; own optimisation projects; review designs; troubleshoot tier-3 incidents",
            "Optimisation KPIs, MTTR on Escalations, Design Review Quality, Mentee Progression", 24),
        new("NET_ENG_RAN", "Network Engineer (RAN)", 3, 11_000m, 20_000m, "NETWORKS", "HEAD_RAN", false, "Bachelor's in EE", 4, "RAN, RF Optimisation, Drive Tests, Vendor Tools (Ericsson/Nokia/Huawei)",
            "Engineers and optimises {Company}'s radio access network — 4G/5G coverage and capacity", "NE-002",
            "Plan and optimise {Company} RAN; conduct drive tests; integrate new sites; troubleshoot RAN incidents; coordinate with field teams",
            "RAN KPIs, Drop Call Rate, Throughput, Site Integration Cycle Time", 24),
        new("NET_ENG_CORE", "Network Engineer (Core)", 3, 12_000m, 22_000m, "NETWORKS", "HEAD_CORE", false, "Bachelor's in CS / EE", 4, "Core Network, EPC, IMS, Signalling, VoLTE, 5GC",
            "Engineers and operates {Company}'s mobile core — packet core, IMS, voice, and signalling", "NE-003",
            "Operate and engineer {Company} core nodes; troubleshoot signalling and voice issues; deliver core upgrades; manage HLR/HSS",
            "Core Availability, Signalling Success, VoLTE Quality, Change Success Rate", 24),
        new("NET_ENG_TX", "Network Engineer (Transport)", 3, 11_000m, 20_000m, "NETWORKS", "HEAD_TX", false, "Bachelor's in EE", 4, "Transmission, IP/MPLS, DWDM, Microwave, Fibre",
            "Engineers and operates {Company}'s transmission and IP/MPLS network", "NE-004",
            "Operate {Company} transmission network; commission new links; troubleshoot fibre and microwave; manage IP/MPLS routing",
            "Transmission Availability, Link MTTR, Capacity Utilisation, Change Success Rate", 24),
        new("NOC_ENG", "NOC Engineer (24x7)", 3, 11_000m, 20_000m, "NETWORKS", "HEAD_SVC_OPS", false, "Bachelor's Degree / Diploma", 3, "NOC Tools, Incident Management, ITIL, OSS, Shift Operations",
            "Operates {Company}'s 24x7 Network Operations Centre — first response across all network domains", "NOC-001",
            "Monitor {Company} network on rotating shifts; triage alarms; open and own tickets; coordinate field dispatch; escalate per runbook",
            "Mean Time to Acknowledge, Ticket Closure Rate, False Alarm Rate, Shift Handover Quality", 24),
        new("FIELD_ENG", "Field Engineer", 3, 11_000m, 18_000m, "NETWORKS", "HEAD_FIELD_OPS", false, "Bachelor's Degree / Diploma", 3, "Field Operations, BTS Maintenance, Power & Cooling, Tower Safety",
            "Field engineer maintaining {Company} cell sites — preventive and corrective maintenance", "FE-001",
            "Conduct {Company} site PM and CM; resolve site faults; manage genset and rectifier issues; coordinate towerco escalations",
            "Site Uptime, MTTR, PM Compliance, HSE Adherence", 24),
        new("SITE_ENG", "Site Engineer", 3, 11_000m, 18_000m, "NETWORKS", "HEAD_FIELD_OPS", false, "Bachelor's Degree / Diploma", 3, "Site Acquisition, Civil Works, Tower Engineering, Site Audits",
            "Manages {Company} site engineering — acquisition, civil works, and rollout supervision", "SE-001",
            "Supervise new {Company} site builds; manage civil contractors; conduct site audits; coordinate with regulatory bodies for permits",
            "Site Build Cycle Time, Audit Pass Rate, Permit Compliance, Civil Works Quality", 24),
        new("OFC_ENG", "Optical Fibre Engineer", 3, 11_000m, 19_000m, "NETWORKS", "HEAD_TX", false, "Bachelor's in EE / Diploma", 4, "OFC Splicing, OTDR, DWDM, Fibre Survey, Last-Mile",
            "Engineers and maintains {Company}'s optical fibre network — splicing, OTDR, DWDM", "OFC-001",
            "Splice and test {Company} fibre; conduct OTDR diagnostics; commission DWDM links; resolve fibre cuts; survey new routes",
            "Fibre Cut MTTR, Splice Quality, Link Availability, Survey Throughput", 24),
        new("IPMPLS_ENG", "IP/MPLS Engineer", 3, 12_000m, 22_000m, "NETWORKS", "HEAD_TX", false, "Bachelor's in CS / EE", 4, "IP Routing, MPLS, BGP, OSPF, Cisco/Juniper, QoS",
            "Engineers and operates {Company}'s IP/MPLS backbone — routing, QoS, and traffic engineering", "IP-001",
            "Operate {Company} IP/MPLS core; manage BGP and OSPF; deliver QoS and TE policies; troubleshoot routing incidents",
            "Backbone Availability, BGP Stability, Change Success, MTTR", 24),
        new("BSS_ENG_T", "BSS Engineer", 3, 11_000m, 20_000m, "IT", "HEAD_IT", false, "Bachelor's in CS", 4, "BSS, Billing Systems, CRM, Charging, Mediation, SQL",
            "Engineers and supports {Company}'s BSS estate — billing, CRM, charging, and mediation", "BSS-001",
            "Operate {Company} BSS platforms; deliver tariff configurations; troubleshoot billing issues; support CRM integrations",
            "BSS Uptime, Billing Accuracy, Tariff Deployment Cycle, Defect Resolution Time", 24),
        new("OSS_ENG_T", "OSS Engineer", 3, 11_000m, 20_000m, "NETWORKS", "HEAD_SVC_OPS", false, "Bachelor's in CS / EE", 4, "OSS, Fault Management, Performance Management, Provisioning, Network Inventory",
            "Engineers and operates {Company}'s OSS — fault, performance, inventory, and provisioning", "OSS-001",
            "Operate {Company} OSS platforms; develop dashboards and reports; integrate new network elements; support service assurance",
            "OSS Availability, Report Delivery, Integration Cycle Time, Data Quality", 24),
        new("CYBER_ANALYST", "Cybersecurity Analyst", 3, 11_000m, 20_000m, "CYBERSECURITY", "HEAD_CYBER", false, "Bachelor's in CS", 3, "SIEM, Threat Hunting, Incident Response, Vulnerability Management, ISO 27001",
            "Monitors {Company}'s SOC, hunts threats, and responds to security incidents", "CYB-001",
            "Monitor SIEM dashboards; investigate alerts; respond to incidents; conduct threat hunting; produce weekly threat reports",
            "MTTD, MTTR, Vulns Closed, False Positive Rate, Incident Containment Time", 24),
        new("IT_SUPPORT_T", "IT Support Engineer", 3, 11_000m, 18_000m, "IT", "HEAD_IT", false, "Bachelor's Degree / Diploma", 2, "IT Helpdesk, Active Directory, Endpoint Management, M365",
            "Provides IT support across {Company} — endpoints, identity, productivity tools, and field IT", "IT-001",
            "Resolve {Company} IT tickets; manage AD and endpoints; support M365; provide field IT for events and openings",
            "First Call Resolution, Ticket Backlog, User Satisfaction, SLA Compliance", 24),
        new("SOL_ARCH", "Solutions Architect", 3, 13_000m, 24_000m, "IT", "CIO", false, "Bachelor's Degree", 5, "Solution Architecture, BSS/OSS, APIs, Integration, Cloud, TM Forum",
            "Designs end-to-end solutions across {Company}'s BSS/OSS, network, and digital channels", "SA-001",
            "Define {Company} solution designs for new products; review integration architecture; lead TM Forum alignment; mentor engineers",
            "Design Quality, Time-to-Solution, Reuse Rate, Integration Defect Rate", 24),
        new("MM_OPS", "Mobile Money Operations", 3, 11_000m, 18_000m, "MOBILE-MONEY", "MM_MGR", false, "Bachelor's Degree", 3, "Mobile Money Operations, Reconciliation, Agent Liquidity, BoG Reporting",
            "Runs {Company} Mobile Money daily operations — reconciliation, agent liquidity, BoG reporting", "MM-001",
            "Reconcile {Company} MoMo float; monitor agent liquidity; produce BoG returns; resolve customer escalations and reversal queries",
            "Reconciliation Accuracy, BoG Return Timeliness, Agent Liquidity Coverage, Reversal Cycle Time", 24),
        new("FRAUD_ANALYST", "Fraud Analyst", 3, 11_000m, 19_000m, "MOBILE-MONEY", "HEAD_CYBER", false, "Bachelor's Degree", 3, "Fraud Analytics, Telco Fraud, MoMo Fraud, Rules & Models, Investigation",
            "Detects and investigates fraud across {Company}'s telco and Mobile Money rails", "FRD-001",
            "Run {Company} fraud detection; tune rules; investigate cases; coordinate with Cyber and Legal; produce fraud loss reports",
            "Fraud Loss Avoided, Detection Rate, Case Closure Time, False Positive Rate", 24),
        new("RISK_COMP_OFFICER", "Risk & Compliance Officer", 3, 11_000m, 18_000m, "FINANCE", "FIN_MGR", false, "Bachelor's Degree", 3, "Enterprise Risk, Compliance, AML/CFT, BoG/NCA Regulations, Risk Assessment",
            "Manages {Company} enterprise risk register and compliance with NCA, BoG, GRA, and DPC", "RC-001",
            "Maintain {Company} risk register; conduct compliance reviews; train staff; support audit and regulatory engagement",
            "Risk Register Currency, Compliance Training Completion, Audit Findings, Regulatory Penalties", 24),
        new("ENT_ACCT_MGR", "Enterprise Account Manager", 3, 11_000m, 19_000m, "COMMERCIAL", "B2B_SALES_MGR", false, "Bachelor's Degree", 4, "Enterprise Sales, Solution Selling, Account Planning, Government Accounts",
            "Manages {Company}'s strategic enterprise accounts — banks, oil & gas, MDAs, multinationals", "EAM-001",
            "Own assigned {Company} enterprise portfolio; develop account plans; close upsell deals; coordinate solutioning and delivery",
            "Account Revenue, Wallet Share, Retention Rate, Upsell Conversion", 24),
        new("SME_ACCT_MGR", "SME Account Manager", 3, 11_000m, 18_000m, "COMMERCIAL", "B2B_SALES_MGR", false, "Bachelor's Degree", 3, "SME Sales, Connectivity, Cloud, Bundle Selling, Pipeline Management",
            "Manages {Company}'s SME portfolio — connectivity, cloud, and Mobile Money business solutions", "SAM-001",
            "Acquire and grow {Company} SME accounts; cross-sell connectivity and cloud; build referral pipeline; achieve quarterly quota",
            "SME Revenue, New SME Logos, Cross-Sell Ratio, Pipeline Health", 24),
        new("DISTRIBUTOR_MGR", "Distributor Manager", 3, 11_000m, 18_000m, "COMMERCIAL", "VP_SALES", false, "Bachelor's Degree", 4, "Distributor Management, Trade Sales, Stock Management, Field Coaching",
            "Manages {Company}'s distributor and dealer network — stock, sell-out, and trade activation", "DM-001",
            "Run assigned {Company} distributor portfolio; ensure stock availability; drive sell-out; coach field force; resolve trade issues",
            "Distributor Sell-Out, Stock Cover, Distribution Reach, Trade NPS", 24),
        new("COMM_ANALYST", "Commercial Analyst", 3, 11_000m, 18_000m, "COMMERCIAL", "CCO", false, "Bachelor's Degree", 3, "Commercial Analytics, SQL, Dashboards, Revenue Reporting, Segmentation",
            "Provides commercial insight to {Company} — revenue, ARPU, segmentation, and product performance", "CA-001",
            "Build {Company} commercial dashboards; produce segment and product reports; support pricing decisions; partner with finance",
            "Report Timeliness, Insight Quality, Forecast Accuracy, Stakeholder NPS", 24),
        new("PRICING_ANALYST", "Pricing Analyst", 3, 11_000m, 18_000m, "COMMERCIAL", "CCO", false, "Bachelor's Degree", 3, "Pricing Analytics, Elasticity, Competitor Analysis, Tariff Modelling",
            "Analyses pricing and tariff performance across {Company}'s consumer and enterprise portfolios", "PA-001",
            "Model {Company} pricing elasticity; build tariff business cases; track competitor moves; support pricing committee",
            "Tariff Margin, Elasticity Accuracy, Pricing Cycle Time, Win Rate vs Competition", 24),
        new("TRADE_MKT_OFFICER", "Trade Marketing Officer", 3, 11_000m, 17_000m, "MARKETING", "VP_MKT", false, "Bachelor's Degree", 3, "Trade Marketing, BTL Activation, POS Materials, Field Promotion",
            "Executes {Company}'s trade marketing — POS materials, activations, and channel promotions", "TM-001",
            "Plan and execute {Company} trade activations; manage POS distribution; coordinate with field sales; measure activation ROI",
            "Activation ROI, POS Compliance Rate, Activation Reach, Channel Engagement Score", 24),
        new("FIN_ANALYST", "Finance Analyst", 3, 11_000m, 17_000m, "FINANCE", "FIN_MGR", false, "Bachelor's Degree / Part ACCA", 3, "Financial Analysis, Excel, Budgeting, Variance Analysis, Reporting",
            "Supports {Company}'s budgeting, forecasting, and variance reporting", "FA-001",
            "Build {Company} budget models; produce monthly variance reports; support reforecasts; analyse cost drivers; prepare board packs",
            "Forecast Accuracy, Variance Report Timeliness, Model Quality, Stakeholder Feedback", 24),
        new("HR_OFFICER", "HR Officer", 3, 11_000m, 17_000m, "HR", "TALENT_MGR", false, "Bachelor's Degree", 3, "HR Operations, Recruitment, Employee Relations, HRIS, Labour Law",
            "Manages {Company} HR operations — recruitment support, employee relations, and HRIS", "HR-001",
            "Coordinate {Company} recruitment; handle ER cases; maintain HRIS; support payroll inputs; run onboarding",
            "Vacancy Fill Time, ER Case Closure, HRIS Data Quality, Onboarding NPS", 24),
        new("LD_SPECIALIST", "Learning & Development Specialist", 3, 11_000m, 17_000m, "HR", "TALENT_MGR", false, "Bachelor's Degree", 3, "L&D Design, LMS, Facilitation, Learning Needs Analysis, Vendor Management",
            "Designs and delivers learning programmes across {Company} — technical, leadership, sales", "LD-001",
            "Run {Company} L&D calendar; manage LMS; design learning interventions; evaluate impact; support {Company} Academy",
            "Training Hours per FTE, Completion Rate, Learning ROI, Trainee NPS", 24),
        new("LOG_OFFICER", "Logistics Officer", 3, 11_000m, 16_000m, "PROCUREMENT", "PROC_MGR", false, "Bachelor's Degree", 3, "Warehouse, Logistics, Distribution, Inventory Management, 3PL",
            "Manages {Company} warehousing, distribution, and 3PL operations for SIMs, scratch cards, and devices", "LOG-001",
            "Run {Company} warehouse operations; manage distribution to regions; coordinate 3PL; ensure inventory accuracy",
            "Inventory Accuracy, Distribution On-Time, Stock Cover, Damage/Loss Rate", 24),
        new("RETENTION_OFFICER_T", "Customer Retention Officer", 3, 11_000m, 16_000m, "CUSTOMER-EXPERIENCE", "VP_CX", false, "Bachelor's Degree", 3, "Churn Management, Loyalty, Win-Back, Customer Analytics",
            "Drives {Company} retention — churn prediction, save desk, and loyalty programmes", "RET-001",
            "Run {Company} save desk; design retention offers; analyse churn drivers; manage loyalty programme",
            "Save Rate, Churn Reduction, Loyalty Active Members, Win-Back Conversion", 24),
        new("CC_TEAM_LEAD", "Customer Care Team Lead", 3, 11_000m, 16_000m, "CUSTOMER-EXPERIENCE", "HEAD_CX", true, "Bachelor's Degree", 3, "Contact Centre Supervision, Coaching, Quality, WFM",
            "Supervises a team of {Company} contact centre agents — coaching, quality, and shift management", "CC-001",
            "Lead {Company} contact centre team; conduct coaching and side-by-sides; manage adherence; escalate critical cases",
            "Team AHT, Team FCR, Quality Score, Adherence, Coaching Hours", 24),
        new("RETAIL_SUP", "Retail Store Supervisor", 3, 11_000m, 16_000m, "COMMERCIAL", "RETAIL_MGR", true, "Bachelor's Degree / HND", 3, "Retail Supervision, Sales Coaching, Visual Merchandising, Cash Management",
            "Supervises a {Company} flagship service centre — sales, service, and store operations", "RS-001",
            "Run a {Company} service centre; coach store agents; ensure VM compliance; manage daily cash and stock; resolve customer escalations",
            "Store Revenue, Store NPS, VM Compliance, Stock Variance", 24),

        // ── Rank 2 — Engineers / Officers / Agents (8 roles) ──────────────────
        new("TOWER_CLIMBER", "Tower Climber", 2, 4_800m, 9_000m, "NETWORKS", "FIELD_ENG", false, "SHS / Trade Certificate", 2, "Tower Climbing, Rigging, HSE, Antenna Tilts, Power Cabling",
            "Certified rigger climbing {Company}'s towers for antenna work, line-of-sight, and microwave alignment", "TC-001",
            "Climb {Company} towers; install and tilt antennas; align microwave dishes; perform tower audits; observe HSE protocols",
            "Climb Productivity, HSE Incidents, Tilt Accuracy, Audit Pass Rate", 21),
        new("CC_AGENT_VOICE", "Call Centre Agent (Voice)", 2, 4_800m, 8_500m, "CUSTOMER-EXPERIENCE", "CC_TEAM_LEAD", false, "HND / Diploma", 0, "Customer Service, Active Listening, CRM, Telco Products",
            "Handles inbound voice calls for {Company} — service, billing, and product enquiries", "CC-002",
            "Answer {Company} customer calls; resolve queries; log interactions in CRM; upsell where appropriate; escalate complex cases",
            "AHT, FCR, CSAT, Adherence, Quality Score", 21),
        new("CC_AGENT_CHAT", "Call Centre Agent (Chat)", 2, 4_800m, 8_500m, "CUSTOMER-EXPERIENCE", "CC_TEAM_LEAD", false, "HND / Diploma", 0, "Live Chat, Multi-Tasking, Written Communication, CRM",
            "Handles chat customer interactions for {Company} across web and app", "CC-003",
            "Handle multiple {Company} chat conversations; resolve queries; deflect calls; log in CRM; meet quality and AHT targets",
            "Concurrent Chats, Chat AHT, FCR, CSAT, Quality Score", 21),
        new("CC_AGENT_SOCIAL", "Call Centre Agent (Social)", 2, 4_800m, 8_500m, "CUSTOMER-EXPERIENCE", "CC_TEAM_LEAD", false, "HND / Diploma", 0, "Social Media, Brand Voice, Public Communication, Crisis Handling",
            "Handles {Company} social media customer care across Facebook, X, Instagram, TikTok", "CC-004",
            "Respond to {Company} social mentions and DMs; protect brand voice; deflect to private channels; flag PR risks",
            "Response Time, Sentiment, Resolution Rate, Escalation Quality", 21),
        new("BACKOFFICE_OFFICER", "Backoffice Officer", 2, 4_800m, 9_000m, "CUSTOMER-EXPERIENCE", "HEAD_CX", false, "HND / Diploma", 1, "Backoffice Processing, Adjustments, Reversals, BSS Tools, Documentation",
            "Processes {Company} backoffice tickets — adjustments, reversals, and provisioning escalations", "BO-001",
            "Resolve {Company} backoffice tickets; process adjustments and reversals; coordinate with BSS and Network; document outcomes",
            "Ticket Closure Rate, SLA Compliance, Accuracy Rate, Repeat Ticket Rate", 21),
        new("SALES_AGENT", "Sales Agent", 2, 4_800m, 8_000m, "COMMERCIAL", "RETAIL_SUP", false, "HND / Diploma", 0, "Retail Sales, Customer Service, Telco Products, POS",
            "Sells SIMs, devices, and bundles at {Company} service centres and authorised dealer outlets", "SA-002",
            "Greet customers; sell {Company} products; activate SIMs; handle till and stock; meet daily sales targets",
            "Daily Sales Target, Activation Quality, Cash Variance, Customer NPS", 21),
        new("FIELD_SALES_REP", "Field Sales Representative", 2, 4_800m, 9_000m, "COMMERCIAL", "DISTRIBUTOR_MGR", false, "HND / Diploma", 1, "Field Sales, Trade, Merchandising, Route Planning",
            "Field representative driving {Company} trade sales — stockists, retailers, vendors", "FSR-001",
            "Cover assigned route; ensure {Company} stock availability; deploy POS; train trade; collect orders; report market intelligence",
            "Route Coverage, Stock Availability, Order Volume, POS Compliance", 21),

        // ── Rank 1 — Trainees & Interns (3 roles) ─────────────────────────────
        new("RAN_TRAINEE", "Network Engineering Trainee", 1, 2_000m, 4_000m, "NETWORKS", "NET_ENG_RAN", false, "Bachelor's Degree", 0, "Network Fundamentals, Vendor Tools, Drive Tests, Field Work",
            "Entry-level rotational programme rotating through {Company} RAN, core, transmission, and NOC", "T-1",
            "Complete {Company} engineering rotations; shadow senior engineers; deliver assigned projects; achieve learning milestones",
            "Rotation Assessments, Project Delivery, Learning Milestones, Supervisor Feedback", 21),
        new("IT_INTERN", "IT / BSS Intern", 1, 2_000m, 3_500m, "IT", "BSS_ENG_T", false, "Current University Student", 0, "IT Basics, SQL, Telco Concepts, Documentation",
            "University intern attached to {Company} IT and BSS teams for project work", "T-1",
            "Support {Company} IT and BSS teams; complete intern project; shadow engineers; submit internship report",
            "Project Quality, Attendance, Supervisor Score, Internship Report", 21),
        new("CC_TRAINEE", "Customer Care Trainee", 1, 2_000m, 3_500m, "CUSTOMER-EXPERIENCE", "CC_TEAM_LEAD", false, "HND / SHS", 0, "Customer Service Basics, Telco Products, Soft Skills",
            "Trainee programme for {Company} contact centres — induction, nesting, and certification", "T-1",
            "Complete {Company} care induction; pass nesting milestones; certify on products; transition to live agent role",
            "Certification Pass Rate, Nesting Quality, Attendance, Supervisor Feedback", 21)
    ];

    // ── Curated Telco station catalogue ───────────────────────────────────────
    // ~50 stations covering the typical footprint of a tier-1 Ghana MNO:
    //   1 HQ (Accra), 2-3 Regional Offices (Kumasi/Takoradi/Tamale),
    //   4-5 Switching Centres / Data Centres / MTSO,
    //   ~20 Service Centres (flagship retail/service across major cities),
    //   ~15 Customer Care Centres / Authorised Dealer Outlets,
    //   ~5 Field Operations Hubs (tower maintenance crews).
    // Phones use NCA-allocated 030/024/055 ranges; emails store local-part only — the
    // factory appends the tenant TLD at row-emit time. {Company} placeholder substituted.
    private static readonly StationSpec[] _telcoStations =
    [
        // ── HQ ────────────────────────────────────────────────────────────────
        new("HO-001", "Head Office", "Head Office", "Greater Accra", "Accra", "Independence Avenue, Ridge", 80, 1500,
            "{Company} corporate headquarters — executive, commercial, marketing, IT, finance, HR, and regulatory affairs",
            "+233 30 XXX XXXX", "headoffice"),

        // ── Regional Offices (3) ──────────────────────────────────────────────
        new("RO-ASH-001", "Ashanti Regional Office - Kumasi", "Regional Office", "Ashanti", "Kumasi", "Prempeh II Street, Adum", 30, 120,
            "{Company} Ashanti regional office — manages Ashanti & Bono regions networks, sales, and care",
            "+233 30 XXX XXXX", "kumasi.region"),
        new("RO-WES-001", "Western Regional Office - Takoradi", "Regional Office", "Western", "Takoradi", "Harbour Road, Market Circle", 25, 100,
            "{Company} Western regional office — covers Western, Western North, and Central regions",
            "+233 30 XXX XXXX", "takoradi.region"),
        new("RO-NOR-001", "Northern Regional Office - Tamale", "Regional Office", "Northern", "Tamale", "Salaga Road, Central Tamale", 25, 100,
            "{Company} Northern regional office — coordinates Northern, Upper East, Upper West, Savannah & North East",
            "+233 30 XXX XXXX", "tamale.region"),

        // ── Switching Centres / Data Centres / MTSO (5) ────────────────────────
        new("MSC-ACC-001", "Accra Mobile Switching Centre", "Switching Centre", "Greater Accra", "Accra", "Spintex Road, Accra", 20, 80,
            "{Company} primary mobile switching centre — core voice, signalling, HLR/HSS for southern Ghana",
            "+233 30 XXX XXXX", "msc.accra"),
        new("MSC-KSI-001", "Kumasi Mobile Switching Centre", "Switching Centre", "Ashanti", "Kumasi", "Asokwa Industrial Area, Kumasi", 15, 60,
            "{Company} secondary MSC serving the Ashanti, Bono, and middle-belt traffic",
            "+233 30 XXX XXXX", "msc.kumasi"),
        new("DC-TEM-001", "Tema Tier-3 Data Centre", "Data Centre", "Greater Accra", "Tema", "Heavy Industrial Area, Tema", 20, 80,
            "{Company} primary tier-3 data centre — BSS/OSS, charging, billing, and digital channels",
            "+233 30 XXX XXXX", "dc.tema"),
        new("DC-ACC-001", "Accra Disaster Recovery Data Centre", "Data Centre", "Greater Accra", "Accra", "Cantonments, Accra", 15, 60,
            "{Company} disaster recovery data centre — BCP for BSS/OSS and Mobile Money platforms",
            "+233 30 XXX XXXX", "dr.accra"),
        new("MTSO-AIR-001", "Airport City MTSO", "MTSO", "Greater Accra", "Accra", "2 Accra-Tema Motorway, Airport City", 15, 60,
            "{Company} mobile telephone switching office — IP backbone, core routing, and carrier interconnect",
            "+233 30 XXX XXXX", "mtso.airport"),

        // ── Service Centres (20) ──────────────────────────────────────────────
        new("SC-IND-001", "Independence Avenue Service Centre", "Service Centre", "Greater Accra", "Accra", "Independence Avenue, Ridge", 8, 25,
            "{Company} flagship service centre adjacent to HQ — high-value and corporate care",
            "+233 30 XXX XXXX", "sc.independence"),
        new("SC-OSU-001", "Osu Oxford Street Service Centre", "Service Centre", "Greater Accra", "Accra", "Oxford Street, Osu", 8, 25,
            "{Company} Osu service centre — high-traffic walk-in care for the Osu community and businesses",
            "+233 30 XXX XXXX", "sc.osu"),
        new("SC-EL-001", "East Legon Service Centre", "Service Centre", "Greater Accra", "Accra", "Boundary Road, East Legon", 8, 25,
            "{Company} East Legon service centre — premium care for the East Legon corridor",
            "+233 30 XXX XXXX", "sc.eastlegon"),
        new("SC-AIR-001", "Airport City Service Centre", "Service Centre", "Greater Accra", "Accra", "2 Accra-Tema Motorway, Airport City", 8, 25,
            "{Company} Airport City service centre — corporate, oil & gas, and aviation customers",
            "+233 30 XXX XXXX", "sc.airportcity"),
        new("SC-MAD-001", "Madina Service Centre", "Service Centre", "Greater Accra", "Accra", "Madina Market Road, Madina", 8, 25,
            "{Company} Madina service centre — busy market and residential community care",
            "+233 30 XXX XXXX", "sc.madina"),
        new("SC-TEM-001", "Tema Community 1 Service Centre", "Service Centre", "Greater Accra", "Tema", "Community 1, Tema Central", 8, 25,
            "{Company} Tema service centre — port operators, industrialists, and Tema community",
            "+233 30 XXX XXXX", "sc.tema"),
        new("SC-SPX-001", "Spintex Service Centre", "Service Centre", "Greater Accra", "Accra", "Spintex Road, Accra", 8, 25,
            "{Company} Spintex service centre — fast-growing Spintex industrial and residential corridor",
            "+233 30 XXX XXXX", "sc.spintex"),
        new("SC-KAN-001", "Kaneshie Service Centre", "Service Centre", "Greater Accra", "Accra", "Kaneshie Market Road, Kaneshie", 8, 25,
            "{Company} Kaneshie service centre — central market traders and western Accra commercial hub",
            "+233 30 XXX XXXX", "sc.kaneshie"),
        new("SC-ACH-001", "Achimota Service Centre", "Service Centre", "Greater Accra", "Accra", "Achimota, Accra", 8, 25,
            "{Company} Achimota service centre — northern Accra residential and commercial belt",
            "+233 30 XXX XXXX", "sc.achimota"),
        new("SC-ADUM-001", "Adum Service Centre - Kumasi", "Service Centre", "Ashanti", "Kumasi", "Adum, Kumasi", 8, 25,
            "{Company} Adum flagship service centre — heart of Kumasi commercial district",
            "+233 30 XXX XXXX", "sc.adum"),
        new("SC-ASOK-001", "Asokwa Service Centre - Kumasi", "Service Centre", "Ashanti", "Kumasi", "Asokwa, Kumasi", 6, 18,
            "{Company} Asokwa service centre — Kumasi southern industrial and residential zone",
            "+233 30 XXX XXXX", "sc.asokwa"),
        new("SC-OBU-001", "Obuasi Service Centre", "Service Centre", "Ashanti", "Obuasi", "High Street, Obuasi", 5, 15,
            "{Company} Obuasi service centre — gold-mining belt and surrounding communities",
            "+233 30 XXX XXXX", "sc.obuasi"),
        new("SC-TKD-001", "Takoradi Market Circle Service Centre", "Service Centre", "Western", "Takoradi", "Market Circle, Takoradi", 8, 25,
            "{Company} Takoradi flagship service centre — oil & gas, port, and the Twin City",
            "+233 30 XXX XXXX", "sc.takoradi"),
        new("SC-TKW-001", "Tarkwa Service Centre", "Service Centre", "Western", "Tarkwa", "High Street, Tarkwa", 5, 15,
            "{Company} Tarkwa service centre — mining belt and university community",
            "+233 30 XXX XXXX", "sc.tarkwa"),
        new("SC-CCO-001", "Cape Coast Service Centre", "Service Centre", "Central", "Cape Coast", "Commercial Street, Cape Coast", 6, 18,
            "{Company} Cape Coast service centre — university community and tourism corridor",
            "+233 30 XXX XXXX", "sc.capecoast"),
        new("SC-KOF-001", "Koforidua Service Centre", "Service Centre", "Eastern", "Koforidua", "Jackson Park Road, Koforidua", 6, 18,
            "{Company} Koforidua service centre — Eastern Region capital and surrounding districts",
            "+233 30 XXX XXXX", "sc.koforidua"),
        new("SC-SUN-001", "Sunyani Service Centre", "Service Centre", "Bono", "Sunyani", "Fiapre Road, Sunyani", 6, 18,
            "{Company} Sunyani service centre — Bono regional capital",
            "+233 30 XXX XXXX", "sc.sunyani"),
        new("SC-TAM-001", "Tamale Central Service Centre", "Service Centre", "Northern", "Tamale", "Salaga Road, Tamale", 8, 25,
            "{Company} Tamale flagship service centre — Northern Region capital",
            "+233 30 XXX XXXX", "sc.tamale"),
        new("SC-HO-001", "Ho Service Centre", "Service Centre", "Volta", "Ho", "Ho-Aflao Road, Ho Central", 6, 18,
            "{Company} Ho service centre — Volta regional capital and surrounding districts",
            "+233 30 XXX XXXX", "sc.ho"),
        new("SC-WA-001", "Wa Service Centre", "Service Centre", "Upper West", "Wa", "Wa Central, Wa", 5, 15,
            "{Company} Wa service centre — Upper West regional capital",
            "+233 30 XXX XXXX", "sc.wa"),

        // ── Customer Care Centres / Authorised Dealer Outlets (15) ────────────
        new("CCC-DAN-001", "Dansoman Customer Care Centre", "Customer Care Centre", "Greater Accra", "Accra", "Dansoman Last Stop, Dansoman", 3, 8,
            "{Company} Dansoman customer care centre — quick service for the Dansoman community",
            "+233 24 XXX XXXX", "ccc.dansoman"),
        new("CCC-LAP-001", "Lapaz Customer Care Centre", "Customer Care Centre", "Greater Accra", "Accra", "Lapaz, Accra", 3, 8,
            "{Company} Lapaz customer care centre — busy commercial and transport hub",
            "+233 24 XXX XXXX", "ccc.lapaz"),
        new("CCC-ASH-001", "Ashaiman Customer Care Centre", "Customer Care Centre", "Greater Accra", "Ashaiman", "Ashaiman Market Road", 3, 8,
            "{Company} Ashaiman customer care centre — dense trading and residential community",
            "+233 24 XXX XXXX", "ccc.ashaiman"),
        new("CCC-KAS-001", "Kasoa Customer Care Centre", "Customer Care Centre", "Central", "Kasoa", "Kasoa Old Barrier", 3, 8,
            "{Company} Kasoa customer care centre — fast-growing peri-urban hub west of Accra",
            "+233 24 XXX XXXX", "ccc.kasoa"),
        new("CCC-ADE-001", "Adenta Customer Care Centre", "Customer Care Centre", "Greater Accra", "Accra", "Adenta SSNIT Flats, Adenta", 3, 8,
            "{Company} Adenta customer care centre — northern Accra residential community",
            "+233 24 XXX XXXX", "ccc.adenta"),
        new("CCC-DOM-001", "Dome Customer Care Centre", "Customer Care Centre", "Greater Accra", "Accra", "Dome Market, Dome", 3, 8,
            "{Company} Dome customer care centre — northwestern Accra suburbs",
            "+233 24 XXX XXXX", "ccc.dome"),
        new("CCC-TEC-001", "Techiman Customer Care Centre", "Customer Care Centre", "Bono East", "Techiman", "Techiman Market Road", 3, 8,
            "{Company} Techiman customer care centre — major middle-belt market town",
            "+233 24 XXX XXXX", "ccc.techiman"),
        new("CCC-EJI-001", "Ejisu Authorised Dealer Outlet", "Authorised Dealer Outlet", "Ashanti", "Ejisu", "Kumasi-Accra Road, Ejisu", 2, 5,
            "{Company} Ejisu authorised dealer — SIM sales, recharges, and basic care",
            "+233 55 XXX XXXX", "ado.ejisu"),
        new("CCC-MAM-001", "Mampong Authorised Dealer Outlet", "Authorised Dealer Outlet", "Ashanti", "Mampong", "Mampong Central, Mampong", 2, 5,
            "{Company} Mampong authorised dealer — SIM sales, recharges, MoMo cash-in/out",
            "+233 55 XXX XXXX", "ado.mampong"),
        new("CCC-NKA-001", "Nkawkaw Authorised Dealer Outlet", "Authorised Dealer Outlet", "Eastern", "Nkawkaw", "Nkawkaw Lorry Park", 2, 5,
            "{Company} Nkawkaw authorised dealer — Kwahu corridor SIM sales and care",
            "+233 55 XXX XXXX", "ado.nkawkaw"),
        new("CCC-WIN-001", "Winneba Authorised Dealer Outlet", "Authorised Dealer Outlet", "Central", "Winneba", "Winneba Junction", 2, 5,
            "{Company} Winneba authorised dealer — university town SIM and recharge outlet",
            "+233 55 XXX XXXX", "ado.winneba"),
        new("CCC-HOH-001", "Hohoe Authorised Dealer Outlet", "Authorised Dealer Outlet", "Volta", "Hohoe", "Hohoe Market Road", 2, 5,
            "{Company} Hohoe authorised dealer — northern Volta corridor",
            "+233 55 XXX XXXX", "ado.hohoe"),
        new("CCC-BOL-001", "Bolgatanga Authorised Dealer Outlet", "Authorised Dealer Outlet", "Upper East", "Bolgatanga", "Bolga Central, Bolgatanga", 2, 5,
            "{Company} Bolgatanga authorised dealer — Upper East regional capital outlet",
            "+233 55 XXX XXXX", "ado.bolga"),
        new("CCC-BWK-001", "Bawku Authorised Dealer Outlet", "Authorised Dealer Outlet", "Upper East", "Bawku", "Bawku Market Road", 2, 5,
            "{Company} Bawku authorised dealer — north-eastern border town outlet",
            "+233 55 XXX XXXX", "ado.bawku"),
        new("CCC-AOD-001", "Akim Oda Authorised Dealer Outlet", "Authorised Dealer Outlet", "Eastern", "Akim Oda", "Akim Oda High Street", 2, 5,
            "{Company} Akim Oda authorised dealer — eastern cocoa belt outlet",
            "+233 55 XXX XXXX", "ado.akimoda"),

        // ── Field Operations Hubs (5) ─────────────────────────────────────────
        new("FOH-ACC-001", "Accra Field Operations Hub", "Field Operations Hub", "Greater Accra", "Accra", "North Industrial Area, Accra", 8, 30,
            "{Company} Accra field operations hub — tower crews, fuel logistics, and field engineering for Greater Accra",
            "+233 30 XXX XXXX", "foh.accra"),
        new("FOH-KSI-001", "Kumasi Field Operations Hub", "Field Operations Hub", "Ashanti", "Kumasi", "Suame Industrial Area, Kumasi", 6, 24,
            "{Company} Kumasi field operations hub — covers Ashanti and Bono regions for site maintenance",
            "+233 30 XXX XXXX", "foh.kumasi"),
        new("FOH-TKD-001", "Takoradi Field Operations Hub", "Field Operations Hub", "Western", "Takoradi", "Sekondi-Takoradi Industrial Area", 5, 20,
            "{Company} Takoradi field operations hub — Western and Central regions tower maintenance",
            "+233 30 XXX XXXX", "foh.takoradi"),
        new("FOH-TAM-001", "Tamale Field Operations Hub", "Field Operations Hub", "Northern", "Tamale", "Tamale Industrial Area", 5, 20,
            "{Company} Tamale field operations hub — northern Ghana tower and transmission maintenance",
            "+233 30 XXX XXXX", "foh.tamale"),
        new("FOH-HO-001", "Ho Field Operations Hub", "Field Operations Hub", "Volta", "Ho", "Ho Industrial Area", 4, 15,
            "{Company} Ho field operations hub — Volta and Oti regions tower maintenance crews",
            "+233 30 XXX XXXX", "foh.ho")
    ];
}
