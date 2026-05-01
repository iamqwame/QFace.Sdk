namespace QimErp.Shared.DemoData.Industry.Profiles;

public sealed class NonProfitIndustryProfile : IIndustryProfile
{
    public string Code => "NONPROFIT";
    public string DisplayName => "Non-Profit & NGO";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "World Vision Ghana", "Plan International Ghana", "ActionAid Ghana",
        "Ghana Red Cross", "Compassion International Ghana", "CARE International Ghana",
        "Save the Children Ghana", "Oxfam Ghana", "WaterAid Ghana",
        "SOS Children's Villages Ghana", "Catholic Relief Services Ghana",
        "Child Rights International"
    ];

    public OrgHierarchySpec BuildOrgHierarchy(CompanyTier tier, int targetEmployees, int randomSeed)
    {
        var (units, distribution) = tier switch
        {
            CompanyTier.Startup    => (StartupUnits,    StartupDistribution),
            CompanyTier.SME        => (SmeUnits,        SmeDistribution),
            CompanyTier.Corporate  => (CorporateUnits,  CorporateDistribution),
            CompanyTier.NonProfit  => (NonProfitUnits,  NonProfitDistribution),
            _                      => (NonProfitUnits,  NonProfitDistribution)
        };
        return OrgHierarchyBuilder.Build(units, distribution, targetEmployees, randomSeed);
    }

    public IReadOnlyList<JobTitleSpec> JobTitles => _jobTitles;

    public StationLayout BuildStations(CompanyTier tier, int targetEmployees, int randomSeed)
    {
        // Corporate tier uses the curated NGO station catalogue verbatim — Country Office,
        // regional offices, field offices, sub-offices, and training centres mirroring how
        // World Vision / Plan / Care / Red Cross actually operate in Ghana. Other tiers fall
        // back to the procedural city-pool builder so smaller NGOs land with a reasonable shape.
        if (tier == CompanyTier.Corporate)
        {
            var hqRow = _nonprofitStations[0];
            var rest = _nonprofitStations.Skip(1).ToList();
            var branchTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Regional Office", "Training Centre"
            };
            return new StationLayout(
                Headquarters: hqRow,
                Branches: rest.Where(s => branchTypes.Contains(s.StationType)).ToList(),
                Satellites: rest.Where(s => !branchTypes.Contains(s.StationType)).ToList());
        }

        var rng = new Random(randomSeed);
        var hq = new StationSpec(
            Code: "HQ",
            Name: "Country Office",
            StationType: "Country Office",
            Region: "Greater Accra",
            City: "Accra",
            Address: "East Legon, Accra",
            CapacityMin: 30,
            CapacityMax: tier == CompanyTier.Corporate ? 400 : 150);

        var fieldCount = tier switch
        {
            CompanyTier.Startup   => 1,
            CompanyTier.SME       => Math.Max(2, targetEmployees / 100),
            CompanyTier.Corporate => Math.Max(4, Math.Min(15, targetEmployees / 80)),
            CompanyTier.NonProfit => Math.Max(2, targetEmployees / 100),
            _                     => 3
        };

        var fieldOffices = new List<StationSpec>(fieldCount);
        for (var i = 0; i < fieldCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            fieldOffices.Add(new StationSpec(
                Code: $"FO{i + 1:D3}",
                Name: $"{city} Field Office",
                StationType: "Field Office",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 5,
                CapacityMax: 40));
        }

        return new StationLayout(hq, fieldOffices, new List<StationSpec>());
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.010,
            [4] = 0.060,
            [3] = 0.300,
            [2] = 0.470,
            [1] = 0.160
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (7_000m, 14_000m),
            [4] = (5_000m, 10_000m),
            [3] = (3_500m,  8_500m),
            [2] = (2_500m,  5_000m),
            [1] = (1_500m,  3_000m)
        });

    // Curated job-title cohort buckets per CorporateUnit. Codes line up with the
    // _jobTitles catalogue below.
    private static readonly IReadOnlyList<string> ExecJobs       = ["CD-001", "DCD-001", "EA-001"];
    private static readonly IReadOnlyList<string> ProgJobs       = ["DOP-001", "HPQ-001", "PM-001", "SPO-001", "PO-001", "PC-001", "FO-001", "CM-001", "VC-001"];
    private static readonly IReadOnlyList<string> MelJobs        = ["HME-001", "MEM-001", "MEO-001", "DQ-001", "RO-001", "KM-001"];
    private static readonly IReadOnlyList<string> AdvCommsJobs   = ["HAD-001", "HCM-001", "AO-001", "PO-002", "CO-001", "MO-001", "DR-001", "RM-001"];
    private static readonly IReadOnlyList<string> GrantsJobs     = ["HGR-001", "GM-001", "GO-001", "CMP-001", "IA-001"];
    private static readonly IReadOnlyList<string> FinanceJobs    = ["DOF-001", "SA-001", "PA-001", "CSH-001"];
    private static readonly IReadOnlyList<string> HrJobs         = ["HRM-001", "HRO-001", "LD-001"];
    private static readonly IReadOnlyList<string> OpsJobs        = ["DO-001", "HPR-001", "PRM-001", "PROC-001", "HLG-001", "LC-001", "FL-001", "DRV-001", "ITO-001", "ICT-001", "OA-001", "REC-001"];
    private static readonly IReadOnlyList<string> SafetyJobs     = ["HSF-001", "SFP-001", "HSE-001", "SSO-001"];

    // Startup-tier (small CBO / pilot project): 3-unit shape.
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",  "Founder/Country Lead", null,      OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMS", "Programmes",           "FOUNDER", OrgUnitKind.Function,  ProgJobs),
        new("ADMIN",    "Admin & Finance",      "FOUNDER", OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]  = 0.20,
        ["PROGRAMS"] = 0.60,
        ["ADMIN"]    = 0.20
    };

    // SME-tier (mid-sized national NGO).
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",       "Country Office Leadership", null,    OrgUnitKind.Executive, ExecJobs),
        new("PROGRAMMES", "Programmes",                "EXEC",  OrgUnitKind.Function,  ProgJobs),
        new("MEL",        "Monitoring, Evaluation & Learning", "EXEC", OrgUnitKind.Function, MelJobs),
        new("GRANTS",     "Grants & Compliance",       "EXEC",  OrgUnitKind.Function,  GrantsJobs),
        new("FINANCE",    "Finance",                   "EXEC",  OrgUnitKind.Function,  FinanceJobs),
        new("OPS",        "Operations",                "EXEC",  OrgUnitKind.Function,  OpsJobs),
        new("HR",         "HR & People",               "EXEC",  OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]       = 0.06,
        ["PROGRAMMES"] = 0.45,
        ["MEL"]        = 0.08,
        ["GRANTS"]     = 0.08,
        ["FINANCE"]    = 0.10,
        ["OPS"]        = 0.15,
        ["HR"]         = 0.08
    };

    // Corporate-tier baseline OrgUnits — tier-1 INGO Country Office (think World Vision Ghana,
    // Plan International Ghana, Care International Ghana scale). Each unit carries rich
    // Description / Budget / CostCenter / Purpose / Phone / Email-local-part. The {Company}
    // placeholder is substituted with the actual tenant company name at row-emit time.
    // Budgets are GHS / annum; programmes 0.8M-10M, support 200k-1M consistent with INGO
    // operating budgets in Ghana (ECOWAS country with mid-size country office presence).
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC", "Country Office Leadership", null, OrgUnitKind.Executive, ExecJobs,
            Description: "Office of the Country Director and Senior Management Team — strategic leadership of {Company}'s Ghana programme",
            BudgetMin: 600_000m, BudgetMax: 1_200_000m,
            CostCenter: "CC-EXEC-001",
            Purpose: "Set country strategy; represent {Company} to government, donors, and civil society; lead the Senior Management Team; uphold programme quality and stewardship of donor resources",
            Phone: "+233 30 XXX XXXX", Email: "countryoffice"),
        new("PROGRAMMES", "Programmes", "EXEC", OrgUnitKind.Function, ProgJobs,
            Description: "Designs and delivers {Company}'s programme portfolio across education, health, WASH, livelihoods, and child protection",
            BudgetMin: 3_000_000m, BudgetMax: 10_000_000m,
            CostCenter: "CC-PROG-001",
            Purpose: "Achieve {Company}'s strategic outcomes for vulnerable communities, on donor commitments, on time, on budget",
            Phone: "+233 30 XXX XXXX", Email: "programmes"),
        new("MEL", "Monitoring, Evaluation & Learning", "EXEC", OrgUnitKind.Function, MelJobs,
            Description: "Generates evidence on programme performance, drives learning, and ensures donor reporting integrity for {Company}",
            BudgetMin: 400_000m, BudgetMax: 900_000m,
            CostCenter: "CC-MEL-001",
            Purpose: "Provide credible, timely evidence of {Company}'s impact; embed adaptive management and learning across the country programme",
            Phone: "+233 30 XXX XXXX", Email: "mel"),
        new("ADVOCACY", "Advocacy & Communications", "EXEC", OrgUnitKind.Function, AdvCommsJobs,
            Description: "Advocacy, policy influence, public engagement, donor relations, and communications for {Company} Ghana",
            BudgetMin: 300_000m, BudgetMax: 800_000m,
            CostCenter: "CC-ADV-001",
            Purpose: "Influence policy and practice in favour of marginalised communities; protect and grow {Company}'s brand, voice, and donor base",
            Phone: "+233 30 XXX XXXX", Email: "advocacy"),
        new("GRANTS", "Grants & Compliance", "EXEC", OrgUnitKind.Function, GrantsJobs,
            Description: "Grant management, sub-grantee oversight, donor compliance, and internal audit for {Company}'s funded portfolio",
            BudgetMin: 250_000m, BudgetMax: 700_000m,
            CostCenter: "CC-GR-001",
            Purpose: "Ensure {Company} meets all donor compliance obligations (USAID, FCDO, EU, GFFO, GAC); manage sub-grants and partner risk",
            Phone: "+233 30 XXX XXXX", Email: "grants"),
        new("FINANCE", "Finance", "EXEC", OrgUnitKind.Function, FinanceJobs,
            Description: "Financial accounting, treasury, donor financial reporting, and budgeting for {Company}'s country programme",
            BudgetMin: 250_000m, BudgetMax: 600_000m,
            CostCenter: "CC-FIN-001",
            Purpose: "Produce accurate {Company} financial statements; manage donor cash flow; deliver compliant donor financial reports; safeguard assets",
            Phone: "+233 30 XXX XXXX", Email: "finance"),
        new("HR", "HR & People", "EXEC", OrgUnitKind.Function, HrJobs,
            Description: "Talent acquisition, learning & development, total rewards, and staff care for {Company}'s national and field workforce",
            BudgetMin: 200_000m, BudgetMax: 500_000m,
            CostCenter: "CC-HR-001",
            Purpose: "Attract and retain mission-driven talent; build leadership capability; promote staff wellbeing and a values-led culture across {Company} Ghana",
            Phone: "+233 30 XXX XXXX", Email: "hr"),
        new("OPS", "Operations", "EXEC", OrgUnitKind.Function, OpsJobs,
            Description: "Procurement, logistics, fleet, IT, and office administration enabling {Company}'s programme delivery across Ghana",
            BudgetMin: 400_000m, BudgetMax: 1_000_000m,
            CostCenter: "CC-OPS-001",
            Purpose: "Deliver value-for-money procurement; keep field teams supplied, connected, and mobile; provide reliable IT and office services",
            Phone: "+233 30 XXX XXXX", Email: "operations"),
        new("SAFEGUARDING", "Safeguarding & Security", "EXEC", OrgUnitKind.Function, SafetyJobs,
            Description: "Safeguarding (child / adult / PSEA), staff safety, security risk management, and duty-of-care for {Company}'s personnel and beneficiaries",
            BudgetMin: 200_000m, BudgetMax: 500_000m,
            CostCenter: "CC-SS-001",
            Purpose: "Protect children, communities, and staff from harm; ensure {Company} meets donor and global safeguarding and security standards",
            Phone: "+233 30 XXX XXXX", Email: "safeguarding")
    ];

    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]         = 0.04,
        ["PROGRAMMES"]   = 0.45,
        ["MEL"]          = 0.07,
        ["ADVOCACY"]     = 0.06,
        ["GRANTS"]       = 0.06,
        ["FINANCE"]      = 0.08,
        ["HR"]           = 0.05,
        ["OPS"]          = 0.13,
        ["SAFEGUARDING"] = 0.06
    };

    // NonProfit-tier — same baseline shape as Corporate so the seeded NGO walks like an INGO.
    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits = CorporateUnits;
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = CorporateDistribution;

    // ────────────────────────────────────────────────────────────────────────
    // Job-title catalogue — INGO Country Office grade, NP-1 (entry) to NP-8
    // (Country Director). Pay bands are GHS / month; AnnualLeave 21-30 days
    // mirroring NGO sector (more generous than corporate). {Company} placeholder
    // substituted at row-emit time.
    // ────────────────────────────────────────────────────────────────────────
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // ── Executive / Country Office Leadership ────────────────────────────
        new("CD-001", "Country Director", 5, 35_000m, 60_000m, "EXEC", null, true, "Master's Degree", 15, "INGO Leadership, Strategic Planning, Donor Relations, Government Engagement, Programme Strategy", "Chief executive accountable for {Company}'s overall Ghana strategy, programmes, partnerships, and stewardship", "NP-8", "Set country strategy; represent {Company} to government, donors, UN, and civil society; chair the Senior Management Team; sign-off on grant agreements; uphold safeguarding and security culture", "Donor Funding Secured, Programme Quality Score, Audit Opinion, Staff Engagement Score, Safeguarding Compliance, Government Relationships", 30),
        new("DCD-001", "Deputy Country Director", 5, 30_000m, 50_000m, "EXEC", "CD-001", true, "Master's Degree", 12, "Programme Leadership, Operations Oversight, Donor Engagement, People Leadership", "Second-in-command supporting the Country Director and deputising in their absence at {Company}", "NP-7", "Oversee Programmes / Operations portfolios; deputise for CD with donors and government; lead SMT meetings; manage strategic initiatives; mentor heads of department", "Programme delivery rate, SMT effectiveness, Donor satisfaction, Staff retention, Strategic initiative completion", 30),
        new("EA-001", "Executive Assistant to Country Director", 3, 7_000m, 11_000m, "EXEC", "CD-001", false, "Bachelor's Degree", 4, "Executive Coordination, Confidentiality, Travel Logistics, Stakeholder Management, Microsoft 365", "Senior support managing the Country Director's office, diary, and executive coordination at {Company}", "NP-4", "Manage CD calendar and travel; coordinate SMT meetings; prepare briefing notes; handle confidential correspondence; liaise with regional and global offices", "Schedule efficiency, Briefing quality, Confidentiality compliance, SMT meeting effectiveness", 27),

        // ── Programmes Leadership ─────────────────────────────────────────────
        new("DOP-001", "Director of Programmes", 5, 32_000m, 55_000m, "PROGRAMMES", "CD-001", true, "Master's Degree", 12, "Programme Strategy, Theory of Change, Donor Engagement, Sectoral Expertise, People Leadership", "Leads design, quality, and delivery of {Company}'s entire programme portfolio across education, health, WASH, livelihoods, and protection", "NP-8", "Own country programme strategy; chair programme review meetings; engage institutional donors; lead proposal development; ensure technical quality across sectors", "Programme delivery rate, New funding secured, Donor satisfaction, Beneficiary reach, Programme Quality Score", 30),
        new("HPQ-001", "Head of Programme Quality", 4, 18_000m, 30_000m, "PROGRAMMES", "DOP-001", true, "Master's Degree", 9, "Programme Design, Quality Assurance, Theory of Change, Sectoral Standards, Mentoring", "Leads {Company}'s programme quality function — standards, technical advisory, and adaptive management", "NP-6", "Maintain programme quality framework; lead technical advisors; review project designs and reports; embed Core Humanitarian Standard; coach project teams", "Programme Quality Score, Design review turnaround, Technical advisory satisfaction, CHS compliance", 27),
        new("PM-001", "Programme Manager", 4, 16_000m, 28_000m, "PROGRAMMES", "DOP-001", true, "Bachelor's Degree", 7, "Project Management, Logframes, Budget Management, Donor Reporting, Team Leadership", "Manages a {Company} multi-million-cedi programme portfolio (e.g. education, WASH, livelihoods) end-to-end", "NP-6", "Lead programme implementation; manage budget and burn rate; submit donor narrative reports; supervise project coordinators and field officers; engage local government", "Burn Rate, Activity Completion Rate, Donor Report Quality, Beneficiaries Reached, Variance to Plan", 27),
        new("SPO-001", "Senior Programme Officer", 3, 11_000m, 16_000m, "PROGRAMMES", "PM-001", false, "Bachelor's Degree", 5, "Project Implementation, Stakeholder Engagement, Reporting, Sectoral Knowledge", "Senior implementer leading components of {Company} programmes and mentoring junior officers", "NP-5", "Lead assigned project components; coordinate sub-grantees; quality-check field deliverables; draft donor reports; mentor programme officers; represent {Company} in district fora", "Activity completion, Sub-grantee performance, Report quality, Mentoring effectiveness", 27),
        new("PO-001", "Programme Officer", 3, 8_000m, 13_000m, "PROGRAMMES", "PM-001", false, "Bachelor's Degree", 3, "Project Implementation, Community Engagement, Reporting, Logframes", "Implements assigned project activities and reports against logframe targets for {Company}", "NP-4", "Plan and run project activities; track outputs against indicators; submit weekly and monthly reports; engage community structures and local partners; support data collection", "Activity completion rate, Output target achievement, Report timeliness, Community satisfaction", 24),
        new("PC-001", "Project Coordinator", 3, 9_000m, 14_000m, "PROGRAMMES", "PM-001", false, "Bachelor's Degree", 4, "Project Coordination, Workplan Management, Partner Engagement, Reporting", "Coordinates {Company} project workplans, partners, and field teams across geographic areas", "NP-5", "Coordinate project workplans across districts; supervise field officers; align with local partners and government; track milestones; submit consolidated reports", "Workplan adherence, Partner engagement quality, Milestone achievement, Field team productivity", 27),
        new("FO-001", "Field Officer", 3, 7_000m, 11_000m, "PROGRAMMES", "PC-001", false, "Diploma / Bachelor's Degree", 2, "Community Mobilisation, Activity Delivery, Data Collection, Local Languages", "Frontline implementer delivering {Company} project activities directly with target communities", "NP-3", "Run trainings, sensitisations, and project activities; mobilise communities; collect monitoring data; submit field reports; liaise with chiefs, assembly members, and CBOs", "Activities delivered, Beneficiaries reached, Data collection accuracy, Community feedback score", 24),
        new("CM-001", "Community Mobiliser", 2, 4_500m, 7_000m, "PROGRAMMES", "FO-001", false, "Diploma / SHS", 1, "Community Mobilisation, Local Languages, Facilitation, Communication", "Mobilises target communities for {Company} programme activities, meetings, and behaviour-change campaigns", "NP-2", "Mobilise community members for project events; facilitate community dialogues; support household-level outreach; record participation; relay community feedback", "Mobilisation turnout, Event facilitation quality, Outreach coverage, Feedback timeliness", 21),
        new("VC-001", "Volunteer Coordinator", 3, 7_500m, 12_000m, "PROGRAMMES", "PM-001", false, "Bachelor's Degree", 3, "Volunteer Management, Training, Coordination, Recordkeeping", "Recruits, trains, deploys, and supports {Company}'s community-based volunteer cadre", "NP-4", "Recruit and onboard volunteers; deliver volunteer training; coordinate field deployment; maintain volunteer records; manage stipends and recognition", "Active volunteer roster size, Training completion, Volunteer retention, Field coverage", 24),

        // ── Monitoring, Evaluation & Learning ────────────────────────────────
        new("HME-001", "Head of M&E / Director of MEL", 4, 18_000m, 30_000m, "MEL", "DOP-001", true, "Master's Degree", 9, "M&E Frameworks, Theory of Change, Donor Indicators, Data Systems, Evaluation Design", "Leads {Company}'s monitoring, evaluation, and learning function across the country programme", "NP-6", "Own MEL strategy and country results framework; design and commission evaluations; ensure donor indicator compliance; lead learning agenda; manage MEL team", "Indicator compliance rate, Evaluation quality, Learning product uptake, Data system uptime", 27),
        new("MEM-001", "M&E Manager", 4, 15_000m, 24_000m, "MEL", "HME-001", true, "Master's Degree", 6, "M&E Methodology, KoBo / DHIS2, SPSS / Stata, Indicator Tracking, Evaluation Management", "Manages day-to-day MEL operations across {Company} projects, including data systems and reporting", "NP-5", "Maintain indicator performance tracking tables; manage data collection rounds; supervise M&E officers; commission and quality-assure evaluations; produce MEL reports", "IPTT accuracy, Data round completion, Evaluation TOR quality, Report timeliness", 27),
        new("MEO-001", "M&E Officer", 3, 8_000m, 13_000m, "MEL", "MEM-001", false, "Bachelor's Degree", 3, "Data Collection, KoBo Toolbox, Power BI / Excel, Survey Design, Reporting", "Collects, cleans, and analyses programme data for {Company} project teams and donor reports", "NP-4", "Run routine data collection; clean and analyse data; populate IPTT; support donor report preparation; train field staff on data tools; visit field sites for verification", "Data quality score, IPTT freshness, Field verification visits, Report contributions", 24),
        new("DQ-001", "Data Quality Officer", 3, 8_500m, 13_500m, "MEL", "MEM-001", false, "Bachelor's Degree", 3, "Data Quality Audit, Data Cleaning, Verification, Statistical Software", "Owns data integrity for {Company} programmes — running data quality assessments and remediation", "NP-4", "Conduct data quality audits across projects; reconcile field data with reported figures; document findings; train teams on data standards; support donor DQAs", "DQA completion rate, Data discrepancy rate, Remediation closure, Donor DQA outcomes", 24),
        new("RO-001", "Research Officer", 3, 8_000m, 13_000m, "MEL", "HME-001", false, "Master's Degree", 3, "Research Methods, Qualitative & Quantitative Analysis, Academic Writing, Ethics", "Designs and conducts formative and operational research to inform {Company} programmes", "NP-4", "Design research studies; secure ethics clearance; manage data collection and analysis; produce research briefs and academic outputs; disseminate findings", "Studies completed, Research brief quality, Ethics compliance, Stakeholder uptake", 24),
        new("KM-001", "Knowledge Management Officer", 3, 8_000m, 12_500m, "MEL", "HME-001", false, "Bachelor's Degree", 3, "Knowledge Management, SharePoint, Learning Products, Documentation, Storytelling", "Captures, curates, and disseminates programme learning and knowledge products for {Company}", "NP-4", "Curate KM platform; produce learning briefs and case studies; facilitate after-action reviews; document best practices; support proposal development with evidence", "KM platform usage, Learning products published, AAR participation, Reuse in proposals", 24),

        // ── Advocacy & Communications ─────────────────────────────────────────
        new("HAD-001", "Head of Advocacy", 4, 16_000m, 28_000m, "ADVOCACY", "CD-001", true, "Master's Degree", 8, "Policy Influence, Government Relations, Coalition Building, Advocacy Strategy", "Leads {Company}'s advocacy and policy-influencing strategy at national level", "NP-6", "Set advocacy strategy; engage Parliament, MDAs, and civil society coalitions; influence policy and budget allocations; represent {Company} in national platforms", "Policy wins, Coalition leadership, Government engagement frequency, Media reach on advocacy", 27),
        new("HCM-001", "Head of Communications", 4, 16_000m, 27_000m, "ADVOCACY", "CD-001", true, "Master's Degree", 8, "Strategic Communications, Brand Management, Media Relations, Digital Strategy", "Leads {Company}'s strategic communications, brand, and external storytelling for Ghana", "NP-6", "Own communications strategy; protect and grow {Company} brand; manage media relations; lead digital channels; produce signature content; coach SMT on messaging", "Brand sentiment, Media coverage value, Digital reach, Audience growth, Content quality", 27),
        new("AO-001", "Advocacy Officer", 3, 8_500m, 13_500m, "ADVOCACY", "HAD-001", false, "Bachelor's Degree", 3, "Policy Briefs, Stakeholder Engagement, Coalition Work, Campaigns", "Drives day-to-day advocacy initiatives and stakeholder engagement for {Company}", "NP-4", "Draft policy briefs and position papers; engage MDAs and Parliamentary committees; coordinate coalition work; organise advocacy events; track policy commitments", "Policy briefs produced, Stakeholder meetings, Coalition contributions, Advocacy event reach", 24),
        new("PO-002", "Policy Officer", 3, 8_500m, 13_500m, "ADVOCACY", "HAD-001", false, "Master's Degree", 3, "Policy Analysis, Legislative Tracking, Research, Briefing", "Analyses policy and legislation relevant to {Company}'s mandate and produces evidence-based briefings", "NP-4", "Track relevant policy and legislation; produce policy analyses; brief CD and SMT; support coalition positions; maintain policy register", "Policy register currency, Brief quality, Brief turnaround, Decision-maker citations", 24),
        new("CO-001", "Communications Officer", 3, 7_500m, 12_500m, "ADVOCACY", "HCM-001", false, "Bachelor's Degree", 3, "Content Creation, Social Media, Press Releases, Photography, Storytelling", "Produces communications content and manages {Company} digital channels day-to-day", "NP-4", "Write press releases and stories; manage social media calendar; produce photo and video content; maintain website; cover field events; ensure brand compliance", "Content output, Engagement rate, Audience growth, Brand compliance score", 24),
        new("MO-001", "Media Officer", 3, 8_000m, 13_000m, "ADVOCACY", "HCM-001", false, "Bachelor's Degree", 3, "Press Relations, Media Pitching, Interview Coordination, Crisis Communications", "Manages {Company}'s media relationships, journalist briefings, and press coverage", "NP-4", "Pitch stories to media; coordinate interviews; brief spokespeople; track press coverage; manage media database; support crisis communications", "Press hits secured, Tone of coverage, Spokesperson briefing quality, Media database currency", 24),
        new("DR-001", "Donor Relations Officer", 3, 9_000m, 14_000m, "ADVOCACY", "HCM-001", false, "Bachelor's Degree", 3, "Donor Stewardship, Pitch Decks, Reporting, Relationship Management", "Cultivates and stewards {Company}'s relationships with institutional and individual donors", "NP-5", "Manage donor stewardship calendar; produce donor updates and impact reports; coordinate donor visits; maintain donor CRM; support proposal storytelling", "Donor retention, Steward touchpoints, Donor visit satisfaction, CRM completeness", 24),
        new("RM-001", "Resource Mobilisation Officer", 3, 9_500m, 15_000m, "ADVOCACY", "HCM-001", false, "Bachelor's Degree", 4, "Proposal Writing, Donor Mapping, Concept Notes, Pipeline Management", "Identifies funding opportunities and develops competitive proposals for {Company}", "NP-5", "Map donor priorities; develop concept notes and full proposals; coordinate proposal teams; maintain funding pipeline; track go/no-go decisions", "Proposals submitted, Win rate, Pipeline value, Concept note quality", 27),

        // ── Grants & Compliance ───────────────────────────────────────────────
        new("HGR-001", "Head of Grants & Compliance", 4, 17_000m, 28_000m, "GRANTS", "CD-001", true, "Master's Degree", 8, "Grant Management, Donor Compliance, Sub-grant Oversight, Risk Management", "Leads {Company}'s grant management and donor compliance function across the active portfolio", "NP-6", "Own grant lifecycle; oversee sub-grant management; ensure compliance with USAID, FCDO, EU, GAC, GFFO regulations; lead risk register; coordinate donor audits", "Compliance rating, Sub-grantee performance, Audit findings, Risk closure rate, Donor disallowances", 27),
        new("GM-001", "Grants Manager", 4, 14_000m, 22_000m, "GRANTS", "HGR-001", true, "Bachelor's Degree", 6, "Grant Administration, Donor Regulations, Reporting, Sub-grant Management", "Manages {Company}'s grant portfolio day-to-day, ensuring on-time, compliant donor reporting", "NP-5", "Maintain grant tracker; coordinate narrative and financial reports; review sub-grantee submissions; manage modifications and no-cost extensions; brief programme teams on compliance", "Report on-time rate, Sub-grantee reporting quality, Modification turnaround, Disallowed cost rate", 27),
        new("GO-001", "Grants Officer", 3, 8_000m, 13_000m, "GRANTS", "GM-001", false, "Bachelor's Degree", 3, "Grants Tracking, Donor Reporting, Sub-grant Coordination, Documentation", "Supports day-to-day grants administration and donor reporting for {Company}", "NP-4", "Maintain grants documentation; consolidate report inputs; track sub-grantee deliverables; flag compliance risks; support proposal budget development", "Tracker accuracy, Documentation completeness, Sub-grantee follow-up, Compliance flags raised", 24),
        new("CMP-001", "Compliance Officer", 3, 8_500m, 13_500m, "GRANTS", "HGR-001", false, "Bachelor's Degree", 3, "Donor Regulations, Internal Controls, Policy Review, Investigations Support", "Monitors {Company}'s adherence to donor regulations and internal policies", "NP-4", "Conduct compliance reviews; maintain compliance register; investigate flagged transactions; train staff on donor rules; support external audits", "Reviews completed, Issues escalated, Audit findings, Training reach", 24),
        new("IA-001", "Internal Auditor", 4, 13_000m, 21_000m, "GRANTS", "HGR-001", false, "Master's Degree / ACCA / ICAG / CIA", 6, "Risk-Based Auditing, IIA Standards, Donor Audit, Report Writing", "Independent assurance over {Company}'s controls, processes, and donor compliance", "NP-5", "Plan and execute risk-based audits; report to CD and audit committee; track management actions; support external donor audits; advise on control improvements", "Audit plan completion, High-risk findings closed, Donor audit outcomes, Repeat findings", 27),

        // ── Finance ───────────────────────────────────────────────────────────
        new("DOF-001", "Director of Finance", 5, 30_000m, 50_000m, "FINANCE", "CD-001", true, "Master's Degree / ACCA / ICAG / CPA", 12, "Financial Leadership, Donor Reporting, Audit Management, Treasury, IPSAS / IFRS", "Executive leading {Company}'s finance function — financial management, donor reporting, audit, and treasury", "NP-7", "Own country financial strategy; lead audit; manage cash flow and FX; oversee donor financial reporting; safeguard assets; chair finance review meetings", "Audit opinion, Cash position, Donor financial report quality, Cost recovery rate, Disallowed costs", 30),
        new("SA-001", "Senior Accountant", 3, 9_500m, 15_000m, "FINANCE", "DOF-001", false, "Bachelor's Degree / ACCA Part-Qualified", 5, "Financial Accounting, IPSAS, Reconciliations, Donor Reporting, ERP", "Senior accountant managing month-end close, reconciliations, and donor financial reports for {Company}", "NP-5", "Lead month-end close; reconcile bank and intercompany accounts; prepare donor financial reports; review project accountant work; support audit", "Close timeliness, Reconciliation accuracy, Donor report quality, Audit findings", 27),
        new("PA-001", "Project Accountant", 3, 8_000m, 13_000m, "FINANCE", "SA-001", false, "Bachelor's Degree", 3, "Project Accounting, Donor Budgets, Cost Allocation, Reconciliations", "Owns the books for assigned {Company} project portfolio — budgets, burn, and donor financials", "NP-4", "Maintain project ledgers; allocate shared costs; produce monthly project financials; support project managers on burn rate; prepare donor financial reports", "Project burn variance, Cost allocation accuracy, Report timeliness, Project audit findings", 24),
        new("CSH-001", "Cashier", 2, 4_000m, 6_500m, "FINANCE", "SA-001", false, "Diploma / HND", 1, "Cash Handling, Petty Cash, Bank Lodgements, Documentation", "Handles {Company}'s petty cash, bank lodgements, and field cash advances", "NP-2", "Process petty cash disbursements; reconcile cash float daily; lodge cheques; manage advance retirement; maintain cash records", "Cash float accuracy, Lodgement timeliness, Advance retirement rate, Documentation completeness", 21),

        // ── HR & People ───────────────────────────────────────────────────────
        new("HRM-001", "HR Manager", 4, 14_000m, 22_000m, "HR", "CD-001", true, "Bachelor's Degree / CIHRM / SHRM", 6, "HR Operations, Recruitment, Performance Management, Labour Law, Total Rewards", "Manages {Company}'s day-to-day HR operations, recruitment, and employee relations in Ghana", "NP-5", "Manage end-to-end recruitment; administer payroll inputs; coordinate performance reviews; handle employee relations; ensure labour law compliance; support staff care", "Time-to-hire, Onboarding satisfaction, Payroll accuracy, Grievance resolution, Turnover", 27),
        new("HRO-001", "HR Officer", 3, 7_500m, 12_500m, "HR", "HRM-001", false, "Bachelor's Degree", 3, "Recruitment, HRIS, Employee Records, Onboarding, Benefits Administration", "Supports day-to-day HR transactions, recruitment, and staff records for {Company}", "NP-4", "Post job adverts; coordinate interviews; onboard new hires; maintain HRIS records; administer SSNIT, pension, and medical benefits; produce HR reports", "Time-to-hire, Records accuracy, Onboarding NPS, Benefit query resolution", 24),
        new("LD-001", "Learning & Development Officer", 3, 8_000m, 13_000m, "HR", "HRM-001", false, "Bachelor's Degree", 3, "Learning Design, LMS Administration, Facilitation, Needs Assessment", "Coordinates {Company}'s training programmes, leadership development, and e-learning platform", "NP-4", "Conduct training needs analysis; manage LMS; coordinate internal and external training; track training hours; evaluate impact; support induction programmes", "Training hours per staff, Completion rate, Learning satisfaction, Certification achievement", 24),

        // ── Operations (Procurement / Logistics / IT / Admin) ────────────────
        new("DO-001", "Director of Operations", 5, 30_000m, 50_000m, "OPS", "CD-001", true, "Master's Degree", 12, "Operations Leadership, Procurement, Logistics, IT, Risk", "Executive leading {Company}'s operations — procurement, logistics, IT, fleet, and admin", "NP-7", "Own operations strategy; ensure value-for-money procurement; manage operational risk; oversee IT and fleet; safeguard assets; engage donors on operational compliance", "Procurement cycle time, Cost savings, Fleet uptime, IT availability, Operational audit findings", 30),
        new("HPR-001", "Head of Procurement", 4, 15_000m, 24_000m, "OPS", "DO-001", true, "Bachelor's Degree / CIPS", 7, "Procurement Strategy, Tender Management, Donor Procurement Rules, Vendor Management", "Leads {Company}'s procurement function and ensures donor-compliant sourcing", "NP-6", "Own procurement plan; chair tender committees; ensure donor procurement rule compliance; manage vendor master; drive savings; mitigate procurement risk", "Cycle time, Cost savings, Donor compliance, Vendor performance, Audit findings", 27),
        new("PRM-001", "Procurement Manager", 4, 12_000m, 19_000m, "OPS", "HPR-001", true, "Bachelor's Degree / CIPS", 5, "Tender Management, Contract Administration, Vendor Evaluation, Negotiation", "Manages {Company}'s procurement pipeline and ensures donor-compliant sourcing decisions", "NP-5", "Run RFQ and tender processes; evaluate bids; administer purchase orders; manage vendor register; ensure donor compliance; track contract deliverables", "Cycle time, Bid evaluation quality, Donor compliance, Vendor SLA adherence", 27),
        new("PROC-001", "Procurement Officer", 3, 7_500m, 12_500m, "OPS", "PRM-001", false, "Bachelor's Degree", 2, "Sourcing, Quotation, Contract Administration, Documentation", "Executes day-to-day procurement transactions for {Company}'s programme and operational needs", "NP-4", "Issue RFQs; tabulate bids; raise purchase orders; manage delivery and acceptance; maintain procurement files; flag compliance issues", "Cycle time, Documentation completeness, Delivery on-time rate, Compliance flags", 24),
        new("HLG-001", "Head of Logistics", 4, 14_000m, 22_000m, "OPS", "DO-001", true, "Bachelor's Degree", 6, "Logistics, Fleet, Warehousing, Asset Management", "Leads {Company}'s logistics, fleet, and asset management across all offices", "NP-5", "Own logistics strategy; manage fleet and drivers; oversee warehousing; maintain asset register; coordinate field movement; ensure safety standards", "Fleet uptime, Asset register accuracy, Field movement on-time, Incident rate", 27),
        new("LC-001", "Logistics Coordinator", 3, 8_000m, 13_000m, "OPS", "HLG-001", false, "Bachelor's Degree", 3, "Logistics Coordination, Fleet, Stock, Travel", "Coordinates {Company}'s logistics activities — fleet, stock, travel, and field movement", "NP-4", "Schedule fleet and trips; coordinate field travel; manage warehouse receipts and issues; track stock; support event logistics", "Fleet utilisation, Trip on-time rate, Stock accuracy, Event logistics quality", 24),
        new("FL-001", "Fleet Officer", 3, 7_000m, 11_000m, "OPS", "HLG-001", false, "Diploma / Bachelor's Degree", 2, "Fleet Management, Vehicle Maintenance, Driver Coordination, Fuel Tracking", "Manages {Company}'s vehicle fleet, drivers, fuel, and maintenance schedules", "NP-3", "Maintain vehicle log; schedule services and renewals; manage fuel drawdown; supervise drivers; track mileage; ensure roadworthiness compliance", "Fleet uptime, Service compliance, Fuel cost per km, Incident rate", 24),
        new("DRV-001", "Driver", 2, 3_500m, 5_500m, "OPS", "FL-001", false, "SHS + Class C / D Licence", 2, "Defensive Driving, Vehicle Care, Route Planning, Local Languages", "Provides safe and reliable transport for {Company} staff, visitors, and field movements", "NP-2", "Drive staff and supplies safely; maintain vehicle log; complete daily checks; report defects; support field movement; observe security protocols", "Trips completed, Incident-free days, Vehicle care score, Punctuality", 21),
        new("ITO-001", "IT Officer", 3, 8_500m, 14_000m, "OPS", "DO-001", false, "Bachelor's Degree", 3, "Microsoft 365, Networking, Endpoint Management, Cybersecurity Basics", "Manages {Company}'s IT infrastructure, Microsoft 365, and endpoint support across offices", "NP-4", "Administer Microsoft 365; manage user accounts; maintain network and endpoints; lead software rollouts; support cybersecurity; train staff on tools", "System uptime, Ticket closure time, Cyber awareness completion, Endpoint compliance", 24),
        new("ICT-001", "ICT Support", 2, 4_500m, 7_000m, "OPS", "ITO-001", false, "HND / Diploma", 1, "Helpdesk, Hardware Support, Active Directory Basics, User Training", "Provides first-line ICT support and resolves user incidents for {Company} staff", "NP-2", "Log and resolve IT tickets; manage user account requests; conduct hardware checks; document resolutions; support new-hire IT setup", "First-call resolution, SLA compliance, User satisfaction, Documentation quality", 21),
        new("OA-001", "Office Administrator", 2, 4_500m, 7_500m, "OPS", "DO-001", false, "Diploma / Bachelor's Degree", 2, "Office Administration, Vendor Coordination, Filing, Travel Support", "Runs day-to-day {Company} office administration, supplies, and visitor coordination", "NP-2", "Manage office supplies; coordinate utilities and minor repairs; support travel and accommodation; maintain filing; greet visitors; manage office calendar", "Supply availability, Visitor satisfaction, Filing accuracy, Travel support quality", 21),
        new("REC-001", "Receptionist", 2, 3_500m, 5_500m, "OPS", "OA-001", false, "Diploma / SHS", 1, "Front-Desk Service, Switchboard, Customer Service, Microsoft 365", "First point of contact for {Company} visitors, callers, and walk-ins at the country office", "NP-2", "Greet visitors; manage switchboard; route correspondence; maintain visitor log; support meeting room bookings; handle courier dispatch", "Visitor experience, Call response rate, Visitor log accuracy, Courier turnaround", 21),

        // ── Safeguarding & Security ──────────────────────────────────────────
        new("HSF-001", "Head of Safeguarding", 4, 16_000m, 26_000m, "SAFEGUARDING", "CD-001", true, "Master's Degree", 8, "Safeguarding, PSEA, Child Protection, Investigations, Training", "Leads {Company}'s safeguarding (child / adult / PSEA) framework, training, and case management", "NP-6", "Own safeguarding policy; lead investigations; deliver training; chair case management committee; report to global safeguarding; engage donors on standards", "Safeguarding training completion, Case closure time, Investigation quality, Donor compliance rating", 27),
        new("SFP-001", "Safeguarding Focal Point", 3, 9_000m, 14_000m, "SAFEGUARDING", "HSF-001", false, "Bachelor's Degree", 3, "Safeguarding, PSEA, Code of Conduct, Confidential Case Handling", "Day-to-day safeguarding contact for {Company} staff and partners across the country programme", "NP-4", "Receive safeguarding concerns; triage and refer cases; deliver staff and partner briefings; monitor field compliance; maintain case register confidentially", "Cases triaged, Briefing reach, Case-handling timeliness, Confidentiality compliance", 24),
        new("HSE-001", "Head of Security", 4, 17_000m, 28_000m, "SAFEGUARDING", "CD-001", true, "Master's Degree / Security Certifications", 8, "Security Risk Management, Crisis Management, Travel Security, INSO / OCHA Liaison", "Leads {Company}'s security risk management, crisis response, and staff safety in Ghana", "NP-6", "Own security risk assessment; lead crisis management team; advise SMT on risk; brief travellers; liaise with INSO, embassies, and security services; run drills", "SRA currency, Crisis drill rating, Incident response time, Traveller briefing rate", 27),
        new("SSO-001", "Security & Safety Officer", 3, 8_500m, 14_000m, "SAFEGUARDING", "HSE-001", false, "Bachelor's Degree", 3, "Security Operations, Travel Tracking, Incident Management, Health & Safety", "Manages day-to-day security and safety operations for {Company}'s staff, premises, and field movements", "NP-4", "Track staff movements; manage hibernation kits; investigate incidents; run safety drills; maintain guard force; advise field teams on travel", "Tracking compliance, Incident closure time, Drill completion, Guard force performance", 24)
    ];

    // ────────────────────────────────────────────────────────────────────────
    // Curated station catalogue (~35 entries) — Country Office, Regional
    // Offices, Field Offices, Sub-Offices, Project Sites, and Training Centres
    // mirroring how tier-1 INGOs operate in Ghana. {Company} placeholder is
    // substituted at row-emit time. Phone digits are placeholders (XXX) per the
    // brief. Email stores ONLY the local-part — the row factory appends the
    // company TLD at runtime.
    // ────────────────────────────────────────────────────────────────────────
    private static readonly StationSpec[] _nonprofitStations =
    [
        // ── Country Office (HQ) ─────────────────────────────────────────────
        new("CO-001", "Country Office - Accra", "Country Office", "Greater Accra", "Accra", "East Legon, Accra", 30, 150, "{Company}'s national headquarters housing the Country Director, SMT, and country-level support functions", "+233 30 XXX XXXX", "countryoffice"),

        // ── Regional Offices (5) ─────────────────────────────────────────────
        new("RO-ASH-001", "Ashanti Regional Office - Kumasi", "Regional Office", "Ashanti", "Kumasi", "Adum, Kumasi", 15, 50, "{Company} regional office coordinating programmes across Ashanti and Bono ecological zone", "+233 32 XXX XXXX", "ashanti.region"),
        new("RO-NOR-001", "Northern Regional Office - Tamale", "Regional Office", "Northern", "Tamale", "Education Ridge, Tamale", 20, 60, "{Company} regional hub for Northern, North East, and Savannah Regions — northern programme delivery", "+233 37 XXX XXXX", "northern.region"),
        new("RO-UPE-001", "Upper East Regional Office - Bolgatanga", "Regional Office", "Upper East", "Bolgatanga", "Zuarungu Road, Bolgatanga", 12, 40, "{Company} regional office covering Upper East Region community programmes", "+233 38 XXX XXXX", "uppereast.region"),
        new("RO-UPW-001", "Upper West Regional Office - Wa", "Regional Office", "Upper West", "Wa", "Wa Central, Wa", 10, 35, "{Company} regional office for Upper West programmes — livelihoods, WASH, and education", "+233 39 XXX XXXX", "upperwest.region"),
        new("RO-VOL-001", "Volta Regional Office - Ho", "Regional Office", "Volta", "Ho", "Ho-Aflao Road, Ho", 12, 40, "{Company} regional office overseeing Volta and Oti Region programmes", "+233 36 XXX XXXX", "volta.region"),
        new("RO-WES-001", "Western Regional Office - Takoradi", "Regional Office", "Western", "Takoradi", "Market Circle, Takoradi", 10, 35, "{Company} regional office serving Western and Western North Regions", "+233 31 XXX XXXX", "western.region"),

        // ── Field Offices (~15) at district capitals ────────────────────────
        new("FO-BAW-001", "Bawku Field Office", "Field Office", "Upper East", "Bawku", "Bawku Central, Bawku", 5, 25, "{Company} field office delivering child protection, WASH, and livelihoods in Bawku and Pusiga districts", "+233 38 XXX XXXX", "bawku"),
        new("FO-NAV-001", "Navrongo Field Office", "Field Office", "Upper East", "Navrongo", "Navrongo Town, Navrongo", 5, 22, "{Company} field office covering Kassena-Nankana programmes — health, education, food security", "+233 38 XXX XXXX", "navrongo"),
        new("FO-LAW-001", "Lawra Field Office", "Field Office", "Upper West", "Lawra", "Lawra Township, Lawra", 5, 20, "{Company} field office covering Lawra, Nandom, and Jirapa districts", "+233 39 XXX XXXX", "lawra"),
        new("FO-TUM-001", "Tumu Field Office", "Field Office", "Upper West", "Tumu", "Tumu Town, Tumu", 5, 20, "{Company} field office for Sissala East and West districts — WASH and livelihoods", "+233 39 XXX XXXX", "tumu"),
        new("FO-SAL-001", "Salaga Field Office", "Field Office", "Savannah", "Salaga", "Salaga Town, Salaga", 5, 22, "{Company} field office for East Gonja Municipal — education and protection programming", "+233 37 XXX XXXX", "salaga"),
        new("FO-DAM-001", "Damongo Field Office", "Field Office", "Savannah", "Damongo", "Damongo Town, Damongo", 5, 20, "{Company} field office covering West Gonja and Mole programme areas", "+233 37 XXX XXXX", "damongo"),
        new("FO-YEN-001", "Yendi Field Office", "Field Office", "Northern", "Yendi", "Yendi Township, Yendi", 5, 24, "{Company} field office covering Yendi Municipal and Mion districts — community resilience programmes", "+233 37 XXX XXXX", "yendi"),
        new("FO-KAR-001", "Karaga Field Office", "Field Office", "Northern", "Karaga", "Karaga Town, Karaga", 5, 18, "{Company} field office serving Karaga and Gushegu districts — health and nutrition focus", "+233 37 XXX XXXX", "karaga"),
        new("FO-NAL-001", "Nalerigu Field Office", "Field Office", "North East", "Nalerigu", "Nalerigu Town, Nalerigu", 5, 18, "{Company} field office for East Mamprusi Municipal — education and child protection", "+233 37 XXX XXXX", "nalerigu"),
        new("FO-KRA-001", "Krachi Field Office", "Field Office", "Oti", "Dambai", "Dambai Town, Dambai", 5, 20, "{Company} field office for Krachi East and West — lakeside livelihoods and child protection", "+233 36 XXX XXXX", "krachi"),
        new("FO-HOH-001", "Hohoe Field Office", "Field Office", "Volta", "Hohoe", "Hohoe Township, Hohoe", 5, 20, "{Company} field office for Hohoe and Afadjato programmes — WASH and education", "+233 36 XXX XXXX", "hohoe"),
        new("FO-JAS-001", "Jasikan Field Office", "Field Office", "Oti", "Jasikan", "Jasikan Township, Jasikan", 4, 18, "{Company} field office for Jasikan and Kadjebi programmes — agriculture and livelihoods", "+233 36 XXX XXXX", "jasikan"),
        new("FO-GOA-001", "Goaso Field Office", "Field Office", "Ahafo", "Goaso", "Goaso Town, Goaso", 5, 20, "{Company} field office for Asunafo and Asutifi programmes — community development", "+233 35 XXX XXXX", "goaso"),
        new("FO-BER-001", "Berekum Field Office", "Field Office", "Bono", "Berekum", "Berekum Township, Berekum", 5, 18, "{Company} field office for Berekum and Dormaa — livelihoods and adolescent programmes", "+233 35 XXX XXXX", "berekum"),
        new("FO-OBU-001", "Obuasi Field Office", "Field Office", "Ashanti", "Obuasi", "Obuasi Township, Obuasi", 5, 22, "{Company} field office for Obuasi and Adansi — child protection and youth programming", "+233 32 XXX XXXX", "obuasi"),

        // ── Sub-Offices / Project Sites (8) ─────────────────────────────────
        new("SO-WAL-001", "Walewale Sub-Office", "Sub-Office", "North East", "Walewale", "Walewale Town, Walewale", 3, 12, "{Company} sub-office co-located with district health office for nutrition project delivery", "+233 24 XXX XXXX", "walewale"),
        new("SO-CHE-001", "Chereponi Sub-Office", "Sub-Office", "North East", "Chereponi", "Chereponi Town, Chereponi", 3, 10, "{Company} sub-office serving Chereponi rural communities — peacebuilding and protection", "+233 24 XXX XXXX", "chereponi"),
        new("PS-PUS-001", "Pusiga Project Site", "Project Site", "Upper East", "Pusiga", "Pusiga Township, Pusiga", 3, 10, "{Company} project site supporting cross-border child protection programming with Burkina Faso", "+233 24 XXX XXXX", "pusiga"),
        new("PS-NAB-001", "Nabdam Project Site", "Project Site", "Upper East", "Nangodi", "Nangodi, Nabdam District", 2, 8, "{Company} project site embedded in Nabdam communities for sanitation and behaviour-change pilots", "+233 24 XXX XXXX", "nabdam"),
        new("PS-SAW-001", "Sawla Project Site", "Project Site", "Savannah", "Sawla", "Sawla Township, Sawla-Tuna-Kalba", 2, 8, "{Company} project site supporting Sawla-Tuna-Kalba district livelihoods and WASH activities", "+233 24 XXX XXXX", "sawla"),
        new("PS-NKW-001", "Nkwanta Project Site", "Project Site", "Oti", "Nkwanta", "Nkwanta Township, Nkwanta", 2, 8, "{Company} project site for Nkwanta North and South — adolescent health and education", "+233 24 XXX XXXX", "nkwanta"),
        new("PS-BIA-001", "Bia Project Site", "Project Site", "Western North", "Essam", "Essam, Bia West", 2, 8, "{Company} project site supporting Bia West cocoa-belt child labour and livelihoods initiatives", "+233 24 XXX XXXX", "bia"),
        new("PS-AKA-001", "Akatsi Project Site", "Project Site", "Volta", "Akatsi", "Akatsi Township, Akatsi South", 2, 8, "{Company} project site for Akatsi North and South — climate resilience and food security", "+233 24 XXX XXXX", "akatsi"),

        // ── Training Centres (3) ─────────────────────────────────────────────
        new("TC-TAM-001", "Northern Training Centre - Tamale", "Training Centre", "Northern", "Tamale", "Education Ridge, Tamale", 5, 30, "{Company} residential training centre hosting staff inductions, partner trainings, and community facilitator certifications for the north", "+233 37 XXX XXXX", "training.northern"),
        new("TC-KSI-001", "Ashanti Training Centre - Kumasi", "Training Centre", "Ashanti", "Kumasi", "KNUST area, Kumasi", 5, 25, "{Company} mid-belt training centre for capacity building, leadership development, and university partnership programmes", "+233 32 XXX XXXX", "training.ashanti"),
        new("TC-ACC-001", "National Learning Centre - Accra", "Training Centre", "Greater Accra", "Accra", "Legon, Accra", 5, 30, "{Company} flagship learning centre hosting national convenings, donor missions, and senior leadership programmes", "+233 30 XXX XXXX", "training.national")
    ];
}
