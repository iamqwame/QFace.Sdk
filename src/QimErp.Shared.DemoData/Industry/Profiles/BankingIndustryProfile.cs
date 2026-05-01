using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

public sealed class BankingIndustryProfile : IIndustryProfile
{
    public string Code => "BANKING";
    public string DisplayName => "Banking & Financial Services";

    public IReadOnlyList<string> SampleCompanyNames => GhanaBanks.CommercialBanks
        .Select(b => b.Name)
        .ToList();

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
        // Corporate tier uses the Cal Bank Limited 49-station catalogue verbatim — full
        // names, addresses, codes, descriptions, phones, emails, station types per the
        // calbank-stations.xlsx reference. Other tiers fall back to the procedural city-
        // pool builder so smaller banks land with a reasonable shape.
        if (tier == CompanyTier.Corporate)
        {
            var hqRow = _calbankStations[0];
            var rest = _calbankStations.Skip(1).ToList();
            var branchTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Regional Office", "Branch Office", "Field Office", "Co-working Space"
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
            CapacityMin: 100,
            CapacityMax: tier == CompanyTier.Corporate ? 3500 : 500);

        // Branch counts by tier match the real world: GCB/Absa run 100-250 branches,
        // rural banks/microfinance 5-30, startups 1-3.
        var branchCount = tier switch
        {
            CompanyTier.Startup   => Math.Max(1, targetEmployees / 200),
            CompanyTier.SME       => Math.Max(3, targetEmployees / 60),
            CompanyTier.Corporate => Math.Max(20, Math.Min(250, targetEmployees / 50)),
            CompanyTier.NonProfit => Math.Max(2, targetEmployees / 80),
            _                     => 5
        };

        var branches = new List<StationSpec>(branchCount);
        for (var i = 0; i < branchCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            branches.Add(new StationSpec(
                Code: $"BR{i + 1:D3}",
                Name: $"{city} Branch",
                StationType: "Branch",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 15,
                CapacityMax: 80));
        }

        // ATM lobbies — zero headcount, exist only to dress the org chart.
        var satelliteCount = branchCount / 3;
        var satellites = new List<StationSpec>(satelliteCount);
        for (var i = 0; i < satelliteCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            satellites.Add(new StationSpec(
                Code: $"AT{i + 1:D3}",
                Name: $"{city} ATM Lobby",
                StationType: "ATM-Lobby",
                Region: region,
                City: city,
                Address: $"Mall, {city}",
                CapacityMin: 0,
                CapacityMax: 3));
        }

