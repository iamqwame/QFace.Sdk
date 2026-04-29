using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

/// <summary>
/// Healthcare industry. Lifted from QimErp.IAM.Seeding.Demo's HealthcareIndustryData.
/// Stations are HQ + hospital sites + outpatient clinics.
/// </summary>
public sealed class HealthcareIndustryProfile : IIndustryProfile
{
    public string Code => "HEALTHCARE";
    public string DisplayName => "Healthcare & Medical Services";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "37 Military Hospital", "Korle Bu Teaching Hospital", "Tema General Hospital",
        "Trust Hospital", "mPharma Group", "Ridge Hospital", "Nyaho Medical Centre",
        "Lister Hospital", "University of Ghana Medical Centre",
        "Komfo Anokye Teaching Hospital", "Ghana Health Service",
        "Holy Trinity Medical Centre"
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
            Name: "Main Hospital",
            StationType: "Hospital",
            Region: "Greater Accra",
            City: "Accra",
            Address: "Liberation Road, Accra",
            CapacityMin: 80,
            CapacityMax: tier == CompanyTier.Corporate ? 1500 : 400);

        var hospitalCount = tier switch
        {
            CompanyTier.Startup   => 0,
            CompanyTier.SME       => Math.Max(1, targetEmployees / 250),
            CompanyTier.Corporate => Math.Max(2, Math.Min(15, targetEmployees / 200)),
            CompanyTier.NonProfit => Math.Max(1, targetEmployees / 200),
            _                     => 2
        };

        var hospitals = new List<StationSpec>(hospitalCount);
        for (var i = 0; i < hospitalCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            hospitals.Add(new StationSpec(
                Code: $"HOSP{i + 1:D2}",
                Name: $"{city} Hospital",
                StationType: "Hospital",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 40,
                CapacityMax: 400));
        }

        // Outpatient clinics — smaller satellites with limited beds.
        var clinicCount = hospitalCount * 2;
        var clinics = new List<StationSpec>(clinicCount);
        for (var i = 0; i < clinicCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            clinics.Add(new StationSpec(
                Code: $"CLN{i + 1:D3}",
                Name: $"{city} Clinic",
                StationType: "Clinic",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 5,
                CapacityMax: 30));
        }

        return new StationLayout(hq, hospitals, clinics);
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.005,
            [4] = 0.040,
            [3] = 0.250, // doctors / nurses / pharmacists are bulk of mid-level
            [2] = 0.500,
            [1] = 0.205
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (10_000m, 25_000m),
            [4] = (5_000m,  19_000m),
            [3] = (3_000m,  15_000m),
            [2] = (2_000m,   4_500m),
            [1] = (1_500m,   2_500m)
        });

    // ─────────── baseline org units (lifted from HealthcareIndustryData) ───────────

    private static readonly IReadOnlyList<string> ClinicalJobs = ["CMO", "MED_DIR", "SENIOR_DOCTOR", "SPECIALIST", "CLINICAL_MGR", "DOCTOR", "PA"];
    private static readonly IReadOnlyList<string> NursingJobs  = ["SENIOR_NURSE", "NURSE", "JUNIOR_NURSE", "NURSE_INTERN"];
    private static readonly IReadOnlyList<string> PharmacyJobs = ["PHARMACIST", "PHARM_ASSIST", "PHARM_INTERN"];
    private static readonly IReadOnlyList<string> LabJobs      = ["LAB_TECH", "LAB_ASSIST"];
    private static readonly IReadOnlyList<string> RadJobs      = ["RADIOGRAPHER"];
    private static readonly IReadOnlyList<string> AdminJobs    = ["HOSP_ADMIN", "MED_RECORDS"];
    private static readonly IReadOnlyList<string> ExecJobs     = ["HOSP_ADMIN", "CMO"];
    private static readonly IReadOnlyList<string> EngJobs      = [];
    private static readonly IReadOnlyList<string> FinanceJobs  = [];
    private static readonly IReadOnlyList<string> ItJobs       = [];
    private static readonly IReadOnlyList<string> ProgramsJobs = ["PA", "DOCTOR"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",  "Founder/CEO", null,      OrgUnitKind.Executive, ExecJobs),
        new("CLINICAL", "Clinical",    "FOUNDER", OrgUnitKind.Function,  ClinicalJobs),
        new("ENG",      "Engineering", "FOUNDER", OrgUnitKind.Function,  EngJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]  = 0.20,
        ["CLINICAL"] = 0.50,
        ["ENG"]      = 0.30
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",     "Executive", null,   OrgUnitKind.Executive, ExecJobs),
        new("CLINICAL", "Clinical",  "EXEC", OrgUnitKind.Function,  ClinicalJobs),
        new("NURSING",  "Nursing",   "EXEC", OrgUnitKind.Function,  NursingJobs),
        new("PHARMACY", "Pharmacy",  "EXEC", OrgUnitKind.Function,  PharmacyJobs),
        new("ADMIN",    "Admin & HR","EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.08,
        ["CLINICAL"] = 0.35,
        ["NURSING"]  = 0.30,
        ["PHARMACY"] = 0.12,
        ["ADMIN"]    = 0.15
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",      "Executive",            null,   OrgUnitKind.Executive, ExecJobs),
        new("CLINICAL",  "Clinical",             "EXEC", OrgUnitKind.Function,  ClinicalJobs),
        new("NURSING",   "Nursing",              "EXEC", OrgUnitKind.Function,  NursingJobs),
        new("PHARMACY",  "Pharmacy",             "EXEC", OrgUnitKind.Function,  PharmacyJobs),
        new("LAB",       "Laboratory",           "EXEC", OrgUnitKind.Function,  LabJobs),
        new("RADIOLOGY", "Radiology",            "EXEC", OrgUnitKind.Function,  RadJobs),
        new("ADMIN",     "Admin & HR",           "EXEC", OrgUnitKind.Function,  AdminJobs),
        new("FINANCE",   "Finance & Procurement","EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("IT",        "IT",                   "EXEC", OrgUnitKind.Function,  ItJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]      = 0.05,
        ["CLINICAL"]  = 0.30,
        ["NURSING"]   = 0.25,
        ["PHARMACY"]  = 0.10,
        ["LAB"]       = 0.08,
        ["RADIOLOGY"] = 0.05,
        ["ADMIN"]     = 0.12,
        ["FINANCE"]   = 0.03,
        ["IT"]        = 0.02
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive", null,   OrgUnitKind.Executive, ExecJobs),
        new("CLINICAL", "Clinical",  "EXEC", OrgUnitKind.Function,  ClinicalJobs),
        new("NURSING",  "Nursing",   "EXEC", OrgUnitKind.Function,  NursingJobs),
        new("PROGRAMS", "Programs",  "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("ADMIN",    "Admin & HR","EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.10,
        ["CLINICAL"] = 0.40,
        ["NURSING"]  = 0.30,
        ["PROGRAMS"] = 0.15,
        ["ADMIN"]    = 0.05
    };

    // ─────────── job titles (lifted from HealthcareIndustryData) ───────────

    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        // Executive (5)
        new("CMO",          "Chief Medical Officer",   5, 15_000m, 25_000m, "CLINICAL", null,         true,  "Medical Degree",                 10, "Medical Leadership, Strategic Planning"),
        new("MED_DIR",      "Medical Director",        5, 12_000m, 20_000m, "CLINICAL", "CMO",        true,  "Medical Degree",                 8,  "Clinical Management, Healthcare Administration"),
        new("HOSP_ADMIN",   "Hospital Administrator",  5, 10_000m, 18_000m, "EXEC",     null,         true,  "Master's Degree",                8,  "Healthcare Administration, Operations Management"),
        // Senior (4)
        new("SENIOR_DOCTOR","Senior Doctor",           4, 10_000m, 18_000m, "CLINICAL", "MED_DIR",    true,  "Medical Degree",                 7,  "Clinical Skills, Patient Care, Diagnosis"),
        new("SPECIALIST",   "Specialist",              4, 11_000m, 19_000m, "CLINICAL", "MED_DIR",    true,  "Medical Degree + Specialization",6,  "Specialized Medical Care, Procedures"),
        new("SENIOR_NURSE", "Senior Nurse",            4, 5_000m,  9_000m,  "NURSING",  null,         true,  "Nursing Degree",                 5,  "Patient Care, Nursing Leadership"),
        new("CLINICAL_MGR", "Clinical Manager",        4, 8_000m,  14_000m, "CLINICAL", "MED_DIR",    true,  "Medical Degree",                 6,  "Clinical Management, Team Leadership"),
        // Mid (3)
        new("DOCTOR",       "Doctor",                  3, 8_000m,  15_000m, "CLINICAL", "SENIOR_DOCTOR", false,"Medical Degree",              3,  "Clinical Skills, Patient Care, Diagnosis"),
        new("NURSE",        "Nurse",                   3, 3_000m,  6_000m,  "NURSING",  "SENIOR_NURSE", false,"Nursing Diploma",             2,  "Patient Care, Medical Procedures, Medication Administration"),
        new("PHARMACIST",   "Pharmacist",              3, 5_000m,  9_000m,  "PHARMACY", null,         false, "Pharmacy Degree",                2,  "Medication Dispensing, Drug Interactions, Counseling"),
        new("LAB_TECH",     "Lab Technician",          3, 3_000m,  6_000m,  "LAB",      null,         false, "Laboratory Science Diploma",     2,  "Laboratory Testing, Sample Processing"),
        new("RADIOGRAPHER", "Radiographer",            3, 3_500m,  6_500m,  "RADIOLOGY",null,         false, "Radiography Diploma",            2,  "Medical Imaging, X-Ray, Ultrasound"),
        new("PA",           "Physician Assistant",     3, 6_000m,  10_000m, "CLINICAL", "DOCTOR",     false, "PA Degree",                      3,  "Patient Assessment, Treatment, Procedures"),
        // Junior (2)
        new("JUNIOR_NURSE", "Junior Nurse",            2, 2_000m,  4_000m,  "NURSING",  "NURSE",      false, "Nursing Certificate",            1,  "Basic Patient Care, Vital Signs"),
        new("PHARM_ASSIST", "Pharmacy Assistant",      2, 2_000m,  4_000m,  "PHARMACY", "PHARMACIST", false, "High School",                    1,  "Medication Dispensing Support, Inventory"),
        new("LAB_ASSIST",   "Lab Assistant",           2, 2_000m,  4_000m,  "LAB",      "LAB_TECH",   false, "High School",                    1,  "Sample Collection, Basic Lab Tasks"),
        new("MED_RECORDS",  "Medical Records Officer", 2, 2_500m,  4_500m,  "ADMIN",    null,         false, "High School",                    1,  "Medical Records Management, Filing"),
        // Entry (1)
        new("NURSE_INTERN", "Nursing Intern",          1, 1_500m,  2_500m,  "NURSING",  "JUNIOR_NURSE", false,"Student",                       0,  "Learning, Supervised Patient Care"),
        new("PHARM_INTERN", "Pharmacy Intern",         1, 1_500m,  2_500m,  "PHARMACY", "PHARM_ASSIST", false,"Student",                       0,  "Learning, Supervised Dispensing")
    ];
}
