using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

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
        // Corporate tier uses the curated 40-station catalogue verbatim — Head Office,
        // Engineering Centre, Fulfilment Centres in industrial cities, Last-Mile Hubs
        // and Pickup/Locker Stations across Ghana, plus Customer Service / Vendor
        // Support hubs. The same shape any tier-1 Ghana e-commerce / marketplace
        // operator (Jumia Ghana, Glovo, Bolt Food scale) deploys in production.
        if (tier == CompanyTier.Corporate)
        {
            var hqRow = _ecommerceStations[0];
            var rest = _ecommerceStations.Skip(1).ToList();
            var branchTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Fulfilment Centre", "Customer Service Hub", "Engineering Centre", "Vendor Support Centre"
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
                Name: $"{city} Fulfilment Centre",
                StationType: "Fulfilment Centre",
                Region: region,
                City: city,
                Address: $"Industrial Estate, {city}",
                CapacityMin: 25,
                CapacityMax: 200));
        }

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
            [2] = 0.500,
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

    private static readonly IReadOnlyList<string> ExecJobs        = ["CEO-001", "COO-001", "CM-001", "VP-COM-001", "VP-OPS-001", "VP-TECH-001"];
    private static readonly IReadOnlyList<string> CommercialJobs  = ["VP-COM-001", "HEAD-VEND-001", "CAT-MGR-ELC-001", "CAT-MGR-FAS-001", "CAT-MGR-HOM-001", "CAT-MGR-GRO-001", "BUY-001", "BUY-002", "VEND-MGR-001", "VEND-ON-001", "PRC-001", "SUP-PLN-001"];
    private static readonly IReadOnlyList<string> MarketingJobs   = ["HEAD-MKT-001", "MKT-001", "MKT-002", "MKT-003", "MKT-004", "MKT-005", "MKT-006"];
    private static readonly IReadOnlyList<string> OpsJobs         = ["VP-OPS-001", "HEAD-LOG-001", "INV-MGR-001", "WH-MGR-001", "WH-SUP-001", "WH-INB-001", "WH-OUT-001", "WH-PNP-001", "WH-PCK-001", "WH-RTN-001", "WH-QC-001", "LM-MGR-001", "LM-HUB-001", "LM-DSP-001", "LM-RDR-001", "LM-FLT-001"];
    private static readonly IReadOnlyList<string> CustomerOpsJobs = ["HEAD-CS-001", "CS-MGR-001", "CS-LD-001", "CS-AGT-001", "CS-AGT-002", "CS-AGT-003", "CS-ESC-001"];
    private static readonly IReadOnlyList<string> TechJobs        = ["VP-TECH-001", "SE-FE-001", "SE-BE-001", "SE-MOB-001", "SE-001", "DA-001", "BI-001", "PM-001", "UX-001"];
    private static readonly IReadOnlyList<string> FinanceJobs     = ["FIN-MGR-001", "ACC-001", "PAY-001"];
    private static readonly IReadOnlyList<string> PeopleJobs      = ["HR-001", "TA-001", "OFF-001"];
    private static readonly IReadOnlyList<string> RiskTrustJobs   = ["RISK-001", "TS-001"];

    // Light-touch lists for Startup / SME / NonProfit tiers
    private static readonly IReadOnlyList<string> StartupExecJobs = ["CEO-001"];
    private static readonly IReadOnlyList<string> StartupOpsJobs  = ["WH-MGR-001", "WH-PCK-001", "LM-RDR-001", "LM-DSP-001"];
    private static readonly IReadOnlyList<string> StartupTechJobs = ["SE-001", "PM-001"];
    private static readonly IReadOnlyList<string> ProgramsJobs    = ["PM-001", "UX-001"];
    private static readonly IReadOnlyList<string> AdminJobs       = ["HR-001", "OFF-001"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER", "Founder/CEO", null,      OrgUnitKind.Executive, StartupExecJobs),
        new("OPS",     "Operations",  "FOUNDER", OrgUnitKind.Function,  StartupOpsJobs),
        new("TECH",    "Technology",  "FOUNDER", OrgUnitKind.Function,  StartupTechJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"] = 0.20,
        ["OPS"]     = 0.50,
        ["TECH"]    = 0.30
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",         "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("OPERATIONS",   "Operations",           "EXEC", OrgUnitKind.Function,  OpsJobs),
        new("CUSTOMER-OPS", "Customer Operations",  "EXEC", OrgUnitKind.Function,  CustomerOpsJobs),
        new("MARKETING",    "Marketing & Growth",   "EXEC", OrgUnitKind.Function,  MarketingJobs),
        new("TECH",         "Technology",           "EXEC", OrgUnitKind.Function,  TechJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]         = 0.05,
        ["OPERATIONS"]   = 0.40,
        ["CUSTOMER-OPS"] = 0.25,
        ["MARKETING"]    = 0.20,
        ["TECH"]         = 0.10
    };

    // Corporate-tier baseline OrgUnits — each carries Description / Budget / CostCenter /
    // Purpose / Phone / Email-local-part. The {Company} placeholder substitutes at row-emit
    // time so the same catalogue reads naturally for any tenant (Jumia Ghana, Glovo Ghana,
    // Bolt Food, Hubtel Mall, etc.). Budget bands are representative for a tier-1 Ghana
    // e-commerce / online marketplace operator with national fulfilment + rider footprint.
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",          "Executive",                        null,         OrgUnitKind.Executive, ExecJobs,
            Description: "Office of the CEO and country leadership team of {Company} — sets strategy, manages investors, and runs the executive committee",
            BudgetMin: 1_000_000m, BudgetMax: 2_500_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Set and execute {Company} strategy in Ghana; manage shareholder, BoG, and regulatory relationships; lead the executive management committee",
            Phone: "+233 30 XXX XXXX", Email: "executive"),
        new("COMMERCIAL",    "Commercial & Vendor Management",   "EXEC",       OrgUnitKind.Function,  CommercialJobs,
            Description: "Category management, buying, vendor onboarding, pricing, and supply planning — owns {Company}'s catalogue and seller experience",
            BudgetMin: 3_000_000m, BudgetMax: 7_000_000m,
            CostCenter: "CC-COM-001",
            Purpose: "Grow {Company} GMV by curating the right assortment, onboarding the right sellers, and pricing competitively across categories",
            Phone: "+233 30 XXX XXXX", Email: "commercial"),
        new("MARKETING",     "Marketing & Growth",               "EXEC",       OrgUnitKind.Function,  MarketingJobs,
            Description: "Performance marketing, brand, content, social, CRM, and email lifecycle — drives traffic, acquisition, and retention for {Company}",
            BudgetMin: 2_500_000m, BudgetMax: 6_000_000m,
            CostCenter: "CC-MKT-001",
            Purpose: "Acquire and retain customers profitably; build the {Company} brand; drive repeat purchase and lifetime value",
            Phone: "+233 30 XXX XXXX", Email: "marketing"),
        new("OPERATIONS",    "Logistics & Fulfilment",           "EXEC",       OrgUnitKind.Function,  OpsJobs,
            Description: "Operates {Company}'s warehouses, last-mile hubs, and rider network — moves orders from vendor to customer doorstep",
            BudgetMin: 3_500_000m, BudgetMax: 9_000_000m,
            CostCenter: "CC-OPS-001",
            Purpose: "Hit promised delivery SLAs at lowest cost-per-order across {Company}'s network",
            Phone: "+233 30 XXX XXXX", Email: "operations"),
        new("CUSTOMER-OPS",  "Customer Operations",              "EXEC",       OrgUnitKind.Function,  CustomerOpsJobs,
            Description: "Voice, chat, email, and social customer support; escalations, refunds, and CSAT — {Company}'s buyer-facing care function",
            BudgetMin: 1_500_000m, BudgetMax: 3_500_000m,
            CostCenter: "CC-CS-001",
            Purpose: "Resolve buyer issues fast; protect CSAT and trust; recover at-risk customers and reduce contact-per-order",
            Phone: "+233 30 XXX XXXX", Email: "support"),
        new("TECH",          "Technology & Product",             "EXEC",       OrgUnitKind.Function,  TechJobs,
            Description: "Web, mobile app, seller portal, payments integrations, data platform, and product management — the engineering engine of {Company}",
            BudgetMin: 3_000_000m, BudgetMax: 7_500_000m,
            CostCenter: "CC-TECH-001",
            Purpose: "Build and run a fast, reliable, secure marketplace platform that scales with {Company} GMV growth",
            Phone: "+233 30 XXX XXXX", Email: "engineering"),
        new("FINANCE",       "Finance & Accounts",               "EXEC",       OrgUnitKind.Function,  FinanceJobs,
            Description: "Financial reporting, vendor payouts, accounts payable/receivable, treasury, tax, and management accounting for {Company}",
            BudgetMin: 600_000m, BudgetMax: 1_500_000m,
            CostCenter: "CC-FIN-001",
            Purpose: "Produce accurate {Company} financials; pay vendors on time; manage cash; drive unit economics visibility",
            Phone: "+233 30 XXX XXXX", Email: "finance"),
        new("PEOPLE",        "People & Culture",                 "EXEC",       OrgUnitKind.Function,  PeopleJobs,
            Description: "Talent acquisition, onboarding, total rewards, performance, and employee relations across HQ, fulfilment, and rider workforce",
            BudgetMin: 500_000m, BudgetMax: 1_200_000m,
            CostCenter: "CC-HR-001",
            Purpose: "Attract, onboard, and retain the corporate, warehouse, and rider talent {Company} needs to scale",
            Phone: "+233 30 XXX XXXX", Email: "people"),
        new("RISK-TRUST",    "Risk, Trust & Safety",             "EXEC",       OrgUnitKind.Function,  RiskTrustJobs,
            Description: "Fraud, payments risk, seller policy enforcement, dispute investigation, KYC, and platform trust for {Company}",
            BudgetMin: 400_000m, BudgetMax: 1_000_000m,
            CostCenter: "CC-RT-001",
            Purpose: "Keep fraud, chargebacks, and abuse low; protect buyers and honest sellers; enforce platform policy fairly",
            Phone: "+233 30 XXX XXXX", Email: "trust"),
        new("OPS-FINANCE",   "Operations Finance & Reconciliation", "FINANCE", OrgUnitKind.Function,  FinanceJobs,
            Description: "Vendor reconciliation, COD float management, rider payouts, marketplace settlement, and ops-level financial controls for {Company}",
            BudgetMin: 300_000m, BudgetMax: 800_000m,
            CostCenter: "CC-OPSFIN-001",
            Purpose: "Close the books on every order; reconcile cash-on-delivery, mobile money, and card flows; pay vendors and riders accurately",
            Phone: "+233 30 XXX XXXX", Email: "opsfinance")
    ];

    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]         = 0.02,
        ["COMMERCIAL"]   = 0.10,
        ["MARKETING"]    = 0.06,
        ["OPERATIONS"]   = 0.45,
        ["CUSTOMER-OPS"] = 0.18,
        ["TECH"]         = 0.10,
        ["FINANCE"]      = 0.03,
        ["PEOPLE"]       = 0.02,
        ["RISK-TRUST"]   = 0.02,
        ["OPS-FINANCE"]  = 0.02
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

    // ──────────────────────────────────────────────────────────────────────────
    // Job titles — Cal Bank-grade richness for a Ghana e-commerce / marketplace
    // operator. {Company} placeholder substituted at row-emit time. Pay grade
    // EC-1..EC-9 corporate / WH-1..WH-5 warehouse & fulfilment. Salary bands by
    // rank: 1=1,800-4,500 / 2=4,000-10,000 / 3=9,500-22,000 / 4=20,000-45,000 /
    // 5=42,000-90,000 GHS/month. Annual leave: rank 5=30 / 4=27 / 3=24 / 2=21 /
    // 1=21 days.
    // ──────────────────────────────────────────────────────────────────────────
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // ── Executive ─────────────────────────────────────────────────────────
        new("CEO-001", "Chief Executive Officer", 5, 60_000m, 90_000m, "EXEC", null, true, "Master's Degree", 15, "E-commerce Strategy, Marketplace Economics, Investor Relations, P&L Leadership, GMV Growth", "Chief executive accountable for {Company}'s overall strategy, performance, and stakeholder relations in Ghana", "EC-9", "Set and execute {Company} strategy; lead executive committee; manage investor and BoG relationships; drive sustainable GMV and contribution-margin growth", "GMV Growth, Contribution Margin, Active Buyers, NPS, Funding Runway, Staff Engagement", 30),
        new("COO-001", "Chief Operating Officer", 5, 50_000m, 80_000m, "EXEC", "CEO-001", true, "Master's Degree", 12, "Operations Leadership, Logistics, Last-Mile, Process Engineering, Cost-to-Serve", "Executive overseeing all {Company} fulfilment, last-mile, and customer operations across Ghana", "EC-9", "Run end-to-end {Company} operations; drive cost-per-order down; ensure SLA delivery; lead operations transformation; manage rider, hub, and warehouse network", "Cost-per-Order, On-Time Delivery %, Rider Productivity, Warehouse Cost Ratio, Operational NPS", 30),
        new("CM-001", "Country Manager", 5, 50_000m, 78_000m, "EXEC", "CEO-001", true, "Master's Degree", 12, "Market Leadership, Stakeholder Management, P&L, Localisation, Regulatory Engagement", "Country leader accountable for {Company}'s Ghana market P&L, expansion, and stakeholder relations", "EC-9", "Own Ghana P&L; drive market expansion; manage local regulatory and partnership relationships; localise the {Company} proposition; lead Ghana ExCo", "Country Revenue, Market Share, Active Cities, Regulatory Compliance, Local NPS", 30),
        new("VP-COM-001", "VP Commercial", 5, 45_000m, 72_000m, "COMMERCIAL", "CEO-001", true, "Master's Degree", 12, "Category Management, Buying, Vendor Strategy, Pricing, Assortment Planning", "Executive leading {Company}'s catalogue, category, vendor, and pricing strategy", "EC-9", "Own {Company} GMV plan by category; lead buying and vendor management; set pricing and promotional strategy; drive seller acquisition; chair commercial review", "GMV by Category, Active Sellers, Take Rate, Assortment Depth, Sell-Through Rate", 30),
        new("VP-OPS-001", "VP Operations", 5, 45_000m, 72_000m, "OPERATIONS", "COO-001", true, "Master's Degree", 12, "Logistics Strategy, Fulfilment Network Design, Last-Mile, S&OP, Lean Operations", "Executive leading {Company}'s warehouse, last-mile, and fulfilment network across Ghana", "EC-9", "Design and run {Company} fulfilment network; hit delivery SLAs; drive cost-to-serve down; lead capacity planning; oversee fleet and rider strategy", "On-Time Delivery %, Cost-per-Order, Warehouse Throughput, Rider Utilisation, Damage Rate", 30),
        new("VP-TECH-001", "VP Engineering & Product", 5, 48_000m, 78_000m, "TECH", "CEO-001", true, "Master's Degree", 12, "Engineering Leadership, Marketplace Architecture, Mobile, Payments, Data Platform", "Executive leading {Company}'s technology and product organisation including web, app, seller tools, and data", "EC-9", "Own {Company} technology roadmap; lead engineering and product teams; ensure platform reliability and security; drive feature velocity and quality", "Platform Uptime, Feature Velocity, App Store Rating, Engineering Headcount Productivity, Incident MTTR", 30),

        // ── Heads / Senior Function Leadership ────────────────────────────────
        new("HEAD-LOG-001", "Head of Logistics", 4, 28_000m, 42_000m, "OPERATIONS", "VP-OPS-001", true, "Bachelor's Degree", 8, "Last-Mile Logistics, Hub Network Design, Fleet Management, Routing Algorithms, Cost Optimisation", "Leads {Company}'s logistics, last-mile hub network, and rider fleet operations", "EC-7", "Run last-mile network; manage hub coordinators and dispatchers; optimise routing and fleet costs; hit delivery SLAs; expand into new cities", "On-Time Delivery %, Failed Delivery Rate, Cost-per-Order, Rider Utilisation, City Expansion Rate", 27),
        new("HEAD-CS-001", "Head of Customer Operations", 4, 25_000m, 38_000m, "CUSTOMER-OPS", "COO-001", true, "Bachelor's Degree", 8, "Customer Service Leadership, Contact Centre Operations, CSAT, NPS, Workforce Management", "Leads {Company}'s contact centre, escalations, and overall customer support function", "EC-7", "Run {Company} contact centre; manage CS managers and team leads; hit CSAT/NPS; reduce contact-per-order; train and develop CS workforce", "CSAT, NPS, Average Handle Time, First Contact Resolution, Contact-per-Order Ratio", 27),
        new("HEAD-MKT-001", "Head of Marketing", 4, 26_000m, 40_000m, "MARKETING", "CEO-001", true, "Bachelor's Degree", 8, "Marketing Strategy, Performance Marketing, Brand, CRM, Customer Acquisition", "Leads {Company}'s performance marketing, brand, content, social, and CRM functions", "EC-7", "Own {Company} marketing plan; manage growth and brand teams; drive cost-effective acquisition; build brand equity; grow repeat purchase rate", "CAC, Blended ROAS, Brand Awareness, Repeat Purchase Rate, Email/Push Engagement", 27),
        new("HEAD-VEND-001", "Head of Vendor & Seller Management", 4, 24_000m, 38_000m, "COMMERCIAL", "VP-COM-001", true, "Bachelor's Degree", 8, "Vendor Management, Seller Acquisition, Marketplace Operations, Account Management, Negotiation", "Leads {Company}'s seller acquisition, onboarding, and ongoing vendor success function", "EC-7", "Drive seller acquisition; manage vendor managers and onboarding team; grow active seller base; lift seller GMV per active seller; reduce time-to-first-order", "Active Sellers, GMV per Seller, Time-to-First-Order, Seller NPS, Vendor Churn Rate", 27),

        // ── Category Managers ─────────────────────────────────────────────────
        new("CAT-MGR-ELC-001", "Category Manager - Electronics", 4, 22_000m, 35_000m, "COMMERCIAL", "VP-COM-001", true, "Bachelor's Degree", 7, "Electronics Category Management, Buying, Promo Planning, Brand Negotiation, Margin Management", "Owns the Electronics category P&L on the {Company} platform — phones, laptops, accessories, audio", "EC-6", "Plan electronics assortment; negotiate with brands and distributors; set promotional calendar; manage category margins; grow GMV per visitor", "Electronics GMV, Margin %, Sell-Through Rate, Brand Coverage, Conversion Rate", 27),
        new("CAT-MGR-FAS-001", "Category Manager - Fashion", 4, 22_000m, 34_000m, "COMMERCIAL", "VP-COM-001", true, "Bachelor's Degree", 7, "Fashion Buying, Trend Analysis, Visual Merchandising, Seasonal Planning, Returns Management", "Owns the Fashion category on the {Company} platform — apparel, footwear, accessories", "EC-6", "Plan fashion assortment by season; onboard fashion sellers; manage returns and quality; run fashion campaigns; lift basket size in apparel", "Fashion GMV, Return Rate, Active Fashion Sellers, Basket Size, Seasonal Sell-Through", 27),
        new("CAT-MGR-HOM-001", "Category Manager - Home & Living", 4, 21_000m, 33_000m, "COMMERCIAL", "VP-COM-001", true, "Bachelor's Degree", 7, "Home Goods Buying, Bulky Goods Logistics, Furniture Sourcing, Margin Management", "Owns Home & Living on the {Company} platform — furniture, kitchen, decor, appliances", "EC-6", "Plan home assortment; manage bulky-item logistics partners; onboard furniture and appliance sellers; run home category promotions", "Home GMV, Bulky Delivery Cost, Margin %, Active Home Sellers, Damage Rate", 27),
        new("CAT-MGR-GRO-001", "Category Manager - Groceries & FMCG", 4, 21_000m, 33_000m, "COMMERCIAL", "VP-COM-001", true, "Bachelor's Degree", 7, "Grocery Buying, FMCG Category Management, Cold Chain, Trade Marketing, Supplier Negotiation", "Owns Groceries & FMCG on the {Company} platform — staples, beverages, household, fresh", "EC-6", "Plan grocery assortment; manage FMCG supplier relationships; run trade promotions; expand fresh and cold-chain capability; grow basket frequency", "Grocery GMV, Order Frequency, Stock-Out Rate, Margin %, Cold-Chain Delivery SLA", 27),

        // ── Buyers / Vendor / Pricing / Planning ──────────────────────────────
        new("BUY-001", "Senior Buyer", 3, 14_000m, 20_000m, "COMMERCIAL", "CAT-MGR-ELC-001", false, "Bachelor's Degree", 5, "Buying, Negotiation, Demand Forecasting, Supplier Management, Margin Modelling", "Senior buyer responsible for sourcing, negotiating, and assortment planning within a {Company} category", "EC-5", "Source new SKUs; negotiate trading terms with brands and distributors; build buying plans by season; review supplier performance; coach junior buyers", "Buying Plan Accuracy, Margin %, On-Time PO Delivery, New SKU Sell-Through, Supplier Performance Score", 24),
        new("BUY-002", "Buyer", 3, 9_500m, 14_500m, "COMMERCIAL", "BUY-001", false, "Bachelor's Degree", 3, "Procurement, Excel, Supplier Communication, PO Management, Reconciliation", "Buyer executing day-to-day purchasing, PO management, and supplier follow-up for {Company}", "EC-4", "Raise and track POs; follow up with suppliers on deliveries; reconcile invoices; maintain SKU master data; support category manager on negotiations", "PO Cycle Time, Invoice Match Rate, On-Time Delivery from Suppliers, SKU Data Accuracy", 24),
        new("VEND-MGR-001", "Vendor Manager", 3, 11_000m, 17_000m, "COMMERCIAL", "HEAD-VEND-001", false, "Bachelor's Degree", 4, "Vendor Account Management, Marketplace Operations, Performance Coaching, Seller Tools, Negotiation", "Manages a portfolio of {Company} marketplace sellers — drives their GMV, quality, and adherence to platform policy", "EC-4", "Own portfolio of sellers; coach on listing quality, pricing, and fulfilment; review performance; resolve seller escalations; grow GMV per seller", "Portfolio GMV, Active Seller Rate, Listing Quality Score, Seller Cancellation Rate, Seller NPS", 24),
        new("VEND-ON-001", "Vendor Onboarding Specialist", 3, 9_500m, 14_000m, "COMMERCIAL", "HEAD-VEND-001", false, "Bachelor's Degree", 2, "Onboarding, KYC, Seller Training, Marketplace Tools, Documentation", "Onboards new sellers onto the {Company} platform — KYC, contracts, training, first listing", "EC-3", "KYC and verify new sellers; collect contracts; train on seller tools; help post first listings; hand over to vendor managers; track time-to-first-order", "Time-to-First-Listing, Time-to-First-Order, Onboarding NPS, KYC Pass Rate, Seller Activation Rate", 24),
        new("PRC-001", "Pricing Analyst", 3, 10_000m, 15_500m, "COMMERCIAL", "VP-COM-001", false, "Bachelor's Degree", 3, "Pricing Analytics, Competitive Scraping, Excel/SQL, Margin Modelling, Promo Pricing", "Sets and monitors {Company} pricing strategy across categories — competitive scraping, dynamic pricing, promo support", "EC-4", "Run competitor price scrapes; recommend price changes; build promo pricing models; monitor margin impact; support category managers on pricing decisions", "Price Competitiveness Index, Margin %, Promo ROI, Price Change Velocity, Stock-Out Rate", 24),
        new("SUP-PLN-001", "Supply Planner", 3, 10_000m, 15_500m, "COMMERCIAL", "VP-COM-001", false, "Bachelor's Degree", 3, "Demand Forecasting, S&OP, Inventory Planning, Excel, ERP Systems", "Forecasts demand and plans inventory replenishment across {Company} fulfilment centres", "EC-4", "Build demand forecasts by SKU/region; plan replenishment; coordinate with buyers on lead times; track stock health; reduce stock-outs and overstock", "Forecast Accuracy, Stock-Out Rate, Days of Cover, Excess Inventory %, Replenishment Cycle Time", 24),
        new("INV-MGR-001", "Inventory Manager", 4, 20_000m, 32_000m, "OPERATIONS", "VP-OPS-001", true, "Bachelor's Degree", 6, "Inventory Management, Cycle Counting, WMS, Shrinkage Control, S&OP", "Owns {Company} end-to-end inventory accuracy across all fulfilment centres", "EC-6", "Drive inventory accuracy; lead cycle counting programmes; manage shrinkage investigations; oversee WMS data integrity; report inventory KPIs to ops leadership", "Inventory Accuracy %, Shrinkage Rate, Cycle Count Coverage, Days of Cover, Write-Off Value", 27),

        // ── Warehouse / Fulfilment ────────────────────────────────────────────
        new("WH-MGR-001", "Warehouse Manager", 4, 21_000m, 32_000m, "OPERATIONS", "VP-OPS-001", true, "Bachelor's Degree", 7, "Warehouse Management, WMS, Lean, Health & Safety, Team Leadership, Throughput Optimisation", "Manages a {Company} fulfilment centre — inbound, outbound, inventory, people, and SLAs", "WH-5", "Run the FC P&L; lead supervisors and shift teams; hit picking/packing SLAs; manage H&S compliance; drive cost-per-unit down; coordinate with last-mile", "FC Throughput, Pick/Pack SLA, Cost-per-Unit, Inventory Accuracy, Lost-Time Incident Rate", 27),
        new("WH-SUP-001", "Warehouse Supervisor", 3, 11_000m, 17_000m, "OPERATIONS", "WH-MGR-001", true, "Diploma", 4, "Shift Supervision, WMS, Operations, People Management, H&S", "Supervises a shift inside a {Company} fulfilment centre — picking, packing, inbound/outbound flows", "WH-4", "Lead shift team; allocate work; resolve operational blockers; ensure H&S; deliver shift SLAs; coach pickers and packers; report shift KPIs", "Shift SLA Achievement, Pick Rate, Pack Rate, Error Rate, Attendance Rate", 24),
        new("WH-INB-001", "Inbound Lead", 3, 10_000m, 15_500m, "OPERATIONS", "WH-MGR-001", true, "Diploma", 3, "Inbound Receiving, GRN Processing, WMS, Quality Check, Vendor Coordination", "Owns inbound receiving operations at a {Company} fulfilment centre — GRNs, putaway, vendor coordination", "WH-4", "Schedule inbound dock; receive and GRN goods; coordinate putaway; resolve discrepancies with vendors; meet inbound SLA; manage inbound team", "Inbound Cycle Time, GRN Accuracy, Discrepancy Rate, Dock Utilisation, Putaway SLA", 24),
        new("WH-OUT-001", "Outbound Lead", 3, 10_000m, 15_500m, "OPERATIONS", "WH-MGR-001", true, "Diploma", 3, "Outbound Operations, Manifest Management, Sortation, Carrier Hand-off, WMS", "Owns outbound dispatch operations at a {Company} fulfilment centre — sortation, manifesting, last-mile hand-off", "WH-4", "Run outbound flow; sort and manifest orders; hand off to last-mile; resolve dispatch exceptions; meet cut-off times; coordinate with hubs", "Dispatch Cut-Off Hit Rate, Manifest Accuracy, Sortation Productivity, Late Dispatch Rate", 24),
        new("WH-PNP-001", "Pick & Pack Lead", 3, 9_500m, 14_500m, "OPERATIONS", "WH-MGR-001", true, "Diploma", 3, "Pick & Pack Supervision, WMS, Quality Control, Workforce Productivity", "Leads pick-and-pack teams inside a {Company} fulfilment centre", "WH-3", "Run pick and pack lines; allocate pickers and packers; monitor productivity; ensure pack quality; reduce mispick and damage; coach team", "Picks per Hour, Pack Quality Score, Mispick Rate, Productivity Variance, Order Cycle Time", 24),
        new("WH-PCK-001", "Picker / Packer", 1, 1_800m, 3_200m, "OPERATIONS", "WH-PNP-001", false, "High School", 0, "Picking, Packing, Scanning, Attention to Detail, WMS Basics, H&S", "Frontline associate picking, packing, and dispatching customer orders at a {Company} fulfilment centre", "WH-1", "Pick orders accurately; pack to spec; scan barcodes; flag damage; meet productivity targets; follow H&S and quality standards", "Picks per Hour, Pack Accuracy, Mispick Rate, Damage Rate, Attendance", 21),
        new("WH-RTN-001", "Returns Officer", 2, 3_000m, 4_800m, "OPERATIONS", "WH-MGR-001", false, "High School", 1, "Returns Processing, Quality Inspection, Refund Triage, WMS, Customer Service", "Processes customer returns and refurbishment decisions inside the {Company} returns area", "WH-2", "Receive and inspect returns; triage refund vs. resale vs. write-off; restock saleable items; coordinate with CS on disputes; track returns KPIs", "Returns Cycle Time, Resale Rate, Write-Off Rate, Refund Decision Accuracy, Dispute Rate", 21),
        new("WH-QC-001", "Quality Inspector", 2, 3_200m, 5_000m, "OPERATIONS", "WH-MGR-001", false, "Diploma", 1, "Quality Control, Product Inspection, Sampling, Documentation, Vendor Quality", "Inspects inbound and outbound goods for quality and compliance with {Company} standards", "WH-2", "Inspect inbound shipments; sample QC outbound packs; document defects; raise vendor quality issues; reduce customer-reported defect rate", "Defect Detection Rate, Vendor QC Pass Rate, Customer Defect PPM, Inspection Coverage", 21),

        // ── Last-Mile / Fleet ─────────────────────────────────────────────────
        new("LM-MGR-001", "Last-Mile Manager", 4, 20_000m, 32_000m, "OPERATIONS", "HEAD-LOG-001", true, "Bachelor's Degree", 6, "Last-Mile Operations, Hub Network, Routing, Rider Management, Cost-per-Drop", "Manages a region of {Company}'s last-mile hubs and rider fleet", "EC-6", "Run regional last-mile P&L; manage hub coordinators and dispatchers; optimise routes; hit delivery SLAs; control cost-per-drop; expand into new areas", "On-Time Delivery %, Cost-per-Drop, Failed Delivery Rate, Rider Utilisation, Hub Cost Ratio", 27),
        new("LM-HUB-001", "Hub Coordinator", 3, 9_500m, 14_500m, "OPERATIONS", "LM-MGR-001", true, "Diploma", 3, "Hub Operations, Sortation, Rider Coordination, COD Reconciliation, WMS Basics", "Runs day-to-day operations at a {Company} last-mile hub — sortation, rider dispatch, COD float", "WH-4", "Sort inbound parcels; brief and dispatch riders; reconcile COD at end of shift; track delivery exceptions; coordinate with FC and dispatchers", "Hub SLA Achievement, COD Reconciliation Accuracy, Rider On-Time Departure, Exception Resolution Time", 24),
        new("LM-DSP-001", "Dispatcher", 2, 3_500m, 5_500m, "OPERATIONS", "LM-HUB-001", false, "Diploma", 1, "Dispatch, Routing Tools, Rider Coordination, Live Ops, Communication", "Allocates orders to riders in real time and monitors live deliveries for {Company}", "WH-2", "Allocate orders to riders; monitor live deliveries; reroute on exceptions; communicate ETAs; resolve rider blockers; report shift performance", "Orders per Rider Hour, On-Time Allocation, Reassignment Rate, Live Delivery NPS", 21),
        new("LM-RDR-001", "Rider / Delivery Driver", 1, 2_000m, 3_500m, "OPERATIONS", "LM-DSP-001", false, "High School", 0, "Riding, Customer Service, Cash Handling, Navigation, Time Management", "Frontline rider delivering customer orders for {Company} on motorbike, bicycle, or van", "WH-1", "Collect parcels at hub; navigate to customer; deliver on time; collect COD where applicable; obtain proof-of-delivery; handle exceptions; reconcile cash", "On-Time Delivery %, COD Cash Variance, Customer Rating, Failed Delivery Rate, Trips per Shift", 21),
        new("LM-FLT-001", "Fleet Coordinator", 3, 9_500m, 14_500m, "OPERATIONS", "HEAD-LOG-001", false, "Diploma", 3, "Fleet Management, Maintenance, Vehicle Compliance, Documentation, Cost Control", "Manages {Company}'s rider and van fleet — vehicles, maintenance, compliance, fuel", "WH-3", "Maintain vehicle and rider documentation; schedule preventive maintenance; manage fuel and parts spend; track accidents and claims; ensure DVLA compliance", "Fleet Availability %, Maintenance Cost per Vehicle, Compliance Rate, Accident Rate, Fuel Cost per km", 24),

        // ── Customer Operations ───────────────────────────────────────────────
        new("CS-MGR-001", "Customer Service Manager", 4, 20_000m, 30_000m, "CUSTOMER-OPS", "HEAD-CS-001", true, "Bachelor's Degree", 6, "Contact Centre Management, WFM, CSAT, Process Improvement, Coaching", "Manages a {Company} customer service department — voice, chat, email, social", "EC-6", "Run CS department P&L; manage team leads; deliver CSAT and AHT targets; drive process improvements; manage WFM and capacity; coach leaders", "CSAT, NPS, AHT, Schedule Adherence, First Contact Resolution, Cost-per-Contact", 27),
        new("CS-LD-001", "CS Team Lead", 3, 9_500m, 14_000m, "CUSTOMER-OPS", "CS-MGR-001", true, "Bachelor's Degree", 3, "Team Leadership, Coaching, Quality Monitoring, Escalation Handling, CRM Tools", "Leads a team of {Company} customer service agents", "EC-3", "Coach 10-15 agents; monitor quality; handle escalations; deliver team CSAT and AHT; run daily huddles; develop top performers", "Team CSAT, AHT, QA Score, Schedule Adherence, Agent Attrition", 24),
        new("CS-AGT-001", "Customer Service Agent - Voice", 2, 2_800m, 4_500m, "CUSTOMER-OPS", "CS-LD-001", false, "High School", 1, "Voice Customer Service, Active Listening, CRM, Order Management, Empathy", "Frontline agent handling inbound and outbound {Company} customer calls", "EC-2", "Answer customer calls; resolve order, delivery, and refund queries; log in CRM; escalate complex issues; uphold service standards; meet AHT and CSAT", "CSAT, AHT, FCR, QA Score, Adherence", 21),
        new("CS-AGT-002", "Customer Service Agent - Chat", 2, 2_800m, 4_500m, "CUSTOMER-OPS", "CS-LD-001", false, "High School", 1, "Chat Support, Multitasking, Typing Speed, CRM, Written Communication", "Frontline agent handling live-chat conversations on the {Company} website and app", "EC-2", "Handle concurrent chat sessions; resolve customer queries; document in CRM; escalate where needed; meet response-time and CSAT targets", "CSAT, Concurrent Chats, Response Time, Resolution Rate, QA Score", 21),
        new("CS-AGT-003", "Customer Service Agent - Email", 2, 2_800m, 4_500m, "CUSTOMER-OPS", "CS-LD-001", false, "High School", 1, "Email Support, Written Communication, CRM, Order Investigation, Process Adherence", "Frontline agent resolving customer queries through email and social tickets at {Company}", "EC-2", "Triage and respond to email tickets; investigate order issues; coordinate with ops/finance; close tickets within SLA; uphold tone-of-voice", "Tickets per Day, SLA Adherence, CSAT, Quality Score, Resolution Rate", 21),
        new("CS-ESC-001", "Escalation Specialist", 3, 9_500m, 14_500m, "CUSTOMER-OPS", "CS-MGR-001", false, "Bachelor's Degree", 3, "Complex Case Resolution, Negotiation, Compensation Frameworks, Cross-Functional Coordination, Empathy", "Resolves escalated customer cases that frontline {Company} agents cannot close", "EC-3", "Own escalated case queue; investigate complex disputes; coordinate with ops, vendor, and finance; apply compensation policy; recover at-risk customers", "Escalation Resolution Time, Customer Recovery Rate, Compensation Spend, Repeat Escalation Rate, NPS Recovery", 24),

        // ── Marketing ─────────────────────────────────────────────────────────
        new("MKT-001", "Senior Performance Marketer", 4, 20_000m, 32_000m, "MARKETING", "HEAD-MKT-001", false, "Bachelor's Degree", 6, "Performance Marketing, Google Ads, Meta Ads, ROAS, Attribution, Bid Management", "Senior marketer running paid acquisition channels for {Company}", "EC-6", "Own paid channel P&L (Google, Meta, TikTok); plan budgets; optimise bids and creatives; report ROAS; mentor junior marketers; partner with creative", "Blended ROAS, CAC, Channel CPC, Conversion Rate, New Buyer Volume", 27),
        new("MKT-002", "Performance Marketer", 3, 11_000m, 17_000m, "MARKETING", "MKT-001", false, "Bachelor's Degree", 3, "Paid Search, Paid Social, Analytics, Creative Briefing, Bid Optimisation", "Mid-level performance marketer managing day-to-day paid campaigns for {Company}", "EC-4", "Run daily paid campaigns; QA creative; manage feeds; optimise bids; report weekly performance; A/B test creatives and audiences", "Channel ROAS, CAC, CTR, Conversion Rate, Campaign Velocity", 24),
        new("MKT-003", "Brand Marketer", 3, 11_000m, 17_000m, "MARKETING", "HEAD-MKT-001", false, "Bachelor's Degree", 4, "Brand Strategy, Campaign Management, Above-the-Line, Sponsorships, PR", "Drives the {Company} brand through above-the-line campaigns, sponsorships, and PR", "EC-4", "Plan and execute brand campaigns; manage sponsorship and PR; brief creative; track brand awareness; coordinate with category and CRM teams", "Brand Awareness, Aided Recall, Share of Voice, Campaign Reach, Earned Media Value", 24),
        new("MKT-004", "Content Manager", 3, 10_000m, 15_500m, "MARKETING", "HEAD-MKT-001", false, "Bachelor's Degree", 3, "Content Strategy, Editorial, Copywriting, SEO Content, Photography Direction", "Owns the {Company} content engine — editorial calendar, on-site content, blog, video", "EC-4", "Plan editorial calendar; commission and edit content; manage SEO content; brief photography and video; grow organic traffic; partner with social and CRM", "Organic Traffic, Time-on-Site, Content-Driven Conversion, Editorial Cadence, Content Quality Score", 24),
        new("MKT-005", "Social Media Manager", 3, 9_500m, 14_500m, "MARKETING", "HEAD-MKT-001", false, "Bachelor's Degree", 3, "Social Media Strategy, Community Management, Influencer Marketing, Crisis Response, Analytics", "Runs {Company}'s organic social presence and influencer programme", "EC-4", "Own social calendar across IG/TikTok/X/Facebook; manage community; run influencer programme; respond to social CS escalations; report engagement", "Follower Growth, Engagement Rate, Social-Driven Sessions, Influencer ROI, Response Time", 24),
        new("MKT-006", "CRM Manager", 4, 20_000m, 30_000m, "MARKETING", "HEAD-MKT-001", true, "Bachelor's Degree", 5, "CRM Strategy, Lifecycle Marketing, Email/Push/SMS, Segmentation, Retention", "Owns retention, CRM, and lifecycle marketing for {Company} buyers", "EC-6", "Build retention strategy; design lifecycle journeys (welcome, repeat, win-back); segment audience; run email/push/SMS; lift repeat purchase and LTV", "Repeat Purchase Rate, Email/Push CTR, LTV, Churn Rate, CRM-Driven Revenue %", 27),

        // ── Tech / Product ────────────────────────────────────────────────────
        new("SE-FE-001", "Senior Software Engineer - Frontend", 4, 22_000m, 35_000m, "TECH", "VP-TECH-001", false, "Bachelor's Degree", 6, "React, TypeScript, Web Performance, Accessibility, Testing, Frontend Architecture", "Senior frontend engineer building the {Company} web storefront and seller portal", "EC-6", "Design and build frontend features; mentor mid-level engineers; review code; own performance and accessibility; partner with product and design", "Feature Velocity, Lighthouse Score, Bug Escape Rate, PR Review Quality, Tech Debt Closed", 27),
        new("SE-BE-001", "Senior Software Engineer - Backend", 4, 22_000m, 35_000m, "TECH", "VP-TECH-001", false, "Bachelor's Degree", 6, "Backend, Microservices, Distributed Systems, Databases, APIs, Reliability", "Senior backend engineer building and operating {Company} marketplace services", "EC-6", "Own backend services; design APIs; ensure reliability and performance; review code; mentor engineers; lead incident response", "Service Uptime, p95 Latency, Bug Escape Rate, Incident MTTR, Feature Velocity", 27),
        new("SE-MOB-001", "Senior Software Engineer - Mobile", 4, 22_000m, 35_000m, "TECH", "VP-TECH-001", false, "Bachelor's Degree", 6, "iOS / Android / React Native, Mobile Performance, App Store Release, Crash Analytics", "Senior mobile engineer building the {Company} buyer app", "EC-6", "Build and release the {Company} app; manage app store releases; reduce crashes; mentor mobile engineers; partner with product and design", "App Store Rating, Crash-Free Sessions, Release Cadence, Feature Velocity, Bug Escape Rate", 27),
        new("SE-001", "Software Engineer", 3, 11_000m, 17_500m, "TECH", "SE-BE-001", false, "Bachelor's Degree", 2, "Full-Stack, REST APIs, SQL, Git, Agile, Testing", "Mid-level engineer building features across the {Company} platform", "EC-4", "Build and ship features; write tests; participate in code review; respond to incidents; own small services or modules", "Sprint Velocity, Bug Escape Rate, Code Review Participation, Test Coverage, On-Call Performance", 24),
        new("DA-001", "Data Analyst", 3, 11_000m, 17_500m, "TECH", "VP-TECH-001", false, "Bachelor's Degree", 3, "SQL, Python, A/B Testing, Funnel Analysis, Looker / Tableau, Storytelling", "Analyses {Company} buyer, seller, and operational data to drive decisions", "EC-4", "Run funnel and cohort analyses; design A/B tests; build dashboards; partner with PMs and category managers; deliver weekly insight readouts", "Insight Adoption Rate, Dashboard Quality, Experiment Velocity, Data Quality Score, Stakeholder NPS", 24),
        new("BI-001", "BI Analyst", 3, 10_000m, 15_500m, "TECH", "VP-TECH-001", false, "Bachelor's Degree", 3, "BI Tools, SQL, Data Modelling, ETL, Reporting", "Builds and maintains {Company}'s BI dashboards and reporting layer", "EC-4", "Model data; build dashboards in Looker/Tableau; productionise weekly reports; partner with finance and ops; ensure data quality and freshness", "Dashboard Adoption, Data Freshness, Report Accuracy, Stakeholder NPS, Pipeline Uptime", 24),
        new("PM-001", "Product Manager", 4, 22_000m, 34_000m, "TECH", "VP-TECH-001", true, "Bachelor's Degree", 5, "Product Management, Roadmapping, Discovery, Experimentation, Stakeholder Management", "Owns a {Company} product area — buyer app, seller portal, payments, search, etc.", "EC-6", "Own area roadmap; run discovery; write specs; partner with engineering and design; ship and measure features; manage stakeholders", "Feature Adoption, North-Star Metric Movement, Sprint Predictability, Stakeholder NPS, Experiment Win Rate", 27),
        new("UX-001", "UX Designer", 3, 11_000m, 17_000m, "TECH", "PM-001", false, "Bachelor's Degree", 3, "User Research, Wireframing, Prototyping, Figma, Usability Testing", "Designs buyer and seller experiences across the {Company} web and mobile platform", "EC-4", "Run user research; produce wireframes and high-fidelity designs; prototype; run usability tests; partner with product and engineering; maintain design system", "Usability Scores, Design Adoption, Research Cadence, Design QA Pass Rate, Stakeholder NPS", 24),

        // ── Risk & Trust ──────────────────────────────────────────────────────
        new("RISK-001", "Risk & Fraud Analyst", 3, 10_000m, 15_500m, "RISK-TRUST", "CEO-001", false, "Bachelor's Degree", 3, "Fraud Analytics, Payments Risk, Rules Engines, SQL, Investigation", "Detects and investigates fraud across {Company} payments, orders, and accounts", "EC-4", "Tune fraud rules; investigate suspicious orders; manage chargebacks; partner with payments and CS; report fraud KPIs to leadership", "Chargeback Rate, Fraud Loss Rate, False Positive Rate, Investigation Closure Time", 24),
        new("TS-001", "Trust & Safety Officer", 3, 9_500m, 14_500m, "RISK-TRUST", "CEO-001", false, "Bachelor's Degree", 2, "Policy Enforcement, Marketplace Trust, Investigations, KYC, Documentation", "Enforces {Company} platform policy across sellers and buyers — counterfeits, abuse, banned goods", "EC-3", "Investigate policy violations; enforce sanctions on sellers; KYC high-risk accounts; coordinate with legal; document case outcomes", "Policy Action Cycle Time, Repeat Violation Rate, KYC Pass Rate, Buyer-Reported Issues per 1000 Orders", 24),

        // ── Finance ───────────────────────────────────────────────────────────
        new("PAY-001", "Payments Specialist", 3, 11_000m, 17_000m, "OPS-FINANCE", "FIN-MGR-001", false, "Bachelor's Degree", 3, "Payments Operations, Mobile Money, Cards, Reconciliation, Settlement", "Operates {Company}'s payments stack — MoMo, cards, wallet, COD reconciliation, settlement", "EC-4", "Reconcile payment gateways daily; monitor settlement; manage payment provider relationships; investigate failures; report payment KPIs", "Settlement Accuracy, Payment Failure Rate, Reconciliation Timeliness, Cost-per-Transaction", 24),
        new("FIN-MGR-001", "Finance Manager", 4, 22_000m, 35_000m, "FINANCE", "CEO-001", true, "Bachelor's Degree / ACCA / ICAG", 7, "Financial Management, IFRS, Management Accounts, Treasury, Tax, Audit", "Manages the {Company} finance function — reporting, vendor payouts, AP/AR, treasury, tax", "EC-7", "Oversee monthly close; run management reporting; manage vendor payouts and AP/AR; coordinate audit and tax; partner with leadership on unit economics", "Close Timeliness, Audit Findings, Vendor Payout SLA, Cash Position, Cost Variance", 27),
        new("ACC-001", "Accountant", 3, 9_500m, 14_500m, "FINANCE", "FIN-MGR-001", false, "Bachelor's Degree / Part ACCA", 3, "Bookkeeping, IFRS, Excel, ERP, Reconciliation, Journals", "Day-to-day accounting, journals, and reconciliations for {Company}", "EC-3", "Post journals; reconcile GL accounts; process AP/AR; support month-end close; assist with statutory filings", "Reconciliation Accuracy, Journal Turnaround, Month-End Deadline, Filing Compliance", 24),

        // ── People & Admin ────────────────────────────────────────────────────
        new("HR-001", "HR Officer", 3, 9_500m, 14_500m, "PEOPLE", "CEO-001", false, "Bachelor's Degree", 3, "HR Operations, Employee Relations, Payroll Inputs, Labour Law, HRIS", "HR generalist supporting {Company} employees across HQ, fulfilment, and rider workforce", "EC-3", "Manage employee relations; run onboarding/offboarding; submit payroll inputs; support performance cycles; maintain HRIS records", "Time-to-Onboard, Payroll Accuracy, Grievance Resolution Time, Engagement Score, Records Accuracy", 24),
        new("TA-001", "Talent Acquisition Specialist", 3, 9_500m, 14_500m, "PEOPLE", "HR-001", false, "Bachelor's Degree", 2, "Recruitment, Sourcing, Employer Branding, ATS, Interviewing", "Owns end-to-end recruitment for {Company} — corporate, fulfilment, and rider hiring", "EC-3", "Source candidates; screen applications; coordinate interviews; manage offers; build talent pipelines; track time-to-hire", "Time-to-Hire, Offer Acceptance, Pipeline Quality, 90-Day Retention, Hiring Manager NPS", 24),
        new("OFF-001", "Office Manager", 2, 4_000m, 6_500m, "PEOPLE", "HR-001", false, "Diploma", 3, "Office Administration, Vendor Management, Facilities, Travel, Procurement", "Runs day-to-day office operations and admin support for {Company} HQ", "EC-2", "Manage office facilities and vendors; coordinate travel and meetings; oversee admin assistants; manage office budget; support events", "Facilities Uptime, Vendor SLA, Office Budget Variance, Employee Admin NPS", 21)
    ];

    // ──────────────────────────────────────────────────────────────────────────
    // Curated 40-station catalogue for the Corporate tier — Head Office,
    // Engineering Centre, Fulfilment Centres in real industrial zones, Last-Mile
    // Hubs spread across major cities, Pickup / Locker Stations, plus Customer
    // Service / Vendor Support Hubs. {Company} placeholder substituted at row-emit
    // time; emails store ONLY the local part (the row factory appends the tenant
    // TLD at runtime). Phone format "+233 30 XXX XXXX" / "+233 24 XXX XXXX".
    // ──────────────────────────────────────────────────────────────────────────
    private static readonly StationSpec[] _ecommerceStations =
    [
        // 1 Head Office
        new("HQ-001", "Head Office - Accra (Spintex)", "Head Office", "Greater Accra", "Accra", "Spintex Road, Baatsona", 80, 250, "{Company} corporate headquarters housing executive, commercial, marketing, finance, and people functions", "+233 30 XXX XXXX", "headoffice"),

        // 1 Engineering Centre
        new("ENG-001", "Engineering Centre - Airport City", "Engineering Centre", "Greater Accra", "Accra", "Airport City, Liberation Road", 40, 120, "{Company} product, engineering, design, and data hub serving the {Company} platform", "+233 30 XXX XXXX", "engineering"),

        // 4 Fulfilment Centres
        new("FC-TEM-001", "Tema Fulfilment Centre", "Fulfilment Centre", "Greater Accra", "Tema", "Tema Industrial Area, Heavy Industrial Zone", 80, 250, "{Company}'s flagship fulfilment centre serving Greater Accra and feeding all southern last-mile hubs", "+233 30 XXX XXXX", "fc.tema"),
        new("FC-KSI-001", "Kumasi Fulfilment Centre", "Fulfilment Centre", "Ashanti", "Kumasi", "Asokwa Industrial Area", 50, 160, "{Company} fulfilment centre serving Ashanti, Bono, Bono East, and Ahafo Regions", "+233 32 XXX XXXX", "fc.kumasi"),
        new("FC-TKD-001", "Takoradi Fulfilment Centre", "Fulfilment Centre", "Western", "Takoradi", "Takoradi Industrial Area, Effia-Kuma", 30, 100, "{Company} fulfilment centre serving Western, Western North, and Central Regions", "+233 31 XXX XXXX", "fc.takoradi"),
        new("FC-TAM-001", "Tamale Fulfilment Centre", "Fulfilment Centre", "Northern", "Tamale", "Industrial Area, Vittin", 25, 80, "{Company} fulfilment centre serving Northern, North East, Savannah, Upper East, and Upper West Regions", "+233 37 XXX XXXX", "fc.tamale"),

        // ~15 Last-Mile Hubs
        new("LM-MAD-001", "Madina Last-Mile Hub", "Last-Mile Hub", "Greater Accra", "Accra", "Madina Market Road, Madina", 8, 25, "{Company} last-mile hub serving Madina, Adenta, and surrounding north-east Accra communities", "+233 24 XXX XXXX", "hub.madina"),
        new("LM-KAS-001", "Kasoa Last-Mile Hub", "Last-Mile Hub", "Central", "Kasoa", "Kasoa Old Barrier, Kasoa", 8, 25, "{Company} last-mile hub serving the fast-growing Kasoa and Awutu Senya East corridor", "+233 24 XXX XXXX", "hub.kasoa"),
        new("LM-ASH-001", "Ashaiman Last-Mile Hub", "Last-Mile Hub", "Greater Accra", "Ashaiman", "Ashaiman Roundabout", 8, 25, "{Company} last-mile hub serving Ashaiman and Tema New Town", "+233 24 XXX XXXX", "hub.ashaiman"),
        new("LM-OSU-001", "Osu Last-Mile Hub", "Last-Mile Hub", "Greater Accra", "Accra", "Oxford Street, Osu", 6, 20, "{Company} last-mile hub serving Osu, Cantonments, Labone, and Ridge", "+233 24 XXX XXXX", "hub.osu"),
        new("LM-EL-001", "East Legon Last-Mile Hub", "Last-Mile Hub", "Greater Accra", "Accra", "Boundary Road, East Legon", 6, 20, "{Company} last-mile hub serving East Legon, Adjiringanor, and surrounding affluent communities", "+233 24 XXX XXXX", "hub.eastlegon"),
        new("LM-LAP-001", "Lapaz Last-Mile Hub", "Last-Mile Hub", "Greater Accra", "Accra", "Lapaz Main Road, Lapaz", 8, 25, "{Company} last-mile hub serving Lapaz, Achimota, and northern Accra communities", "+233 24 XXX XXXX", "hub.lapaz"),
        new("LM-SPX-001", "Spintex Last-Mile Hub", "Last-Mile Hub", "Greater Accra", "Accra", "Spintex Junction, Baatsona", 6, 20, "{Company} last-mile hub serving Spintex, Sakumono, and Tema beach road communities", "+233 24 XXX XXXX", "hub.spintex"),
        new("LM-DAN-001", "Dansoman Last-Mile Hub", "Last-Mile Hub", "Greater Accra", "Accra", "Dansoman Last Stop, Dansoman", 6, 20, "{Company} last-mile hub serving Dansoman, Kaneshie, and western Accra", "+233 24 XXX XXXX", "hub.dansoman"),
        new("LM-ADM-001", "Adum Last-Mile Hub", "Last-Mile Hub", "Ashanti", "Kumasi", "Prempeh II Street, Adum", 8, 25, "{Company} last-mile hub serving Adum and central Kumasi business district", "+233 24 XXX XXXX", "hub.adum"),
        new("LM-ASK-001", "Asokwa Last-Mile Hub", "Last-Mile Hub", "Ashanti", "Kumasi", "Asokwa Roundabout, Kumasi", 6, 20, "{Company} last-mile hub serving Asokwa, Atonsu, and southern Kumasi", "+233 24 XXX XXXX", "hub.asokwa"),
        new("LM-TAM-001", "Tamale Last-Mile Hub", "Last-Mile Hub", "Northern", "Tamale", "Salaga Road, Tamale Central", 6, 20, "{Company} last-mile hub serving Tamale Metropolitan Area", "+233 24 XXX XXXX", "hub.tamale"),
        new("LM-CAP-001", "Cape Coast Last-Mile Hub", "Last-Mile Hub", "Central", "Cape Coast", "Commercial Street, Cape Coast", 5, 18, "{Company} last-mile hub serving Cape Coast, Elmina, and surrounding Central Region", "+233 24 XXX XXXX", "hub.capecoast"),
        new("LM-HO-001", "Ho Last-Mile Hub", "Last-Mile Hub", "Volta", "Ho", "Ho-Aflao Road, Ho Central", 5, 18, "{Company} last-mile hub serving Ho and Volta Region capital area", "+233 24 XXX XXXX", "hub.ho"),
        new("LM-SUN-001", "Sunyani Last-Mile Hub", "Last-Mile Hub", "Bono", "Sunyani", "Fiapre Road, Sunyani Central", 5, 18, "{Company} last-mile hub serving Sunyani and Bono Region", "+233 24 XXX XXXX", "hub.sunyani"),
        new("LM-KOF-001", "Koforidua Last-Mile Hub", "Last-Mile Hub", "Eastern", "Koforidua", "Hospital Road, Koforidua", 5, 18, "{Company} last-mile hub serving Koforidua and Eastern Region", "+233 24 XXX XXXX", "hub.koforidua"),

        // ~10 Pickup / Locker Stations
        new("PU-ACM-001", "Accra Mall Pickup Station", "Pickup Station", "Greater Accra", "Accra", "Accra Mall, Spintex Road", 2, 6, "{Company} in-mall pickup station for self-collection by Accra Mall shoppers", "+233 24 XXX XXXX", "pickup.accramall"),
        new("PU-WHM-001", "West Hills Mall Pickup Station", "Pickup Station", "Greater Accra", "Accra", "West Hills Mall, Weija", 2, 6, "{Company} pickup station inside West Hills Mall serving Weija, Kasoa, and surrounding suburbs", "+233 24 XXX XXXX", "pickup.westhills"),
        new("PU-JM-001", "Junction Mall Pickup Station", "Pickup Station", "Greater Accra", "Accra", "Junction Mall, Nungua", 2, 6, "{Company} pickup station at Junction Mall serving Nungua, Teshie, and Sakumono", "+233 24 XXX XXXX", "pickup.junctionmall"),
        new("PU-AC-001", "Achimota Pickup Station", "Pickup Station", "Greater Accra", "Accra", "Achimota Retail Centre, Achimota", 2, 6, "{Company} pickup station at Achimota Retail Centre", "+233 24 XXX XXXX", "pickup.achimota"),
        new("PU-LEG-001", "Legon Pickup Station", "Pickup Station", "Greater Accra", "Accra", "University of Ghana, Legon", 2, 6, "{Company} pickup station serving University of Ghana students and Legon residents", "+233 24 XXX XXXX", "pickup.legon"),
        new("PU-KCM-001", "Kumasi City Mall Pickup Station", "Pickup Station", "Ashanti", "Kumasi", "Kumasi City Mall, Asokwa", 2, 6, "{Company} pickup station inside Kumasi City Mall", "+233 24 XXX XXXX", "pickup.kumasimall"),
        new("PU-KNUST-001", "KNUST Pickup Station", "Pickup Station", "Ashanti", "Kumasi", "KNUST Campus, Kumasi", 2, 6, "{Company} pickup station serving KNUST students and staff", "+233 24 XXX XXXX", "pickup.knust"),
        new("LK-OSU-001", "Osu Locker Station", "Locker Station", "Greater Accra", "Accra", "Oxford Street, Osu", 0, 2, "{Company} 24/7 self-service locker station in Osu", "+233 24 XXX XXXX", "locker.osu"),
        new("LK-EL-001", "East Legon Locker Station", "Locker Station", "Greater Accra", "Accra", "A&C Mall, East Legon", 0, 2, "{Company} 24/7 self-service locker station inside A&C Mall, East Legon", "+233 24 XXX XXXX", "locker.eastlegon"),
        new("LK-AIR-001", "Airport City Locker Station", "Locker Station", "Greater Accra", "Accra", "Marina Mall, Airport City", 0, 2, "{Company} self-service locker station at Marina Mall, Airport City", "+233 24 XXX XXXX", "locker.airportcity"),
        new("PU-OBU-001", "Obuasi Pickup Station", "Pickup Station", "Ashanti", "Obuasi", "Main Street, Obuasi", 2, 6, "{Company} pickup station serving Obuasi mining community and surrounding towns", "+233 24 XXX XXXX", "pickup.obuasi"),
        new("PU-TEM-001", "Tema Community 1 Pickup Station", "Pickup Station", "Greater Accra", "Tema", "Community 1, Tema Central", 2, 6, "{Company} pickup station in central Tema serving Community 1-9 residents", "+233 24 XXX XXXX", "pickup.tema"),
        new("LK-MAD-001", "Madina Locker Station", "Locker Station", "Greater Accra", "Accra", "Madina Mall, Madina", 0, 2, "{Company} 24/7 self-service locker station at Madina Mall", "+233 24 XXX XXXX", "locker.madina"),

        // ~6 Customer Service / Vendor Support Hubs
        new("CS-ACC-001", "Accra Customer Service Hub", "Customer Service Hub", "Greater Accra", "Accra", "Independence Avenue, Ridge", 25, 80, "{Company}'s primary contact centre handling voice, chat, email, and social CS for Ghana", "+233 30 XXX XXXX", "cs.accra"),
        new("CS-KSI-001", "Kumasi Customer Service Hub", "Customer Service Hub", "Ashanti", "Kumasi", "Adum Commercial Centre, Kumasi", 12, 40, "{Company} regional customer service hub serving Ashanti, Bono, and middle-belt regions", "+233 32 XXX XXXX", "cs.kumasi"),
        new("VS-ACC-001", "Accra Vendor Support Centre", "Vendor Support Centre", "Greater Accra", "Accra", "Spintex Road, Baatsona", 15, 50, "{Company} vendor support centre — seller onboarding, training, and account management for Accra-based merchants", "+233 30 XXX XXXX", "vendors.accra"),
        new("VS-KSI-001", "Kumasi Vendor Support Centre", "Vendor Support Centre", "Ashanti", "Kumasi", "Adum, Kumasi", 8, 25, "{Company} vendor support centre for Ashanti and middle-belt sellers", "+233 32 XXX XXXX", "vendors.kumasi"),
        new("VS-TKD-001", "Takoradi Vendor Support Centre", "Vendor Support Centre", "Western", "Takoradi", "Market Circle, Takoradi", 5, 18, "{Company} vendor support centre for Western Region sellers", "+233 31 XXX XXXX", "vendors.takoradi"),
        new("VS-TAM-001", "Tamale Vendor Support Centre", "Vendor Support Centre", "Northern", "Tamale", "Salaga Road, Tamale", 5, 18, "{Company} vendor support centre for Northern Region sellers", "+233 37 XXX XXXX", "vendors.tamale")
    ];
}