        return new StationLayout(hq, branches, satellites);
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        // 5=executive, 4=senior, 3=mid, 2=junior, 1=entry
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.005,
            [4] = 0.040,
            [3] = 0.150,
            [2] = 0.500,
            [1] = 0.305
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (15_000m, 35_000m),
            [4] = (7_000m,  16_000m),
            [3] = (4_000m,  11_000m),
            [2] = (2_000m,   5_500m),
            [1] = (1_500m,   2_500m)
        });

    private static readonly IReadOnlyList<string> RetailJobs   = ["BRANCH_MGR", "LOAN_OFFICER", "TELLER", "JUNIOR_TELLER", "CSR", "TELLER_TRAINEE"];
    private static readonly IReadOnlyList<string> CorpJobs     = ["REL_MGR", "CORP_OFFICER", "HEAD_CORP"];
    private static readonly IReadOnlyList<string> RiskJobs     = ["CRO", "RISK_MGR", "COMPLIANCE", "RISK_ANALYST"];
    private static readonly IReadOnlyList<string> TreasuryJobs = ["TREASURY_MGR"];
    private static readonly IReadOnlyList<string> ItJobs       = ["CTO", "IT_MGR", "IT_OFFICER"];
    private static readonly IReadOnlyList<string> OpsJobs      = ["OPS_OFFICER", "OPS_ASSIST", "OPS_INTERN"];
    private static readonly IReadOnlyList<string> FinJobs      = ["ACCOUNTANT", "JUNIOR_ACCOUNTANT"];
    private static readonly IReadOnlyList<string> ExecJobs     = ["CEO", "HEAD_RETAIL"];
    private static readonly IReadOnlyList<string> HrJobs       = [];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER", "Founder/CEO", null, OrgUnitKind.Executive, ExecJobs),
        new("OPS",     "Operations",  "FOUNDER", OrgUnitKind.Function, OpsJobs),
        new("TECH",    "Technology",  "FOUNDER", OrgUnitKind.Function, ItJobs)
    ];

    private static readonly IReadOnlyDictionary<string, double> StartupDistribution =
        new Dictionary<string, double>
        {
            ["FOUNDER"] = 0.20,
            ["OPS"]     = 0.50,
            ["TECH"]    = 0.30
        };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",      "Executive",         null,   OrgUnitKind.Executive, ExecJobs),
        new("RETAIL",    "Retail Banking",    "EXEC", OrgUnitKind.Function,  RetailJobs),
        new("CORPORATE", "Corporate Banking", "EXEC", OrgUnitKind.Function,  CorpJobs),
        new("OPS",       "Operations",        "EXEC", OrgUnitKind.Function,  OpsJobs),
        new("RISK",      "Risk & Compliance", "EXEC", OrgUnitKind.Function,  RiskJobs),
        new("HR",        "HR & Admin",        "EXEC", OrgUnitKind.Function,  HrJobs)
    ];

    private static readonly IReadOnlyDictionary<string, double> SmeDistribution =
        new Dictionary<string, double>
        {
            ["EXEC"]      = 0.05,
            ["RETAIL"]    = 0.40,
            ["CORPORATE"] = 0.15,
            ["OPS"]       = 0.20,
            ["RISK"]      = 0.10,
            ["HR"]        = 0.10
        };

    // Corporate-tier baseline OrgUnits — each carries rich Description / Budget /
    // CostCenter / Purpose / Phone / Email-local-part sourced from the Cal Bank
    // calbank-org-v2.xlsx reference data. The {Company} placeholder gets substituted
    // with the actual tenant's company name at row-emit time so the same catalogue
    // reads naturally for any banking tenant. Phone numbers and budget ranges are
    // representative for a tier-1 commercial bank in Ghana (BOG-licensed, GSE-listed).
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",       "Executive",            null,   OrgUnitKind.Executive, ExecJobs,
            Description: "Office of the Managing Director and executive leadership of {Company}",
            BudgetMin: 800_000m, BudgetMax: 1_500_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Set and execute {Company} strategy; lead the executive management committee; manage Bank of Ghana and GSE relationships",
            Phone: "+233 30 222 3104", Email: "executive"),
        new("RETAIL",     "Retail Banking",       "EXEC", OrgUnitKind.Function,  RetailJobs,
            Description: "Personal banking, mass-market deposits, consumer loans, and {Company}'s branch network across Ghana",
            BudgetMin: 3_000_000m, BudgetMax: 6_000_000m,
            CostCenter: "CC-RBD-001",
            Purpose: "Grow {Company} retail deposit base, consumer loan book, and customer numbers across the branch network",
            Phone: "+233 30 222 3200", Email: "retail"),
        new("CORPORATE",  "Corporate & Institutional Banking", "EXEC", OrgUnitKind.Function, CorpJobs,
            Description: "Large corporate clients, public sector, multinationals, and SME banking — {Company}'s wholesale franchise",
            BudgetMin: 4_000_000m, BudgetMax: 8_000_000m,
            CostCenter: "CC-CIB-001",
            Purpose: "Serve Ghana's top corporates, MDAs, and growing SME clients; drive non-funded fee income",
            Phone: "+233 30 222 3201", Email: "corporate"),
        new("INVESTMENT", "Investment Banking & Wealth",   "EXEC", OrgUnitKind.Function, CorpJobs,
            Description: "Capital markets, M&A advisory, private banking, and wealth management for {Company} HNW clients",
            BudgetMin: 2_000_000m, BudgetMax: 4_000_000m,
            CostCenter: "CC-PWM-001",
            Purpose: "Deliver exclusive banking and bespoke investment advisory to high-net-worth individuals and institutional investors",
            Phone: "+233 30 222 3204", Email: "investment"),
        new("OPS",        "Operations",           "EXEC", OrgUnitKind.Function, OpsJobs,
            Description: "Centralised back-office operations, SWIFT, GHIPSS settlements, and payments processing for {Company}",
            BudgetMin: 1_500_000m, BudgetMax: 3_000_000m,
            CostCenter: "CC-OPS-001",
            Purpose: "Process all {Company} transactions accurately and on time; drive operational excellence and process automation",
            Phone: "+233 30 222 3400", Email: "operations"),
        new("RISK",       "Risk & Compliance",    "EXEC", OrgUnitKind.Function, RiskJobs,
            Description: "Enterprise risk management, credit risk, compliance, internal audit, and legal — {Company}'s second and third lines of defence",
            BudgetMin: 1_200_000m, BudgetMax: 2_400_000m,
            CostCenter: "CC-RISK-001",
            Purpose: "Maintain {Company} risk appetite, regulatory compliance, and audit independence across all business lines",
            Phone: "+233 30 222 3500", Email: "risk"),
        new("TREASURY",   "Treasury & Financial Markets", "EXEC", OrgUnitKind.Function, TreasuryJobs,
            Description: "Liquidity management, FX dealing, fixed income trading, and ALCO secretariat — {Company}'s treasury function",
            BudgetMin: 1_200_000m, BudgetMax: 2_500_000m,
            CostCenter: "CC-TFM-001",
            Purpose: "Manage {Company} liquidity, FX positions, and investment portfolio while maintaining BOG cash reserve compliance",
            Phone: "+233 30 222 3300", Email: "treasury"),
        new("IT",         "Information Technology", "EXEC", OrgUnitKind.Function, ItJobs,
            Description: "Core banking T24 platform, digital banking, cybersecurity, and IT infrastructure — {Company}'s technology engine",
            BudgetMin: 3_000_000m, BudgetMax: 6_000_000m,
            CostCenter: "CC-IT-001",
            Purpose: "Run {Company} technology platform with high availability, security, and the digital products customers expect",
            Phone: "+233 30 222 3203", Email: "it"),
        new("FINANCE",    "Finance & Accounts",   "EXEC", OrgUnitKind.Function, FinJobs,
            Description: "Financial reporting, IFRS compliance, BOG prudential returns, budgeting, and management accounting for {Company}",
            BudgetMin: 800_000m, BudgetMax: 1_500_000m,
            CostCenter: "CC-FIN-001",
            Purpose: "Produce accurate {Company} financial statements; manage capital adequacy; deliver timely management reports",
            Phone: "+233 30 222 3600", Email: "finance"),
        new("HR",         "Human Resources",      "EXEC", OrgUnitKind.Function, HrJobs,
            Description: "Talent acquisition, learning & development, total rewards, and employee relations — {Company} people function",
            BudgetMin: 1_000_000m, BudgetMax: 2_000_000m,
            CostCenter: "CC-HR-001",
            Purpose: "Attract, develop, and retain the talent {Company} needs to execute its strategy; champion the {Company} culture",
            Phone: "+233 30 222 3700", Email: "hr")
    ];

    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution =
        new Dictionary<string, double>
        {
            ["EXEC"]       = 0.05,
            ["RETAIL"]     = 0.35,
            ["CORPORATE"]  = 0.15,
            ["INVESTMENT"] = 0.08,
            ["OPS"]        = 0.15,
            ["RISK"]       = 0.08,
            ["TREASURY"]   = 0.04,
            ["IT"]         = 0.05,
            ["FINANCE"]    = 0.03,
            ["HR"]         = 0.02
        };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",         "Executive",     null,   OrgUnitKind.Executive, ExecJobs),
        new("MICROFINANCE", "Microfinance",  "EXEC", OrgUnitKind.Function,  RetailJobs),
        new("PROGRAMS",     "Programs",      "EXEC", OrgUnitKind.Function,  CorpJobs),
        new("OPS",          "Operations",    "EXEC", OrgUnitKind.Function,  OpsJobs),
        new("HR",           "HR & Admin",    "EXEC", OrgUnitKind.Function,  HrJobs)
    ];

    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution =
        new Dictionary<string, double>
        {
            ["EXEC"]         = 0.10,
            ["MICROFINANCE"] = 0.40,
            ["PROGRAMS"]     = 0.30,
            ["OPS"]          = 0.15,
            ["HR"]           = 0.05
        };

    
    // Auto-generated from calbank-job-titles-fresh.xlsx — static role catalogue.
    // Names, codes, descriptions, responsibilities, KPIs are IDENTICAL across every
    // company seeded with this profile. {Company} placeholder is substituted with the
    // actual company name at row-emit time so the same description reads naturally for
    // any tenant (e.g. "Chairs the Acme Bank Board of Directors" / "Chairs the Cal Bank Board").
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        new("BC-001", "Board Chairman", 5, 50000m, 85000m, "EXEC", null, true, "Master's Degree", 20, "Corporate Governance, Strategic Leadership, Risk Oversight, Stakeholder Management", "Chairs the {Company} Board of Directors and provides governance leadership", "EX-0", "Chair board and committee meetings; oversee MD performance; represent {Company} shareholders; set governance tone", "Board effectiveness rating, Shareholder returns, Governance compliance score, Regulatory relationship quality", 30),
        new("NED-001", "Non-Executive Director", 5, 20000m, 42000m, "EXEC", null, false, "Master's Degree", 15, "Corporate Governance, Risk Management, Banking Oversight, Independent Judgment", "Independent director providing strategic oversight and challenge to {Company} management", "EX-0", "Attend board meetings; serve on board sub-committees; provide independent challenge on strategy; protect minority shareholder interests", "Board attendance rate, Committee contribution quality, Independence compliance, Value-add to deliberations", 30),
        new("BS-001", "Board Secretary", 4, 9000m, 16000m, "EXEC", null, false, "LLB / ICSA", 8, "Company Secretarial Practice, Corporate Governance, Minute-Taking, GSE Regulations, BOG Guidelines", "Manages {Company} board governance, minutes, and GSE statutory compliance", "P8", "Prepare board agendas and papers; record minutes; maintain statutory registers; manage GSE filing obligations; advise board on governance", "Board paper timeliness, Statutory register accuracy, GSE compliance rating, Board satisfaction score", 21),
        new("MD-001", "Managing Director & CEO", 5, 65000m, 110000m, "EXEC", null, true, "Master's Degree", 20, "Executive Leadership, Banking Strategy, P&L Management, Regulatory Relations, Investor Relations", "Chief executive accountable for {Company}'s overall strategy, performance, and stakeholder relations", "EX-1", "Set and execute {Company} strategy; lead executive management committee; manage BOG and GSE relationships; drive profitability and sustainable growth", "Return on Equity, Net Profit After Tax, Market Share, Capital Adequacy Ratio, Staff Engagement Score", 30),
        new("DMD-001", "Deputy Managing Director", 5, 50000m, 82000m, "EXEC", null, true, "Master's Degree", 18, "Banking Operations, Strategic Execution, Business Development, People Leadership", "Second-in-command supporting the MD and deputising in their absence", "EX-2", "Support MD in strategy execution; oversee assigned business units; manage executive management committee agenda; represent {Company} at key events", "Business unit performance, Strategic initiative delivery, Executive committee effectiveness, Staff engagement scores", 30),
        new("EA-001", "Executive Assistant to MD", 3, 5000m, 9000m, "EXEC", null, false, "Bachelor's Degree", 4, "Executive Coordination, Confidentiality, Stakeholder Management, Office Management, Communication", "Senior support managing the MD's office, diary, and executive coordination", "P5", "Manage MD calendar and travel; coordinate executive meetings; prepare briefing documents; handle sensitive correspondence; liaise with board", "Schedule efficiency, Briefing document quality, Stakeholder satisfaction, Confidentiality compliance", 21),
        new("CFO-001", "Chief Finance Officer", 5, 42000m, 72000m, "FINANCE", null, true, "Master's Degree / ACCA / ICAG", 15, "IFRS, Capital Management, Financial Reporting, BOG Prudential Returns, Budgeting, Investor Relations", "Executive responsible for {Company} financial management, reporting, and capital strategy", "EX-2", "Oversee financial statements; manage capital adequacy; lead budget cycle; engage BOG on prudential matters; support IR activities; chair ALCO", "CAR, Cost-to-Income Ratio, Reporting Accuracy, Budget Variance, Audit Opinion, ALCO Compliance", 30),
        new("CRO-001", "Chief Risk Officer", 5, 40000m, 68000m, "RISK", null, true, "Master's Degree", 15, "Enterprise Risk Management, Basel III, Credit Risk, Market Risk, Operational Risk, ICAAP, Stress Testing", "Executive accountable for {Company} enterprise-wide risk management and risk appetite", "EX-2", "Own ERM framework; chair risk management committee; set risk appetite; oversee all risk pillars; lead ICAAP and stress testing; report to Board Risk Committee", "NPL Ratio, Risk-Weighted Asset Efficiency, Operational Loss Rate, ICAAP Rating, Regulatory Risk Rating", 30),
        new("CIO-001", "Chief Information Officer", 5, 40000m, 68000m, "IT", null, true, "Master's Degree", 15, "IT Strategy, Temenos T24, Cloud Architecture, Cybersecurity, Digital Transformation, Vendor Management", "Executive leading {Company} technology strategy, digital transformation, and cybersecurity", "EX-2", "Define {Company} IT roadmap; oversee Temenos T24; ensure cyber resilience; manage IT budget; lead digital product delivery; engage fintech partners", "System Uptime, IT Project Delivery Rate, Cyber Incident Rate, Technology Cost Ratio, Digital Revenue Growth", 30),
        new("CHRO-001", "Chief Human Resources Officer", 5, 38000m, 64000m, "HR", null, true, "Master's Degree", 15, "HR Strategy, Talent Management, Organisational Design, Labour Law, Total Rewards, Succession Planning", "Executive leading {Company} people strategy, talent management, and organisational development", "EX-2", "Lead HR strategy; drive talent acquisition and retention; manage total compensation; champion {Company} culture; oversee {Company} Academy", "Staff Turnover Rate, Employee Engagement Score, Time-to-Hire, Succession Coverage Ratio, Training ROI", 30),
        new("COO-001", "Chief Operations Officer", 5, 40000m, 68000m, "OPS", null, true, "Master's Degree", 15, "Banking Operations, Process Automation, Payments Infrastructure, SWIFT, GHIPSS, Change Management", "Executive overseeing all {Company} banking operations, process efficiency, and service delivery", "EX-2", "Oversee end-to-end {Company} operations; drive process automation; manage operational risk; ensure SLA compliance; lead operations transformation", "Transaction Processing Accuracy, Settlement Rate, Operational Loss Incidents, Process Efficiency Gains, SLA Compliance", 30),
        new("HSP-001", "Head of Strategy & Planning", 4, 13000m, 22000m, "EXEC", null, true, "Master's Degree", 10, "Strategic Planning, Financial Modelling, Balanced Scorecard, Business Analysis, Executive Reporting", "Leads {Company} corporate planning, performance management, and strategic initiative delivery", "P8", "Coordinate annual strategic plan; manage corporate scorecard; facilitate strategy reviews; lead special projects for the MD; produce board strategy papers", "Strategy plan completion, KPI reporting timeliness, Initiative delivery rate, Board paper quality, MD satisfaction", 21),
        new("SRA-001", "Strategy & Research Analyst", 3, 3500m, 6500m, "EXEC", null, false, "Bachelor's Degree", 3, "Market Research, Financial Analysis, PowerPoint, Data Visualisation, Competitive Intelligence", "Provides market intelligence, competitor analysis, and strategic research to support {Company} leadership", "P4", "Conduct banking industry and competitor research; prepare strategic presentations; analyse Ghana banking market trends; support strategy review sessions", "Research report quality, Turnaround time, Executive feedback, Insight accuracy, Publication timeliness", 21),
        new("IRO-001", "Investor Relations Officer", 3, 4000m, 7000m, "EXEC", null, false, "Bachelor's Degree", 4, "Investor Relations, GSE Regulations, Financial Reporting, Communication, Annual Reports, Stakeholder Management", "Manages {Company}'s GSE investor communications, analyst briefings, and annual report production", "P5", "Manage investor queries; coordinate analyst briefings; prepare GSE disclosures; support annual report production; maintain shareholder register", "GSE filing compliance, Investor satisfaction, Disclosure timeliness, Annual report quality, Analyst coverage quality", 21),
        new("DD-RBD-001", "Divisional Director, Retail Banking", 5, 24000m, 42000m, "RETAIL", null, true, "Master's Degree", 15, "Retail Banking, Product Management, Branch Network Management, Consumer Lending, Business Development", "Leads {Company} retail banking strategy, branch network performance, and consumer product development", "EX-3", "Drive retail deposit and loan targets; manage {Company} branch network; develop consumer products; oversee customer acquisition; manage retail P&L", "Retail Deposit Growth, Consumer Loan Book, Branch Network Revenue, Customer Acquisition Numbers, NPS Score", 25),
        new("BM-001", "Branch Manager", 4, 9000m, 17000m, "RETAIL", null, true, "Bachelor's Degree", 8, "Branch Operations, Business Development, Credit Assessment, Team Leadership, Customer Relationship Management", "Manages all commercial, operational, and people activities at a {Company} branch", "P7", "Drive branch deposit and loan targets; manage branch staff; ensure compliance; resolve customer escalations; maintain operational standards; submit branch reports", "Branch Deposit Target, Loan Disbursements, Customer Satisfaction Score, Compliance Rating, Staff Productivity Index", 21),
        new("ABM-001", "Assistant Branch Manager", 4, 5500m, 9500m, "RETAIL", null, true, "Bachelor's Degree", 5, "Branch Operations, Sales Management, Customer Service, Credit, Staff Supervision", "Supports the Branch Manager and leads branch operations and sales in their absence", "P6", "Support branch manager; supervise operations and sales teams; handle customer escalations; drive sales targets; manage branch in BM's absence", "Sales target achievement, Operations accuracy rate, Customer satisfaction, Staff supervision effectiveness", 21),
        new("BOS-001", "Branch Operations Supervisor", 4, 4000m, 7500m, "RETAIL", null, true, "Bachelor's Degree", 4, "Teller Supervision, Cash Management, Vault Operations, Core Banking T24, Compliance", "Supervises teller operations, cash management, and back-office activities at the branch", "P5", "Supervise tellers; manage vault and cash positions; authorise over-limit transactions; ensure branch compliance; produce daily operations reports", "Teller Error Rate, Cash Balancing Accuracy, Queue Management Time, Branch Compliance Score", 21),
        new("RO-RET-001", "Relationship Officer, Retail", 3, 3000m, 5500m, "RETAIL", null, false, "Bachelor's Degree", 2, "Customer Relationship Management, Sales, Cal Bank Products, Cross-Selling, Communication", "Manages {Company} retail customer relationships and drives product cross-selling and acquisition", "P4", "Onboard new retail customers; cross-sell savings, loans, and {Company} insurance products; manage existing portfolio; achieve monthly sales targets", "New Accounts Opened, Products Per Customer, Deposit Mobilisation Target, Loan Applications Submitted", 21),
        new("TLR-001", "Teller / Customer Service Representative", 2, 1800m, 3200m, "RETAIL", null, false, "HND / Diploma", 0, "Cash Handling, Customer Service, Temenos T24, Accuracy, Attention to Detail", "Frontline role processing customer transactions and handling service requests at {Company} branches", "P2", "Process deposits, withdrawals, and transfers; handle customer enquiries; balance till daily; identify and escalate suspicious transactions; promote {Company} products", "Transaction Accuracy Rate, Queue Service Time, Daily Balancing Record, Customer Satisfaction Score", 15),
        new("DD-CIB-001", "Divisional Director, Corporate Banking", 5, 26000m, 46000m, "CORPORATE", null, true, "Master's Degree", 15, "Corporate Finance, Capital Markets, Client Management, Credit Structuring, Syndications", "Leads {Company} corporate client coverage, deal origination, and institutional banking revenue", "EX-3", "Manage top-tier corporate and institutional client relationships; structure complex credit facilities; lead deal teams; drive fee and non-funded income growth", "Corporate Loan Book, Fee Income, Client Retention Rate, NPL Ratio, Deal Pipeline Value, Wallet Share", 25),
        new("SRM-001", "Senior Relationship Manager, Corporate", 4, 15000m, 25000m, "CORPORATE", null, true, "Master's Degree", 10, "Corporate Banking, Credit Analysis, Financial Modelling, Negotiation, Syndications, Trade Finance", "Manages a portfolio of large corporate clients and originates high-value deals for {Company}", "P9", "Manage assigned corporate portfolio; originate new deals; coordinate credit approvals; cross-sell treasury and trade products; manage client profitability", "Portfolio Revenue, New Deal Origination, Wallet Share Growth, Credit Quality Score, Cross-Sell Ratio", 21),
        new("RM-CIB-001", "Relationship Manager, Corporate", 4, 9000m, 15000m, "CORPORATE", null, false, "Bachelor's Degree", 5, "Corporate Banking, Credit Analysis, Financial Statement Analysis, Client Management, Proposal Writing", "Manages mid-to-large corporate client relationships and credit facilities at {Company}", "P6", "Maintain corporate client relationships; prepare credit proposals; monitor facility utilisation; grow revenue from assigned portfolio; submit call reports", "Revenue from Portfolio, Loan Book Growth, Portfolio NPL, Cross-Sell Ratio, Client Satisfaction Score", 21),
        new("IBA-001", "Investment Banking Analyst", 3, 4500m, 8000m, "CORPORATE", null, false, "Bachelor's Degree", 2, "Financial Modelling, DCF Valuation, PowerPoint, Deal Documentation, Capital Markets Analysis", "Supports deal structuring, financial modelling, and transaction execution for {Company} corporate clients", "P5", "Build financial models; prepare pitch books; conduct due diligence; support deal execution and documentation; analyse market comparables", "Model accuracy, Pitch book quality, Deal execution support rating, Turnaround time, Research quality", 21),
        new("DD-PSD-001", "Divisional Director, Public Sector Banking", 5, 22000m, 40000m, "CORPORATE", null, true, "Master's Degree", 12, "Public Sector Banking, Government Relations, Development Finance, Credit, Stakeholder Management", "Leads {Company} government banking, MDA relationships, and development finance portfolio", "EX-3", "Develop and manage government and MDA client relationships; originate public sector deals; manage development finance partnerships; drive public sector revenue", "Public Sector Loan Book, MDA Deposit Base, Government Fee Income, Portfolio Quality, New MDA Relationships", 25),
        new("RM-PSD-001", "Public Sector Relationship Manager", 4, 8000m, 14000m, "CORPORATE", null, false, "Bachelor's Degree", 5, "Public Sector Banking, Government Procurement, Credit, Relationship Management, BOG Regulations", "Manages {Company} relationships with government agencies, statutory bodies, and MDAs", "P6", "Manage assigned MDA and government client portfolio; process salary advance facilities; grow public sector deposits; coordinate government payroll banking", "Public Sector Revenue, New MDA Accounts, Government Salary Accounts, Deposit Mobilisation, NPL", 21),
        new("DD-SME-001", "Divisional Director, SME Banking", 5, 22000m, 38000m, "CORPORATE", null, true, "Master's Degree", 12, "SME Banking, Business Development, Credit, Product Design, Team Leadership", "Leads {Company} SME banking strategy, product development, and portfolio growth", "EX-3", "Grow SME loan and deposit portfolio; design SME-specific products; build SME ecosystem partnerships; manage SME relationship team; drive SME revenue", "SME Loan Book, SME Deposit Base, SME Fee Income, NPL Ratio, Number of Active SME Clients", 25),
        new("RO-SME-001", "SME Relationship Officer", 3, 3200m, 5800m, "CORPORATE", null, false, "Bachelor's Degree", 2, "SME Lending, Business Development, Financial Analysis, Customer Relationship Management, Credit Assessment", "Manages a portfolio of SME clients and drives loan and deposit growth for {Company}", "P4", "Acquire and retain SME clients; process SME loan applications; monitor repayments; cross-sell {Company} products; achieve monthly targets; submit call reports", "New SME Clients Acquired, Loan Disbursement Volume, Portfolio Quality, Deposit Mobilisation Target", 21),
        new("BDO-SME-001", "SME Business Development Officer", 3, 3200m, 5800m, "CORPORATE", null, false, "Bachelor's Degree", 2, "Sales, SME Market Knowledge, Prospecting, Presentation Skills, Networking", "Focuses on acquiring new SME clients and growing {Company}'s SME market share", "P4", "Identify and pitch to prospective SME clients; organise SME outreach events; manage referral pipeline; onboard new SME accounts; represent {Company} at business events", "New Client Acquisition, Pipeline Conversion Rate, Referral Numbers Generated, Onboarding Satisfaction", 21),
        new("DD-PWM-001", "Divisional Director, Private Banking", 5, 24000m, 44000m, "INVESTMENT", null, true, "Master's Degree / CFA / CFP", 15, "Wealth Management, Portfolio Management, HNW Client Relations, Investment Advisory, Estate Planning", "Leads {Company} private banking and wealth management strategy for HNW clients", "EX-3", "Manage HNW client relationships; oversee investment portfolio services; drive AUM growth; develop exclusive product offerings; manage private banking team", "AUM Growth, Revenue per HNW Client, Client Retention Rate, New HNW Acquisitions, Investment Returns", 25),
        new("PB-001", "Private Banker / Wealth Advisor", 4, 10000m, 18000m, "INVESTMENT", null, false, "Bachelor's Degree / CFA", 7, "Investment Advisory, Portfolio Management, Tax Planning, Client Relationship Management, Financial Planning", "Provides bespoke banking and investment advisory to {Company} HNW clients", "P7", "Manage assigned HNW client book; provide personalised investment and financial planning advice; coordinate product specialists; grow AUM and wallet share", "AUM per Client, Revenue per Client, Client Satisfaction Score, Wallet Share Growth, Retention Rate", 21),
        new("HT-001", "Head of Treasury", 4, 17000m, 30000m, "TREASURY", null, true, "Master's Degree / ACI Dealing Certificate", 12, "Treasury Management, Forex, Fixed Income, Liquidity Management, ALM, Bloomberg, ALCO", "Leads {Company} treasury operations including liquidity, FX, fixed income, and ALM", "P9", "Manage {Company} liquidity; oversee FX dealing desk; manage T-bill and bond portfolio; ensure BOG cash reserve compliance; lead ALCO secretariat", "Net Interest Margin, FX Trading Revenue, Liquidity Coverage Ratio, Investment Yield, ALCO Compliance Rate", 25),
        new("STD-001", "Senior Treasury Dealer", 4, 9000m, 16000m, "TREASURY", null, false, "Bachelor's Degree / ACI Dealing Certificate", 7, "FX Trading, Fixed Income, Bloomberg Terminal, Risk Limits Management, Interbank Markets", "Senior FX and fixed income dealer executing large transactions and mentoring junior dealers", "P7", "Execute large FX spot and forward transactions; manage dealer book; mentor junior dealers; monitor market risk limits; produce daily P&L reports", "Trading Revenue, Position Accuracy, Limit Compliance, P&L Reporting Timeliness, Junior Dealer Mentoring", 21),
        new("TD-001", "Treasury Dealer", 3, 5500m, 10000m, "TREASURY", null, false, "Bachelor's Degree", 4, "FX Trading, Money Markets, Bloomberg, Financial Markets, Deal Confirmation, Risk Awareness", "Executes FX, money market, and T-bill transactions for {Company}", "P5", "Execute FX spot and forward deals; trade T-bills and bonds; confirm interbank placements; monitor open positions; report daily P&L to Head of Treasury", "Trading Revenue, Deal Ticket Accuracy, Limit Compliance, P&L Reporting Timeliness, Settlement Rate", 21),
        new("TOO-001", "Treasury Operations Officer", 3, 3000m, 5500m, "TREASURY", null, false, "Bachelor's Degree", 2, "SWIFT, Nostro Reconciliation, Deal Settlement, Temenos T24, Attention to Detail", "Back-office settlement, nostro reconciliation, and SWIFT processing for {Company} treasury deals", "P3", "Confirm and settle FX and money market deals; reconcile nostro accounts; process SWIFT MT messages; maintain deal records; resolve settlement failures", "Settlement Accuracy Rate, Nostro Reconciliation Timeliness, SWIFT Error Rate, Failed Trade Rate", 21),
        new("HF-001", "Head of Finance", 4, 13000m, 22000m, "FINANCE", null, true, "Master's Degree / ACCA / ICAG", 10, "IFRS, Financial Reporting, Management Accounting, Temenos T24, Budgeting, BOG Returns", "Leads {Company} finance team responsible for financial statements, reporting, and budgets", "P8", "Oversee preparation of {Company} financial statements; lead budget process; manage month-end close; produce management accounts; liaise with external auditors", "Reporting Timeliness, Audit Findings, Budget Accuracy, BOG Returns Compliance, Cost-to-Income Ratio", 21),
        new("FRM-001", "Financial Reporting Manager", 4, 8000m, 14000m, "EXEC", null, true, "Bachelor's Degree / ACCA / ICAG", 6, "IFRS 9, BOG Prudential Returns, Financial Statements, Consolidation, ERP Systems, Audit Coordination", "Manages {Company} IFRS financial statements, BOG prudential returns, and external audit process", "P6", "Prepare IFRS financial statements; submit BOG prudential returns; manage IFRS 9 provisioning; coordinate external audit; prepare board finance papers", "Submission Compliance Rate, Audit Adjustments, IFRS Compliance, BOG Return Accuracy, Report Timeliness", 21),
        new("FA-001", "Financial Accountant", 3, 3500m, 6200m, "FINANCE", null, false, "Bachelor's Degree / Part ACCA", 3, "Bookkeeping, IFRS, Excel, Temenos T24, General Ledger Reconciliation, Journal Entries", "Prepares {Company} financial records, reconciliations, and supports reporting processes", "P4", "Post journal entries; reconcile GL accounts; prepare trial balance; assist with IFRS disclosures; process intercompany transactions; support month-end close", "Reconciliation Accuracy, Journal Posting Turnaround, Error Rate, Month-End Deadline Compliance", 21),
        new("BPA-001", "Budget & Planning Analyst", 3, 3500m, 6200m, "EXEC", null, false, "Bachelor's Degree", 3, "Budgeting, Financial Modelling, Excel, Variance Analysis, Management Reporting, Cost Analysis", "Supports {Company} budgeting, forecasting, and management reporting", "P4", "Coordinate departmental budget submissions; prepare monthly variance reports; support quarterly reforecasting; analyse cost drivers; produce management reports", "Budget Submission Timeliness, Variance Analysis Quality, Forecast Accuracy, Management Report Deadlines", 21),
        new("HCR-001", "Head of Credit", 4, 15000m, 25000m, "RISK", null, true, "Master's Degree", 12, "Credit Risk, Loan Underwriting, IFRS 9, Portfolio Management, Credit Policy, NPL Management", "Leads {Company} credit underwriting, policy, portfolio management, and NPL reduction", "P9", "Chair credit committee; set credit policies; manage NPL reduction programme; ensure IFRS 9 provisioning accuracy; oversee loan administration processes", "NPL Ratio, Credit Approval Turnaround, Provisioning Accuracy, Loan Book Quality, Policy Compliance", 21),
        new("CRM-001", "Credit Risk Manager", 4, 8000m, 14000m, "RISK", null, true, "Bachelor's Degree", 6, "Credit Assessment, Portfolio Monitoring, Risk Grading, Financial Analysis, IFRS 9, Credit Reporting", "Manages {Company} credit assessment processes and portfolio quality monitoring", "P6", "Review credit applications; monitor watchlist accounts; prepare portfolio quality reports; manage credit risk grading system; support ALCO and board reporting", "Credit Memo Quality, Portfolio Reporting Timeliness, Watchlist Management, Risk Grade Accuracy", 21),
        new("CA-001", "Credit Analyst", 3, 3500m, 6200m, "RISK", null, false, "Bachelor's Degree", 2, "Financial Analysis, Credit Assessment, Report Writing, Excel Modelling, Industry Analysis", "Analyses {Company} credit requests and prepares detailed credit memoranda for approval", "P4", "Evaluate loan applications; analyse financial statements; prepare credit memos; monitor borrower covenants; maintain credit files; check securities documentation", "Credit Memo Turnaround Time, Analysis Accuracy, NPL Prediction Rate, Covenant Monitoring Compliance", 21),
        new("LRO-001", "Loan Recovery Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "Debt Recovery, Negotiation, Credit Law, Collateral Realisation, Communication", "Manages recovery of {Company} non-performing loans through negotiation and legal enforcement", "P3", "Contact and negotiate with delinquent borrowers; initiate legal proceedings; manage collateral valuation and disposal; track recovery progress; file court proceedings", "Recovery Rate, NPL Reduction Achieved, Legal Case Outcomes, Repayment Arrangement Compliance", 21),
        new("HC-001", "Head of Compliance", 4, 13000m, 22000m, "RISK", null, true, "Master's Degree / LLB", 10, "Regulatory Compliance, AML/CFT, BOG Regulations, Policy Development, Compliance Training, FIC Liaison", "Leads {Company} compliance function ensuring adherence to all BOG regulatory obligations", "P8", "Maintain {Company} compliance programme; liaise with BOG and FIC; manage regulatory submissions; conduct compliance training; oversee KYC/CDD framework", "Regulatory Penalty Rate, Compliance Training Completion, STR Filing Rate, BOG Examination Rating", 21),
        new("CO-001", "Compliance Officer", 3, 3500m, 6200m, "RISK", null, false, "Bachelor's Degree", 3, "AML/CFT, KYC, Regulatory Reporting, Policy Review, BOG Guidelines, FATF Standards", "Monitors {Company} regulatory compliance and conducts periodic compliance reviews", "P4", "Conduct compliance reviews; monitor transactions for suspicious activity; prepare regulatory reports; update compliance registers; support staff compliance training", "Review Completion Rate, Regulatory Report Timeliness, Issue Escalation Effectiveness, Compliance Register Currency", 21),
        new("AMLA-001", "AML / CFT Analyst", 3, 3500m, 6200m, "EXEC", null, false, "Bachelor's Degree", 2, "Transaction Monitoring, AML Software, SAR/STR Filing, Financial Crime Investigation, FATF 40 Recommendations", "Monitors {Company} transactions for money laundering and suspicious activity and files STRs to FIC", "P4", "Review AML system alerts; investigate suspicious transactions; file STRs to FIC; conduct PEP and sanctions screening; maintain AML case records", "Alert Review Turnaround, STR Filing Accuracy, False Positive Rate, Case Investigation Closure Rate", 21),
        new("KYC-001", "KYC / CDD Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "KYC, CDD, EDD, PEP Screening, Sanctions Lists, BOG AML Guidelines, Document Verification", "Conducts customer due diligence and enhanced due diligence for {Company} customer onboarding and periodic reviews", "P3", "Verify customer identity documents; conduct CDD and EDD for high-risk clients; screen PEP and sanctions lists; maintain KYC records; support periodic CDD reviews", "KYC Accuracy Rate, CDD Completion Timeliness, EDD Quality Score, Periodic Review Completion Rate", 21),
        new("CIA-001", "Chief Internal Auditor", 5, 20000m, 34000m, "RISK", null, true, "Master's Degree / CIA / ACCA", 12, "Risk-Based Auditing, IIA Standards, Banking Audit, Report Writing, Board Engagement", "Leads {Company} internal audit function reporting independently to the Board Audit Committee", "EX-3", "Own annual risk-based audit plan; lead key audit engagements; report to Board Audit & Risk Committee; track management action implementation", "Audit Plan Completion Rate, High-Risk Findings Closure, Repeat Audit Findings, Board Audit Committee Satisfaction", 25),
        new("SIA-001", "Senior Internal Auditor", 4, 7000m, 12000m, "RISK", null, false, "Bachelor's Degree / ACCA / CIA", 5, "Audit Methodology, Data Analytics, Temenos T24 Audit, IFRS, Report Writing, Control Testing", "Leads {Company} audit assignments, coaches junior auditors, and drafts high-quality audit reports", "P6", "Plan and execute audit fieldwork; document findings; review junior auditor work; draft audit reports; follow up on prior-period management actions", "Report Quality Score, Fieldwork Timeliness, Finding Significance, Recommendation Uptake Rate", 21),
        new("IA-001", "Internal Auditor", 3, 3000m, 5500m, "RISK", null, false, "Bachelor's Degree", 2, "Audit Techniques, Excel, Working Papers, Control Testing, Risk Assessment", "Executes {Company} audit procedures, documents working papers, and identifies control deficiencies", "P3", "Perform control testing; document audit working papers; identify control weaknesses; assist in preparing audit findings and management letters", "Working Paper Quality, Testing Completeness, Deadline Adherence, Finding Relevance Score", 21),
        new("HRM-001", "HR Manager", 4, 9000m, 16000m, "HR", null, true, "Bachelor's Degree / CIHRM / SHRM", 7, "HR Operations, Recruitment, Performance Management, Labour Law, HRIS, Compensation & Benefits", "Manages {Company} day-to-day HR operations, recruitment, and employee relations", "P7", "Manage end-to-end recruitment; administer payroll inputs; coordinate performance reviews; handle employee relations; maintain {Company} HRIS; support CHRO", "Vacancy Fill Time, Onboarding Satisfaction, Payroll Accuracy, Grievance Resolution Time, Staff Turnover", 21),
        new("RTAO-001", "Recruitment & Talent Acquisition Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "Recruitment, Employer Branding, Interviewing, ATS Systems, Onboarding, Talent Sourcing", "Manages {Company} end-to-end recruitment process from job posting to onboarding", "P3", "Post job adverts; screen applications; coordinate interviews; manage offer process; conduct onboarding orientation; manage {Company} talent pipeline", "Time-to-Hire, Offer Acceptance Rate, New Hire 90-Day Retention, Onboarding NPS, Job Board Spend Efficiency", 21),
        new("PBO-001", "Payroll & Benefits Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "Payroll Processing, SSNIT, Tier 2 Pension, Tax Computation, HRIS, Confidentiality", "Processes {Company} monthly payroll, SSNIT contributions, and administers staff benefits", "P3", "Process monthly {Company} payroll; administer SSNIT and pension contributions; manage medical and group life insurance; resolve staff payslip queries", "Payroll Accuracy Rate, Processing Timeliness, Statutory Compliance Rate, Query Resolution Time", 21),
        new("LDO-001", "Learning & Development Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "Training Coordination, LMS Administration, Needs Assessment, Facilitation, Learning Design", "Coordinates {Company} training programmes, {Company} Academy, and e-learning platforms", "P3", "Coordinate internal and external training logistics; manage LMS platform; conduct training needs analysis; track training hours; evaluate learning impact", "Training Hours per Employee, Completion Rate, Training Satisfaction Score, Learning ROI, Certification Achievement", 21),
        new("ITM-001", "IT Manager", 4, 12000m, 20000m, "IT", null, true, "Bachelor's Degree", 8, "IT Management, Temenos T24, Project Management, ITIL, Vendor Management, Infrastructure", "Manages {Company} IT operations, T24 administration, projects, and vendor relationships", "P7", "Manage IT team; oversee T24 operations; manage vendors and SLAs; ensure system availability; coordinate change management; produce IT performance reports", "System Uptime, Project Delivery Rate, Vendor SLA Compliance, IT Budget Variance, Change Success Rate", 21),
        new("CBSA-001", "Core Banking Systems Administrator", 4, 7000m, 12000m, "IT", null, false, "Bachelor's Degree", 5, "Temenos T24, SQL, System Administration, ITIL, EOD/EOM Processing, User Access Management", "Administers and maintains {Company}'s Temenos T24 core banking platform", "P6", "Maintain T24 application; manage user access; apply system patches; support EOD/EOM processing; troubleshoot system incidents; liaise with Temenos support", "T24 Availability %, EOD Processing Success Rate, Incident Resolution Time, Patch Compliance Rate", 21),
        new("CSA-001", "Cybersecurity Analyst", 3, 5000m, 9000m, "IT", null, false, "Bachelor's Degree", 3, "SIEM, Threat Intelligence, Incident Response, Network Security, Vulnerability Management, ISO 27001", "Monitors {Company} security threats, responds to incidents, and enforces security policies", "P5", "Monitor SIEM dashboards; investigate security alerts; conduct vulnerability assessments; enforce {Company} security policies; produce weekly security reports", "Mean Time to Detect, Mean Time to Respond, Vulnerabilities Remediated, Security Awareness Completion Rate", 21),
        new("SWD-001", "Software Developer", 3, 5500m, 10000m, "IT", null, false, "Bachelor's Degree", 3, "Java / .NET / Python, REST APIs, SQL, Git, Agile, T24 Integration, Microservices", "Designs and builds {Company} digital banking applications, APIs, and T24 integrations", "P5", "Build Cal Mobile and digital banking features; maintain APIs; conduct code reviews; fix production bugs; participate in Agile sprint ceremonies; document technical specs", "Feature Delivery Velocity, Bug Escape Rate, Code Quality Score, API Uptime, Sprint Completion Rate", 21),
        new("DE-001", "Data Engineer", 3, 5500m, 10000m, "EXEC", null, false, "Bachelor's Degree", 3, "SQL, Python, ETL, Data Warehousing, Power BI / Tableau, Cloud Data Platforms", "Builds and maintains {Company} data pipelines, data warehouse, and BI dashboards", "P5", "Design and maintain ETL pipelines; manage data warehouse; build BI dashboards; ensure data quality; support analytics team; produce management reports", "Pipeline Uptime, Data Freshness, Dashboard Adoption Rate, Data Quality Score, Incident Resolution Time", 21),
        new("ITSD-001", "IT Service Desk Officer", 2, 2000m, 3500m, "IT", null, false, "HND / Diploma", 1, "IT Support, Ticketing Systems, Windows, Active Directory, Network Basics, T24 User Support", "Provides first-line IT support and resolves user incidents for all {Company} staff", "P2", "Log and resolve IT incidents; manage user account requests; escalate unresolved tickets; conduct routine hardware checks; document resolutions in knowledge base", "First Call Resolution Rate, Ticket Closure Time, User Satisfaction Score, SLA Compliance", 15),
        new("HDB-001", "Head of Digital Banking", 4, 15000m, 26000m, "IT", null, true, "Master's Degree", 10, "Digital Product Management, Mobile Banking, Open Banking, API Strategy, Fintech Partnerships, Agile", "Leads {Company} digital product strategy, Cal Mobile, and fintech partnership programme", "P8", "Define {Company} digital roadmap; manage Cal Mobile and USSD channels; build fintech ecosystem; drive digital revenue; report digital KPIs to ExCo", "Active Cal Mobile Users, Digital Transaction Volume, Digital Revenue, App Store Rating, Feature Release Cadence", 21),
        new("MMPM-001", "Mobile Money & Payments Manager", 4, 8000m, 14000m, "EXEC", null, true, "Bachelor's Degree", 5, "Mobile Money, GhIPSS, USSD, Payment Interoperability, Product Management, Fintech Regulations", "Manages {Company} MoMo, GhIPSS, and digital payment rails strategy and daily operations", "P6", "Manage MoMo and GhIPSS integrations; drive digital payment adoption; track transaction volumes; manage payment scheme relationships; ensure uptime SLAs", "Mobile Money Transaction Volume, Active Wallet Users, Uptime SLA, Revenue from Digital Payments", 21),
        new("CECM-001", "Cards & E-Channels Manager", 4, 8000m, 14000m, "EXEC", null, true, "Bachelor's Degree", 5, "Card Management, ATM Operations, POS Acquiring, Internet Banking, Scheme Management, Chargeback Management", "Manages {Company} debit/credit card portfolio, ATM network, POS acquiring, and Cal Online", "P6", "Manage card issuance and lifecycle; oversee ATM network; drive POS merchant acquiring; handle chargebacks; manage Visa/Mastercard scheme relationships; grow cards revenue", "Active Cards, ATM Uptime %, POS Terminals Deployed, Cards Revenue, Chargeback Resolution Rate", 21),
        new("HO-001", "Head of Operations", 4, 13000m, 22000m, "OPS", null, true, "Bachelor's Degree", 10, "Banking Operations, SWIFT, GHIPSS, Process Improvement, Payments Systems, Team Leadership", "Leads {Company} back-office operations, GHIPSS settlements, SWIFT, and operational excellence", "P8", "Oversee all back-office processing; manage operational risk; ensure same-day settlement; drive process automation; produce operational SLA reports for ExCo", "Transaction Processing Accuracy, Settlement Rate, Operational Loss Incidents, Failed Payment Rate, SLA Compliance", 21),
        new("PSO-001", "Payments & Settlements Officer", 3, 3000m, 5500m, "OPS", null, false, "Bachelor's Degree", 2, "SWIFT, GHIPSS, GhIPSS Instant Pay, Temenos T24, Payment Processing, Reconciliation", "Processes {Company} domestic and international payments, GHIPSS, and SWIFT transactions", "P3", "Process domestic and international payments; reconcile GHIPSS settlement accounts; handle SWIFT MT messages; resolve payment exceptions; report daily settlement position", "Payment Processing Accuracy, Turnaround Time, Reconciliation Exceptions, Failed Transaction Rate", 21),
        new("HLCS-001", "Head of Legal & Company Secretary", 4, 15000m, 25000m, "RISK", null, true, "LLB / BL", 10, "Banking Law, Contract Drafting, Corporate Governance, Litigation Management, GSE Regulations, Company Secretariat", "Leads {Company} legal advisory, contract management, litigation, and board secretariat", "P8", "Provide legal advisory to management and board; manage litigation portfolio; draft and review material contracts; coordinate board meetings; maintain statutory registers", "Contract Turnaround Time, Litigation Win Rate, Board Compliance Score, Legal Risk Incidents, GSE Filing Compliance", 21),
        new("LO-001", "Legal Officer", 3, 4000m, 7000m, "RISK", null, false, "LLB", 2, "Contract Law, Legal Research, Drafting, Banking Law, Communication, Attention to Detail", "Supports {Company} legal operations including contract management, research, and litigation support", "P4", "Draft and review legal documents; conduct legal research; support litigation proceedings; maintain contract register; prepare board papers; file court processes", "Document Turnaround, Legal Accuracy, Contract Compliance Rate, Research Quality, Statutory Filing Timeliness", 21),
        new("HMK-001", "Head of Marketing", 4, 11000m, 19000m, "EXEC", null, true, "Bachelor's Degree", 8, "Marketing Strategy, Brand Management, Digital Marketing, PR, Campaign Management, Media Relations", "Leads {Company} brand strategy, marketing campaigns, and corporate communications", "P7", "Develop {Company} marketing strategy; manage brand identity; lead advertising campaigns; handle media relations; oversee digital marketing channels; manage {Company} social media", "Brand Awareness Score, Customer Acquisition Cost, Campaign ROI, Share of Voice, Social Media Reach", 21),
        new("DMO-001", "Digital Marketing Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "Social Media Marketing, Google Ads, SEO/SEM, Content Creation, Analytics, Canva / Adobe", "Manages {Company} digital channels, social media, SEO, and paid online campaigns", "P3", "Manage {Company} social media accounts; run paid digital campaigns; create content calendar; track digital KPIs; grow {Company} online brand and community", "Social Media Engagement Rate, Digital Leads Generated, Campaign CTR, Follower Growth, Content Quality Score", 21),
        new("CCO-001", "Corporate Communications Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "PR Writing, Media Relations, Crisis Communications, Brand Management, Stakeholder Engagement", "Manages {Company} press releases, media relations, and corporate reputation", "P3", "Draft {Company} press releases and announcements; manage media inquiries; coordinate sponsorships and CSR events; monitor press coverage; manage reputation risks", "Media Coverage Quality, Response Turnaround, Sponsorship ROI, Reputation Index, Crisis Containment Effectiveness", 21),
        new("TFM-M-001", "Trade Finance Manager", 4, 9000m, 16000m, "FINANCE", null, true, "Bachelor's Degree / CDCS", 6, "Letters of Credit, UCP 600, Documentary Collections, Bank Guarantees, SWIFT MT700, Trade Structuring", "Manages {Company} trade finance products, client relationships, and transaction processing", "P6", "Process LCs and documentary collections; advise {Company} clients on trade instruments; manage trade finance revenue; ensure SWIFT MT accuracy; supervise trade officers", "LC Issuance Volume, Trade Finance Income, Document Discrepancy Rate, Client Retention, Processing Turnaround", 21),
        new("TFO-001", "Trade Finance Officer", 3, 3500m, 6200m, "FINANCE", null, false, "Bachelor's Degree", 2, "UCP 600, Documentary Credits, SWIFT MT700/760, Trade Documentation, Reconciliation", "Processes {Company} trade finance transactions including LCs, guarantees, and documentary collections", "P4", "Examine trade documents; process LC presentations; issue bank guarantees; prepare SWIFT messages; reconcile trade finance accounts; resolve document discrepancies", "Document Examination Accuracy, Processing Turnaround, Discrepancy Rate, SWIFT Error Rate, Client Query Resolution", 21),
        new("CXM-001", "Customer Experience Manager", 4, 7000m, 12000m, "EXEC", null, true, "Bachelor's Degree", 5, "Customer Experience, Complaints Management, NPS Methodology, CRM Systems, Service Design, Root Cause Analysis", "Manages {Company} service quality, complaints resolution, and customer satisfaction programmes", "P6", "Own {Company} complaints management process; track NPS and CSAT; run CX improvement initiatives; train frontline staff on service standards; produce monthly CX reports", "NPS Score, Complaint Resolution Time, First Call Resolution Rate, CSAT Score, Repeat Complaint Rate", 21),
        new("CCA-001", "Contact Centre Agent", 2, 1800m, 3200m, "EXEC", null, false, "HND / Diploma", 0, "Customer Service, Active Listening, Cal Bank Products, CRM, Communication, Problem Solving", "Handles inbound {Company} customer calls, digital queries, and service requests across all channels", "P1", "Answer {Company} customer calls and chats; resolve account queries; escalate complex issues; log interactions in CRM; upsell {Company} products where appropriate", "Average Handle Time, First Call Resolution, Customer Satisfaction Score, Upsell Conversion Rate, Attendance Rate", 15),
        new("HER-001", "Head of Enterprise Risk", 4, 14000m, 24000m, "RISK", null, true, "Master's Degree", 10, "Enterprise Risk, Basel III/IV, Operational Risk, Market Risk, ICAAP, Stress Testing, Risk Reporting", "Leads {Company} operational risk, market risk, ICAAP, and ERM framework management", "P8", "Develop and maintain {Company} ERM framework; lead ICAAP and stress testing; produce ALCO and board risk reports; manage risk register; drive risk culture", "Risk Register Coverage, ALCO Report Quality, ICAAP Completion, Stress Test Accuracy, Operational Loss Rate", 21),
        new("RA-001", "Risk Analyst", 3, 3500m, 6200m, "RISK", null, false, "Bachelor's Degree", 2, "Risk Analysis, Excel Modelling, Risk Registers, Operational Risk, Report Writing, Data Analysis", "Analyses {Company} risk exposures, maintains risk registers, and supports risk reporting", "P4", "Maintain risk and control self-assessments; analyse operational loss incidents; support stress testing; prepare risk dashboard; monitor key risk indicators", "RCSA Completion Rate, Loss Incident Reporting Timeliness, KRI Accuracy, Risk Report Quality", 21),
        new("FM-001", "Facilities Manager", 4, 6000m, 10000m, "EXEC", null, true, "Bachelor's Degree", 5, "Facilities Management, Property Management, Security Coordination, Fleet Management, Vendor Supervision", "Manages {Company} premises, security, fleet, and office administration across all locations", "P5", "Manage {Company} office and branch premises; coordinate security and cleaning vendors; oversee fleet; manage utilities; ensure safe and functional work environment", "Facilities Uptime, Premises Compliance Rating, Maintenance Turnaround, Cost per Square Metre, Security Incidents", 21),
        new("PRO-001", "Procurement Officer", 3, 3000m, 5500m, "EXEC", null, false, "Bachelor's Degree", 2, "Procurement, Tender Management, Vendor Evaluation, Contract Administration, PPA Compliance, Negotiation", "Manages {Company} sourcing, vendor evaluation, and contract administration", "P3", "Manage tender processes; evaluate vendor proposals; administer purchase orders; maintain vendor register; ensure PPA compliance; track contract deliverables", "Procurement Cycle Time, Cost Savings Achieved, Vendor SLA Compliance, PPA Compliance Rate, Contract Renewal Timeliness", 21),
        new("GT-001", "Graduate Trainee", 1, 1600m, 2400m, "HR", null, false, "Bachelor's Degree (minimum Second Class Lower)", 0, "Analytical Thinking, Communication, Teamwork, MS Office, Learning Agility, Initiative", "Entry-level rotational programme at {Company} rotating through key departments over 12-18 months", "T1", "Rotate through assigned {Company} departments; complete learning milestones; contribute to team projects; write departmental reports; participate in {Company} Academy mentorship", "Rotation Assessment Scores, Learning Milestone Completion, Supervisor Feedback, Attendance Rate, Academy Participation", 15),
        new("INT-001", "Banking Intern / Apprentice", 1, 700m, 1200m, "HR", null, false, "Current University Student", 0, "Eagerness to Learn, Communication, MS Office, Numeracy, Professionalism, Teamwork", "Short-term placement for university students gaining practical {Company} banking experience", "T0", "Support assigned team with tasks; shadow senior {Company} staff; complete internship assessment; prepare departmental report; attend structured learning sessions", "Supervisor Assessment Score, Task Completion Rate, Attendance, Internship Report Quality, Learning Log Submission", 21),
    ];
    
    // Auto-generated from calbank-stations.xlsx — static station catalogue.
    // Phone numbers + addresses kept verbatim from the reference data; emails store ONLY
    // the local part (before @) so the row factory appends the actual company TLD at runtime.
    private static readonly StationSpec[] _calbankStations =
    [
        new("HO-001", "Head Office - Accra (Independence Avenue)", "Head Office", "Greater Accra", "Accra", "23 Independence Avenue, Ridge", 50, 100, "{Company} main corporate headquarters housing executive, divisional, and support functions", "+233 30 222 3100", "headoffice"),
        new("OTC-001", "Operations & Technology Centre - Accra", "Head Office", "Greater Accra", "Accra", "14 Switchback Road, Cantonments", 50, 100, "{Company} centralised back-office, IT infrastructure, data centre, and T24 operations hub", "+233 30 222 3101", "optech"),
        new("RO-ASH-001", "Ashanti Regional Office - Kumasi", "Regional Office", "Ashanti", "Kumasi", "Prempeh II Street, Adum", 50, 40, "{Company} regional management office coordinating all Ashanti Region branches", "+233 32 202 3100", "ashanti.region"),
        new("RO-WES-001", "Western Regional Office - Takoradi", "Regional Office", "Western", "Takoradi", "Harbour Road, Market Circle", 50, 40, "{Company} regional office overseeing Western and Western North Region branches", "+233 31 202 3100", "western.region"),
        new("RO-NOR-001", "Northern Regional Office - Tamale", "Regional Office", "Northern", "Tamale", "Salaga Road, Central Tamale", 50, 40, "{Company} regional office coordinating Northern, North East, and Savannah Region branches", "+233 37 202 3100", "northern.region"),
        new("RO-VOL-001", "Volta Regional Office - Ho", "Regional Office", "Volta", "Ho", "Ho-Aflao Road, Ho Central", 50, 40, "{Company} regional office overseeing Volta and Oti Region branches", "+233 36 202 3100", "volta.region"),
        new("RO-CEN-001", "Central Regional Office - Cape Coast", "Regional Office", "Central", "Cape Coast", "Commercial Street, Cape Coast", 50, 40, "{Company} regional office managing Central Region and surrounding branches", "+233 33 202 3100", "central.region"),
        new("RO-BON-001", "Bono Regional Office - Sunyani", "Regional Office", "Bono", "Sunyani", "Fiapre Road, Sunyani Central", 50, 40, "{Company} regional office managing Bono, Bono East, and Ahafo Region branches", "+233 35 202 3100", "bono.region"),
        new("RO-UPE-001", "Upper East Regional Office - Bolgatanga", "Regional Office", "Upper East", "Bolgatanga", "Zuarungu Road, Bolgatanga Central", 50, 40, "{Company} regional office covering Upper East and Upper West Region branches", "+233 37 202 3101", "uppereast.region"),
        new("BRN-IND-001", "Independence Avenue Branch", "Branch Office", "Greater Accra", "Accra", "23 Independence Avenue, Ridge", 15, 25, "{Company} flagship branch co-located with HQ on Independence Avenue", "+233 30 222 3600", "independence"),
        new("BRN-OSU-001", "Osu Oxford Street Branch", "Branch Office", "Greater Accra", "Accra", "Oxford Street, Osu", 15, 25, "{Company} branch serving Osu's vibrant business, hospitality, and diplomatic community", "+233 30 222 3601", "osu"),
        new("BRN-APC-001", "Airport City Branch", "Branch Office", "Greater Accra", "Accra", "2 Accra-Tema Motorway, Airport City", 15, 25, "{Company} premium branch for oil companies, airlines, and corporate HQs at Airport City", "+233 30 276 3600", "airportcity"),
        new("BRN-EL-001", "East Legon Branch", "Branch Office", "Greater Accra", "Accra", "Boundary Road, East Legon", 15, 25, "{Company} branch serving the affluent East Legon residential and commercial corridor", "+233 30 222 3603", "eastlegon"),
        new("BRN-ACM-001", "Accra Mall Branch", "Branch Office", "Greater Accra", "Accra", "Accra Mall, Spintex Road", 15, 25, "{Company} in-mall branch capturing high-footfall consumer banking at Accra Mall", "+233 30 222 3604", "accramall"),
        new("BRN-TEM-001", "Tema Community 1 Branch", "Branch Office", "Greater Accra", "Tema", "Community 1, Tema Central", 15, 25, "{Company} main Tema branch serving port operators, industrialists, and the Tema community", "+233 30 322 3600", "tema"),
        new("BRN-SPX-001", "Spintex Road Branch", "Branch Office", "Greater Accra", "Accra", "Spintex Road, Accra", 15, 25, "{Company} branch serving the fast-growing Spintex industrial and residential corridor", "+233 30 222 3605", "spintex"),
        new("BRN-ADB-001", "Adabraka Branch", "Branch Office", "Greater Accra", "Accra", "Asylum Down Road, Adabraka", 15, 25, "{Company} branch serving Adabraka traders, businesses, and surrounding communities", "+233 30 222 3606", "adabraka"),
        new("BRN-MAD-001", "Madina Branch", "Branch Office", "Greater Accra", "Accra", "Madina Market Road, Madina", 15, 25, "{Company} branch serving the busy Madina market traders and residential community", "+233 30 222 3607", "madina"),
        new("BRN-KAN-001", "Kaneshie Branch", "Branch Office", "Greater Accra", "Accra", "Kaneshie Market Road, Kaneshie", 15, 25, "{Company} branch serving Kaneshie market traders and the western Accra commercial hub", "+233 30 222 3608", "kaneshie"),
        new("BRN-ACH-001", "Achimota Branch", "Branch Office", "Greater Accra", "Accra", "Achimota, Accra", 15, 25, "{Company} branch serving Achimota and the northern Accra residential and commercial belt", "+233 30 222 3609", "achimota"),
        new("BRN-TC11-001", "Tema Community 11 Branch", "Branch Office", "Greater Accra", "Tema", "Community 11, Tema", 15, 25, "{Company} branch serving the growing Tema Community 11 residential area", "+233 30 322 3601", "temac11"),
        new("BRN-ASH-001", "Ashaiman Branch", "Branch Office", "Greater Accra", "Ashaiman", "Ashaiman Market Road, Ashaiman", 15, 25, "{Company} branch serving Ashaiman's dense trading and residential community", "+233 30 322 3602", "ashaiman"),
        new("BRN-DAN-001", "Dansoman Branch", "Branch Office", "Greater Accra", "Accra", "Dansoman Last Stop, Dansoman", 15, 25, "{Company} branch serving Dansoman's large residential and commercial community", "+233 30 222 3610", "dansoman"),
        new("BRN-LAP-001", "Lapaz Branch", "Branch Office", "Greater Accra", "Accra", "Lapaz Main Road, Lapaz", 15, 25, "{Company} branch serving Lapaz and the surrounding northern Accra communities", "+233 30 222 3611", "lapaz"),
        new("BRN-TES-001", "Teshie-Nungua Branch", "Branch Office", "Greater Accra", "Accra", "Teshie-Nungua Main Road", 15, 25, "{Company} branch serving Teshie-Nungua coastal and residential communities", "+233 30 222 3612", "teshie"),
        new("BRN-KSI-001", "Kumasi Adum Branch", "Branch Office", "Ashanti", "Kumasi", "Prempeh II Street, Adum", 15, 25, "{Company} main Kumasi branch at the heart of the Ashanti Region commercial hub", "+233 32 202 3600", "kumasi"),
        new("BRN-KSA-001", "Kumasi Asokwa Branch", "Branch Office", "Ashanti", "Kumasi", "Asokwa Industrial Area, Kumasi", 15, 25, "{Company} branch serving Asokwa's industrial zone and surrounding Kumasi communities", "+233 32 202 3601", "asokwa"),
        new("BRN-OBU-001", "Obuasi Branch", "Branch Office", "Ashanti", "Obuasi", "Main Street, Obuasi", 15, 25, "{Company} branch serving Obuasi mining community and AngloGold Ashanti employees", "+233 32 209 3600", "obuasi"),
        new("BRN-KSM-001", "Kumasi Manhyia Branch", "Branch Office", "Ashanti", "Kumasi", "Manhyia Road, Kumasi", 15, 25, "{Company} branch serving Manhyia Palace area and surrounding Kumasi communities", "+233 32 202 3602", "manhyia"),
        new("BRN-TKD-001", "Takoradi Market Circle Branch", "Branch Office", "Western", "Takoradi", "Market Circle, Takoradi", 15, 25, "{Company} main Takoradi branch serving Western Region oil, gas, and trading economy", "+233 31 202 3600", "takoradi"),
        new("BRN-SKD-001", "Sekondi Branch", "Branch Office", "Western", "Sekondi", "Railway Line Road, Sekondi", 15, 25, "{Company} branch serving Sekondi's fishing, naval, and residential communities", "+233 31 202 3601", "sekondi"),
        new("BRN-TKH-001", "Takoradi Harbour Branch", "Branch Office", "Western", "Takoradi", "Harbour Road, Takoradi", 15, 25, "{Company} branch serving Takoradi Harbour operators, freight forwarders, and oil service companies", "+233 31 202 3602", "takoradiharbour"),
        new("BRN-TAM-001", "Tamale Branch", "Branch Office", "Northern", "Tamale", "Salaga Road, Central Tamale", 15, 25, "{Company} main Northern Region branch serving government, commercial, and agribusiness clients", "+233 37 202 3600", "tamale"),
        new("BRN-BOL-001", "Bolgatanga Branch", "Branch Office", "Upper East", "Bolgatanga", "Zuarungu Road, Bolgatanga", 15, 25, "{Company} branch extending banking access to the Upper East Region", "+233 37 202 3601", "bolgatanga"),
        new("BRN-WA-001", "Wa Branch", "Branch Office", "Upper West", "Wa", "Wa Main Road, Wa Central", 15, 25, "{Company} branch serving the Upper West Region capital and surrounding communities", "+233 39 202 3600", "wa"),
        new("BRN-HO-001", "Ho Branch", "Branch Office", "Volta", "Ho", "Ho-Aflao Road, Ho Central", 15, 25, "{Company} branch serving the Volta Region capital and government institutions", "+233 36 202 3600", "ho"),
        new("BRN-HOH-001", "Hohoe Branch", "Branch Office", "Volta", "Hohoe", "Hohoe Market Road, Hohoe", 15, 25, "{Company} branch serving Hohoe and surrounding Volta Region communities", "+233 36 202 3601", "hohoe"),
        new("BRN-CAP-001", "Cape Coast Branch", "Branch Office", "Central", "Cape Coast", "Commercial Street, Cape Coast", 15, 25, "{Company} branch serving Cape Coast university community, tourism, and Central Region traders", "+233 33 202 3600", "capecoast"),
        new("BRN-KAS-001", "Kasoa Branch", "Branch Office", "Central", "Kasoa", "Kasoa Market Road, Kasoa", 15, 25, "{Company} high-traffic branch in the fast-growing Kasoa commercial and residential hub", "+233 33 202 3601", "kasoa"),
        new("BRN-KOF-001", "Koforidua Branch", "Branch Office", "Eastern", "Koforidua", "Hospital Road, Koforidua", 15, 25, "{Company} branch serving the Eastern Region capital and commercial community", "+233 34 202 3600", "koforidua"),
        new("BRN-NKW-001", "Nkawkaw Branch", "Branch Office", "Eastern", "Nkawkaw", "Accra-Kumasi Highway, Nkawkaw", 15, 25, "{Company} branch serving Nkawkaw's busy trading hub on the Accra-Kumasi highway", "+233 34 202 3601", "nkawkaw"),
        new("BRN-SUN-001", "Sunyani Branch", "Branch Office", "Bono", "Sunyani", "Fiapre Road, Sunyani Central", 15, 25, "{Company} branch serving Bono Region agricultural and commercial clients", "+233 35 202 3600", "sunyani"),
        new("BRN-TCH-001", "Techiman Branch", "Branch Office", "Bono East", "Techiman", "Techiman Market Road, Techiman", 15, 25, "{Company} branch serving Techiman's major market hub and Bono East Region clients", "+233 35 209 3600", "techiman"),
        new("SAT-KIA-001", "Kotoka International Airport Satellite Office", "Satellite Office", "Greater Accra", "Accra", "Terminal 3, Kotoka International Airport", 50, 25, "{Company} small-format FX and banking service point at KIA arrivals terminal", "+233 30 222 9001", "airport"),
        new("FO-TEM-001", "Tema Port Field Office", "Field Office", "Greater Accra", "Tema", "Port Access Road, Tema Port", 50, 25, "{Company} field office providing trade finance and FX services to Tema port operators and shipping agents", "+233 30 322 9001", "temaport"),
        new("FO-TKD-001", "Takoradi Port Field Office", "Field Office", "Western", "Takoradi", "Takoradi Harbour Road, Takoradi Port", 50, 25, "{Company} field office serving oil and gas companies and port operators in Takoradi", "+233 31 202 9001", "takoradiport"),
        new("FO-OBU-001", "Obuasi Mine Field Office", "Field Office", "Ashanti", "Obuasi", "AngloGold Ashanti Mine, Obuasi", 50, 25, "{Company} dedicated field office within the AngloGold Ashanti mine facility at Obuasi", "+233 32 209 9001", "obuasimine"),
        new("CW-ADC-001", "Accra Digital Centre Co-working Office", "Co-working Space", "Greater Accra", "Accra", "Accra Digital Centre, Accra", 50, 25, "{Company} co-working base for digital banking and fintech teams at Accra Digital Centre", "+233 30 222 9002", "digitalcentre"),
        new("LO-GSE-001", "Ghana Stock Exchange Liaison Office", "Remote Office", "Greater Accra", "Accra", "Cedi House, Liberia Road", 50, 25, "{Company} small liaison office near the Ghana Stock Exchange for investor relations activities", "+233 30 222 9003", "gse.liaison"),
    ];
}
