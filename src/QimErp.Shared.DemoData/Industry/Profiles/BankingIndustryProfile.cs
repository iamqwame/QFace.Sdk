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

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",       "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("RETAIL",     "Retail Banking",       "EXEC", OrgUnitKind.Function,  RetailJobs),
        new("CORPORATE",  "Corporate Banking",    "EXEC", OrgUnitKind.Function,  CorpJobs),
        new("INVESTMENT", "Investment Banking",   "EXEC", OrgUnitKind.Function,  CorpJobs),
        new("OPS",        "Operations",           "EXEC", OrgUnitKind.Function,  OpsJobs),
        new("RISK",       "Risk & Compliance",    "EXEC", OrgUnitKind.Function,  RiskJobs),
        new("TREASURY",   "Treasury",             "EXEC", OrgUnitKind.Function,  TreasuryJobs),
        new("IT",         "IT",                   "EXEC", OrgUnitKind.Function,  ItJobs),
        new("FINANCE",    "Finance & Accounting", "EXEC", OrgUnitKind.Function,  FinJobs),
        new("HR",         "HR & Admin",           "EXEC", OrgUnitKind.Function,  HrJobs)
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

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // ── Executive (rank 5) ────────────────────────────────────────────────
        new("CEO",            "Chief Executive Officer",       5, 20_000m, 35_000m, null,         null,         true,  "Master's Degree",   12, "Banking Leadership, Strategic Planning"),
        new("DEPUTY_MD",      "Deputy Managing Director",      5, 18_000m, 30_000m, "EXEC",       "CEO",        true,  "Master's Degree",   12, "Executive Strategy, Stakeholder Management"),
        new("CFO",            "Chief Financial Officer",       5, 18_000m, 32_000m, "FINANCE",    "CEO",        true,  "Master's Degree",   12, "Financial Strategy, Capital Markets"),
        new("CRO",            "Chief Risk Officer",            5, 18_000m, 30_000m, "RISK",       "CEO",        true,  "Master's Degree",   10, "Risk Management, Regulatory Compliance"),
        new("CTO",            "Chief Technology Officer",      5, 17_000m, 28_000m, "IT",         "CEO",        true,  "Master's Degree",   10, "Banking Technology, Digital Transformation"),
        new("CHRO",           "Chief People Officer",          5, 16_000m, 26_000m, "HR",         "CEO",        true,  "Master's Degree",   10, "Human Capital Strategy, Talent Management"),
        new("COO",            "Chief Operating Officer",       5, 18_000m, 30_000m, "OPS",        "CEO",        true,  "Master's Degree",   12, "Operational Excellence, Business Processes"),
        new("HEAD_RETAIL",    "Head of Retail Banking",        5, 15_000m, 25_000m, "RETAIL",     "CEO",        true,  "Master's Degree",   8,  "Retail Banking Strategy, Branch Network"),
        new("HEAD_CORP",      "Head of Corporate Banking",     5, 15_000m, 25_000m, "CORPORATE",  "CEO",        true,  "Master's Degree",   8,  "Corporate Banking, Business Development"),
        new("HEAD_INVEST",    "Head of Investment Banking",    5, 16_000m, 28_000m, "INVESTMENT", "CEO",        true,  "Master's Degree",   10, "Capital Markets, M&A Advisory"),
        new("HEAD_TREASURY",  "Head of Treasury",              5, 15_000m, 26_000m, "TREASURY",   "CEO",        true,  "Master's Degree",   10, "Treasury Strategy, Asset & Liability Management"),
        new("HEAD_DIGITAL",   "Head of Digital Banking",       5, 14_000m, 24_000m, "IT",         "CTO",        true,  "Master's Degree",   8,  "Digital Strategy, Mobile Banking"),

        // ── Senior (rank 4) ───────────────────────────────────────────────────
        new("REGIONAL_MGR",   "Regional Manager",              4, 11_000m, 18_000m, "RETAIL",     "HEAD_RETAIL",true,  "Bachelor's Degree", 8,  "Regional Operations, Branch Oversight"),
        new("BRANCH_MGR",     "Branch Manager",                4, 8_000m,  14_000m, "RETAIL",     "REGIONAL_MGR",true, "Bachelor's Degree", 6,  "Branch Operations, Customer Service, Sales"),
        new("REL_MGR",        "Relationship Manager",          4, 7_000m,  12_000m, "CORPORATE",  "HEAD_CORP",  true,  "Bachelor's Degree", 5,  "Client Relationship, Business Development"),
        new("WEALTH_MGR",     "Wealth Manager",                4, 10_000m, 18_000m, "INVESTMENT", "HEAD_INVEST",true,  "Bachelor's Degree", 6,  "Private Banking, Investment Advisory"),
        new("RISK_MGR",       "Risk Manager",                  4, 9_000m,  15_000m, "RISK",       "CRO",        true,  "Bachelor's Degree", 6,  "Risk Assessment, Compliance, Regulatory Reporting"),
        new("CREDIT_MGR",     "Credit Manager",                4, 9_000m,  15_000m, "RISK",       "CRO",        true,  "Bachelor's Degree", 6,  "Credit Underwriting, Loan Portfolio Management"),
        new("AUDIT_MGR",      "Internal Audit Manager",        4, 9_000m,  15_000m, "RISK",       "CRO",        true,  "Bachelor's Degree", 6,  "Internal Audit, Process Review"),
        new("COMPLIANCE_MGR", "Compliance Manager",            4, 8_000m,  13_000m, "RISK",       "CRO",        true,  "Bachelor's Degree", 5,  "Regulatory Compliance, AML, Policy"),
        new("AML_OFFICER",    "AML Officer",                   4, 7_500m,  12_500m, "RISK",       "COMPLIANCE_MGR",true,"Bachelor's Degree", 5,"Anti-Money Laundering, KYC, Sanctions Screening"),
        new("COMPLIANCE",     "Compliance Officer",            4, 7_000m,  12_000m, "RISK",       "COMPLIANCE_MGR",true,"Bachelor's Degree", 5,"Regulatory Compliance, Audit, Policy"),
        new("TREASURY_MGR",   "Treasury Manager",              4, 10_000m, 16_000m, "TREASURY",   "HEAD_TREASURY",true,"Bachelor's Degree", 6, "Liquidity Management, Foreign Exchange"),
        new("FX_TRADER",      "FX Trader",                     4, 11_000m, 19_000m, "TREASURY",   "TREASURY_MGR",true,"Bachelor's Degree",  5, "Foreign Exchange Trading, Market Analysis"),
        new("IT_MGR",         "IT Manager",                    4, 9_000m,  15_000m, "IT",         "CTO",        true,  "Bachelor's Degree", 6,  "IT Operations, Banking Systems, Security"),
        new("INFOSEC_MGR",    "Information Security Manager",  4, 10_000m, 17_000m, "IT",         "CTO",        true,  "Bachelor's Degree", 7,  "Cybersecurity, Threat Management, Incident Response"),
        new("FINANCE_MGR",    "Finance Manager",               4, 9_000m,  15_000m, "FINANCE",    "CFO",        true,  "Bachelor's Degree", 6,  "Financial Reporting, Budgeting, Forecasting"),
        new("HR_MGR",         "HR Manager",                    4, 7_500m,  13_000m, "HR",         "CHRO",       true,  "Bachelor's Degree", 6,  "Employee Relations, Talent Acquisition, L&D"),
        new("MARKETING_MGR",  "Marketing Manager",             4, 7_500m,  12_500m, "EXEC",       "CEO",        true,  "Bachelor's Degree", 5,  "Brand Management, Customer Acquisition, Campaigns"),

        // ── Mid (rank 3) ──────────────────────────────────────────────────────
        new("LOAN_OFFICER",   "Loan Officer",                  3, 4_000m,  8_000m,  "RETAIL",     "BRANCH_MGR", false, "Bachelor's Degree", 2,  "Loan Processing, Credit Analysis, Customer Service"),
        new("MORTGAGE_OFFICER","Mortgage Officer",             3, 4_500m,  8_500m,  "RETAIL",     "BRANCH_MGR", false, "Bachelor's Degree", 2,  "Mortgage Origination, Property Valuation"),
        new("BUSINESS_DEV",   "Business Development Officer",  3, 5_000m,  9_500m,  "CORPORATE",  "REL_MGR",    false, "Bachelor's Degree", 3,  "Pipeline Generation, Client Acquisition"),
        new("DIGITAL_OFFICER","Digital Banking Officer",       3, 5_500m,  10_000m, "IT",         "HEAD_DIGITAL",false,"Bachelor's Degree",  3, "Mobile Banking, Online Channels, UX"),
        new("DATA_ANALYST",   "Data Analyst",                  3, 5_500m,  10_000m, "IT",         "IT_MGR",     false, "Bachelor's Degree", 3,  "SQL, Reporting, Dashboards, Data Modelling"),
        new("TELLER",         "Teller",                        3, 2_500m,  5_000m,  "RETAIL",     "BRANCH_MGR", false, "High School",       1,  "Cash Handling, Customer Service, Transactions"),
        new("CORP_OFFICER",   "Corporate Banking Officer",     3, 6_000m,  11_000m, "CORPORATE",  "REL_MGR",    false, "Bachelor's Degree", 3,  "Corporate Banking, Credit Analysis, Business Development"),
        new("INVEST_ANALYST", "Investment Analyst",            3, 6_500m,  11_500m, "INVESTMENT", "WEALTH_MGR", false, "Bachelor's Degree", 3,  "Equity Research, Valuation Models, Pitch Decks"),
        new("RISK_ANALYST",   "Risk Analyst",                  3, 5_000m,  9_000m,  "RISK",       "RISK_MGR",   false, "Bachelor's Degree", 2,  "Risk Analysis, Data Analysis, Reporting"),
        new("CREDIT_ANALYST", "Credit Analyst",                3, 5_500m,  9_500m,  "RISK",       "CREDIT_MGR", false, "Bachelor's Degree", 3,  "Credit Underwriting, Cashflow Analysis"),
        new("AUDIT_OFFICER",  "Internal Audit Officer",        3, 5_000m,  9_000m,  "RISK",       "AUDIT_MGR",  false, "Bachelor's Degree", 2,  "Internal Audit Procedures, Walkthrough Testing"),
        new("OPS_OFFICER",    "Operations Officer",            3, 4_000m,  7_500m,  "OPS",        "COO",        false, "Bachelor's Degree", 2,  "Transaction Processing, Operations, Reconciliation"),
        new("CARDS_OFFICER",  "Cards & Payments Officer",      3, 4_500m,  8_000m,  "OPS",        "OPS_OFFICER",false, "Bachelor's Degree", 2,  "Card Issuance, Payments, Dispute Resolution"),
        new("CLEARING_OFFICER","Clearing & Settlement Officer",3, 4_500m,  8_000m,  "OPS",        "OPS_OFFICER",false, "Bachelor's Degree", 2,  "Clearing, Cheque Processing, Inter-Bank Settlement"),
        new("ACCOUNTANT",     "Accountant",                    3, 5_000m,  9_000m,  "FINANCE",    "FINANCE_MGR",false, "Bachelor's Degree", 2,  "Accounting, Financial Reporting, Bookkeeping"),
        new("IT_OFFICER",     "IT Officer",                    3, 6_000m,  11_000m, "IT",         "IT_MGR",     false, "Bachelor's Degree", 3,  "Banking Systems, Network Administration, Support"),
        new("HR_OFFICER",     "HR Officer",                    3, 4_500m,  8_000m,  "HR",         "HR_MGR",     false, "Bachelor's Degree", 2,  "Recruitment, Employee Records, Payroll Liaison"),
        new("MARKETING_EXEC", "Marketing Executive",           3, 4_000m,  7_500m,  "EXEC",       "MARKETING_MGR",false,"Bachelor's Degree",2, "Campaign Execution, Social Media, Content"),

        // ── Junior (rank 2) ───────────────────────────────────────────────────
        new("JUNIOR_TELLER",  "Junior Teller",                 2, 2_000m,  3_500m,  "RETAIL",     "TELLER",     false, "High School",       0,  "Cash Handling, Basic Transactions"),
        new("CSR",            "Customer Service Rep",          2, 2_500m,  4_500m,  "RETAIL",     "BRANCH_MGR", false, "High School",       1,  "Customer Service, Account Inquiries"),
        new("OPS_ASSIST",     "Operations Assistant",          2, 2_500m,  4_500m,  "OPS",        "OPS_OFFICER",false, "High School",       1,  "Transaction Processing Support, Filing"),
        new("HR_ASSIST",      "HR Assistant",                  2, 2_500m,  4_500m,  "HR",         "HR_OFFICER", false, "Diploma",           1,  "Onboarding Coordination, HR Records, Filing"),
        new("JUNIOR_ACCOUNTANT","Junior Accountant",           2, 3_000m,  5_500m,  "FINANCE",    "ACCOUNTANT", false, "Diploma",           1,  "Basic Accounting, Data Entry"),
        new("IT_ASSIST",      "IT Support Assistant",          2, 3_000m,  5_500m,  "IT",         "IT_OFFICER", false, "Diploma",           1,  "Helpdesk, Hardware Support, Account Provisioning"),
        new("BRANCH_ASSIST",  "Branch Operations Assistant",   2, 2_500m,  4_500m,  "RETAIL",     "BRANCH_MGR", false, "High School",       1,  "Branch Floor Support, Queue Management"),

        // ── Entry (rank 1) ────────────────────────────────────────────────────
        new("TELLER_TRAINEE", "Teller Trainee",                1, 1_500m,  2_500m,  "RETAIL",     "TELLER",     false, "High School",       0,  "Learning, Supervised Transactions"),
        new("OPS_INTERN",     "Operations Intern",             1, 1_500m,  2_500m,  "OPS",        "OPS_OFFICER",false, "Student",           0,  "Learning, Operations Support"),
        new("FINANCE_INTERN", "Finance Intern",                1, 1_500m,  2_500m,  "FINANCE",    "ACCOUNTANT", false, "Student",           0,  "Learning, Financial Records Support"),
        new("RISK_INTERN",    "Risk & Compliance Intern",      1, 1_500m,  2_500m,  "RISK",       "RISK_ANALYST",false,"Student",           0,  "Learning, Risk & Compliance Support"),
        new("IT_INTERN",      "IT Intern",                     1, 1_500m,  2_500m,  "IT",         "IT_OFFICER", false, "Student",           0,  "Learning, Helpdesk Shadowing"),
        new("HR_INTERN",      "HR Intern",                     1, 1_500m,  2_500m,  "HR",         "HR_OFFICER", false, "Student",           0,  "Learning, HR Administration Support"),
        new("CUSTOMER_INTERN","Customer Service Intern",       1, 1_500m,  2_500m,  "RETAIL",     "CSR",        false, "Student",           0,  "Learning, Customer Service Shadowing")
    ];
}
