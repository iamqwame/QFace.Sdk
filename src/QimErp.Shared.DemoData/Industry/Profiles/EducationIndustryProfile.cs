using QimErp.Shared.DemoData.Ghana;

namespace QimErp.Shared.DemoData.Industry.Profiles;

public sealed class EducationIndustryProfile : IIndustryProfile
{
    public string Code => "EDUCATION";
    public string DisplayName => "Education & Training";

    public IReadOnlyList<string> SampleCompanyNames =>
    [
        "University of Ghana", "KNUST", "University of Cape Coast", "GIMPA",
        "Ashesi University", "Lancaster University Ghana", "Central University",
        "Webster University Ghana", "University of Professional Studies",
        "Ghana Institute of Languages", "African University College of Communications",
        "Wisconsin International University"
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
            Name: "Main Campus",
            StationType: "Campus",
            Region: "Greater Accra",
            City: "Accra",
            Address: "Legon, Accra",
            CapacityMin: 60,
            CapacityMax: tier == CompanyTier.Corporate ? 1200 : 350);

        var campusCount = tier switch
        {
            CompanyTier.Startup   => 0,
            CompanyTier.SME       => Math.Max(1, targetEmployees / 250),
            CompanyTier.Corporate => Math.Max(2, Math.Min(8, targetEmployees / 200)),
            CompanyTier.NonProfit => 1,
            _                     => 2
        };

        var campuses = new List<StationSpec>(campusCount);
        for (var i = 0; i < campusCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            campuses.Add(new StationSpec(
                Code: $"CAMP{i + 1:D2}",
                Name: $"{city} Campus",
                StationType: "Campus",
                Region: region,
                City: city,
                Address: $"University Road, {city}",
                CapacityMin: 25,
                CapacityMax: 250));
        }

        var centreCount = campusCount * 2;
        var centres = new List<StationSpec>(centreCount);
        for (var i = 0; i < centreCount; i++)
        {
            var region = GhanaGeography.Regions[rng.Next(GhanaGeography.Regions.Count)];
            var cities = GhanaGeography.CitiesByRegion[region];
            var city = cities[rng.Next(cities.Count)];
            centres.Add(new StationSpec(
                Code: $"LC{i + 1:D3}",
                Name: $"{city} Learning Centre",
                StationType: "Learning Centre",
                Region: region,
                City: city,
                Address: $"{GhanaGeography.Streets[rng.Next(GhanaGeography.Streets.Count)]}, {city}",
                CapacityMin: 3,
                CapacityMax: 20));
        }

        return new StationLayout(hq, campuses, centres);
    }

    public EmployeeDistributionSpec EmployeeDistribution => new(
        ByRankLevel: new Dictionary<int, double>
        {
            [5] = 0.010,
            [4] = 0.060,
            [3] = 0.350,
            [2] = 0.430,
            [1] = 0.150
        },
        ByOrgUnitCode: null);

    public SalaryBandSpec SalaryBands => new(
        ByRankLevel: new Dictionary<int, (decimal Min, decimal Max)>
        {
            [5] = (6_500m, 14_000m),
            [4] = (4_000m,  9_500m),
            [3] = (3_000m,  9_000m),
            [2] = (2_000m,  4_500m),
            [1] = (1_500m,  3_000m)
        });

    private static readonly IReadOnlyList<string> AcademicJobs   = ["ACAD_DIR", "SENIOR_LECTURER", "HOD", "LECTURER", "TEACHER", "ASSIST_TEACHER", "TA"];
    private static readonly IReadOnlyList<string> AdminJobs      = ["ADMIN", "ADMIN_OFFICER", "ADMIN_ASSIST", "INTERN"];
    private static readonly IReadOnlyList<string> StudentJobs    = ["STUDENT_MGR", "STUDENT_OFFICER", "STUDENT_ASSIST"];
    private static readonly IReadOnlyList<string> SupportJobs    = ["LIBRARIAN", "LIBRARY_ASSIST"];
    private static readonly IReadOnlyList<string> ItJobs         = ["IT_OFFICER"];
    private static readonly IReadOnlyList<string> FinanceJobs    = [];
    private static readonly IReadOnlyList<string> HrJobs         = [];
    private static readonly IReadOnlyList<string> ExecJobs       = ["PRINCIPAL", "VP"];
    private static readonly IReadOnlyList<string> ProgramsJobs   = ["LECTURER", "TEACHER"];

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> StartupUnits =
    [
        new("FOUNDER",  "Founder/Director", null,      OrgUnitKind.Executive, ExecJobs),
        new("ACADEMIC", "Academic",         "FOUNDER", OrgUnitKind.Function,  AcademicJobs),
        new("ADMIN",    "Administration",   "FOUNDER", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> StartupDistribution = new Dictionary<string, double>
    {
        ["FOUNDER"]  = 0.15,
        ["ACADEMIC"] = 0.60,
        ["ADMIN"]    = 0.25
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> SmeUnits =
    [
        new("EXEC",     "Executive",       null,   OrgUnitKind.Executive, ExecJobs),
        new("ACADEMIC", "Academic",        "EXEC", OrgUnitKind.Function,  AcademicJobs),
        new("ADMIN",    "Administration",  "EXEC", OrgUnitKind.Function,  AdminJobs),
        new("SUPPORT",  "Support Services","EXEC", OrgUnitKind.Function,  SupportJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> SmeDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.08,
        ["ACADEMIC"] = 0.55,
        ["ADMIN"]    = 0.22,
        ["SUPPORT"]  = 0.15
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> CorporateUnits =
    [
        new("EXEC",             "Executive",        null,   OrgUnitKind.Executive, ExecJobs),
        new("ACADEMIC",         "Academic",         "EXEC", OrgUnitKind.Function,  AcademicJobs),
        new("STUDENT_SERVICES", "Student Services", "EXEC", OrgUnitKind.Function,  StudentJobs),
        new("ADMIN",            "Administration",   "EXEC", OrgUnitKind.Function,  AdminJobs),
        new("SUPPORT",          "Support Services", "EXEC", OrgUnitKind.Function,  SupportJobs),
        new("IT",               "IT",               "EXEC", OrgUnitKind.Function,  ItJobs),
        new("FINANCE",          "Finance",          "EXEC", OrgUnitKind.Function,  FinanceJobs),
        new("HR",               "HR",               "EXEC", OrgUnitKind.Function,  HrJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> CorporateDistribution = new Dictionary<string, double>
    {
        ["EXEC"]             = 0.05,
        ["ACADEMIC"]         = 0.50,
        ["STUDENT_SERVICES"] = 0.15,
        ["ADMIN"]            = 0.12,
        ["SUPPORT"]          = 0.10,
        ["IT"]               = 0.05,
        ["FINANCE"]          = 0.02,
        ["HR"]               = 0.01
    };

    private static readonly IReadOnlyList<OrgHierarchyBuilder.BaselineUnit> NonProfitUnits =
    [
        new("EXEC",     "Executive",       null,   OrgUnitKind.Executive, ExecJobs),
        new("ACADEMIC", "Academic",        "EXEC", OrgUnitKind.Function,  AcademicJobs),
        new("PROGRAMS", "Programs",        "EXEC", OrgUnitKind.Function,  ProgramsJobs),
        new("ADMIN",    "Administration",  "EXEC", OrgUnitKind.Function,  AdminJobs)
    ];
    private static readonly IReadOnlyDictionary<string, double> NonProfitDistribution = new Dictionary<string, double>
    {
        ["EXEC"]     = 0.10,
        ["ACADEMIC"] = 0.50,
        ["PROGRAMS"] = 0.25,
        ["ADMIN"]    = 0.15
    };

    // "ADMIN" is shared by an OrgUnit and a JobTitle code — namespaces don't collide at runtime.
    private static readonly IReadOnlyList<JobTitleSpec> _jobTitles =
    [
        new("PRINCIPAL",       "Principal/Head",          5, 8_000m, 14_000m, "EXEC",             null,        true,  "Master's Degree",   8, "Educational Leadership, Administration"),
        new("VP",              "Vice Principal",          5, 6_500m, 11_000m, "EXEC",             "PRINCIPAL", true,  "Master's Degree",   6, "Academic Administration, Curriculum"),
        new("ACAD_DIR",        "Academic Director",       5, 7_000m, 12_000m, "ACADEMIC",         "PRINCIPAL", true,  "Master's Degree",   7, "Academic Leadership, Curriculum Development"),
        new("SENIOR_LECTURER", "Senior Lecturer",         4, 5_000m,  9_000m, "ACADEMIC",         "ACAD_DIR",  true,  "Master's Degree",   6, "Teaching, Research, Course Development"),
        new("HOD",             "Head of Department",      4, 5_500m,  9_500m, "ACADEMIC",         "ACAD_DIR",  true,  "Master's Degree",   5, "Department Management, Teaching"),
        new("ADMIN",           "Administrator",           4, 4_000m,  7_000m, "ADMIN",            null,        true,  "Bachelor's Degree", 5, "Administration, Office Management"),
        new("STUDENT_MGR",     "Student Services Manager",4, 4_500m,  8_000m, "STUDENT_SERVICES", null,        true,  "Bachelor's Degree", 5, "Student Support, Counseling"),
        new("LECTURER",        "Lecturer",                3, 4_000m,  7_000m, "ACADEMIC",         "SENIOR_LECTURER", false, "Bachelor's Degree", 3, "Teaching, Course Delivery"),
        new("TEACHER",         "Teacher",                 3, 3_000m,  6_000m, "ACADEMIC",         "HOD",       false, "Bachelor's Degree", 2, "Teaching, Lesson Planning"),
        new("ADMIN_OFFICER",   "Administrative Officer",  3, 3_000m,  5_500m, "ADMIN",            "ADMIN",     false, "Diploma",           2, "Administrative Support, Record Keeping"),
        new("STUDENT_OFFICER", "Student Services Officer",3, 3_000m,  5_500m, "STUDENT_SERVICES", "STUDENT_MGR", false, "Bachelor's Degree", 2, "Student Support, Enrollment"),
        new("LIBRARIAN",       "Librarian",               3, 3_500m,  6_000m, "SUPPORT",          null,        false, "Bachelor's Degree", 2, "Library Management, Research Support"),
        new("IT_OFFICER",      "IT Officer",              3, 5_000m,  9_000m, "IT",               null,        false, "Bachelor's Degree", 3, "IT Support, Systems Administration"),
        new("ASSIST_TEACHER",  "Assistant Teacher",       2, 2_500m,  4_500m, "ACADEMIC",         "TEACHER",   false, "Diploma",           1, "Teaching Support, Classroom Assistance"),
        new("ADMIN_ASSIST",    "Administrative Assistant",2, 2_000m,  3_500m, "ADMIN",            "ADMIN_OFFICER", false, "High School",   1, "Administrative Support, Filing"),
        new("STUDENT_ASSIST",  "Student Services Assistant",2,2_500m, 4_000m, "STUDENT_SERVICES", "STUDENT_OFFICER",false,"High School",  1, "Student Support, Reception"),
        new("LIBRARY_ASSIST",  "Library Assistant",       2, 2_000m,  3_500m, "SUPPORT",          "LIBRARIAN", false, "High School",       1, "Library Support, Shelving"),
        new("TA",              "Teaching Assistant",      1, 1_500m,  3_000m, "ACADEMIC",         "ASSIST_TEACHER", false, "Student",      0, "Learning, Teaching Support"),
        new("INTERN",          "Intern",                  1, 1_500m,  2_500m, "ADMIN",            "ADMIN_ASSIST",   false, "Student",      0, "Learning, Administrative Support"),
        // ── Expansion ────────────────────────────────────────────────────────
        new("VICE_HEAD",       "Deputy Head Teacher",     5, 11_000m, 18_000m, "ACADEMIC",         null,             true,  "Master's in Education",  10, "Curriculum Oversight, Teacher Mentoring"),
        new("FACULTY_DEAN",    "Faculty Dean",            5, 12_000m, 20_000m, "ACADEMIC",         null,             true,  "PhD",                    12, "Faculty Strategy, Research Leadership"),
        new("REGISTRAR",       "Registrar",               5, 9_000m,  15_000m, "STUDENT_SERVICES", null,             true,  "Master's Degree",        10, "Records, Admissions, Examinations"),
        new("HEAD_FINANCE",    "Bursar",                  5, 9_000m,  15_000m, "FINANCE",          null,             true,  "Master's Degree",        10, "School Finance, Fees Collection"),
        new("HEAD_LIBRARY",    "Chief Librarian",         4, 5_500m,  9_500m,  "ACADEMIC",         null,             true,  "Library Sciences Master's",6,"Library Operations, Cataloguing"),
        new("DEPT_HEAD",       "Head of Department",      4, 7_000m,  12_000m, "ACADEMIC",         "FACULTY_DEAN",   true,  "Master's Degree",        7,  "Departmental Strategy, Staff Management"),
        new("EXAMS_OFFICER",   "Examinations Officer",    4, 5_000m,  8_500m,  "ACADEMIC",         "REGISTRAR",      true,  "Bachelor's Degree",      5,  "Exam Logistics, Invigilation"),
        new("ADMISSIONS_MGR",  "Admissions Manager",      4, 5_500m,  9_000m,  "STUDENT_SERVICES", "REGISTRAR",      true,  "Bachelor's Degree",      5,  "Admissions, Outreach"),
        new("LECTURER",        "Lecturer",                3, 5_500m,  9_500m,  "ACADEMIC",         "DEPT_HEAD",      false, "Master's Degree",        3,  "Teaching, Lecture Delivery"),
        new("RESEARCHER",      "Research Fellow",         3, 5_500m,  9_500m,  "ACADEMIC",         "FACULTY_DEAN",   false, "PhD Candidate",          3,  "Research Projects, Publications"),
        new("LIBRARIAN",       "Librarian",               3, 4_000m,  7_000m,  "ACADEMIC",         "HEAD_LIBRARY",   false, "Bachelor's Degree",      2,  "Cataloguing, Reader Services"),
        new("LAB_TECH_EDU",    "Science Lab Technician",  3, 3_500m,  6_500m,  "ACADEMIC",         "DEPT_HEAD",      false, "Diploma",                2,  "Practical Sessions, Equipment Care"),
        new("SCHOOL_NURSE",    "School Nurse",            3, 3_500m,  6_500m,  "STUDENT_SERVICES", null,             false, "Nursing Diploma",        2,  "Student Health, First Aid"),
        new("COUNSELLOR",      "Student Counsellor",      3, 4_000m,  7_500m,  "STUDENT_SERVICES", null,             false, "Psychology Bachelor's",  2,  "Guidance, Counselling, Wellbeing"),
        new("IT_OFFICER_EDU",  "IT Officer",              3, 4_500m,  8_000m,  "IT",               null,             false, "Bachelor's Degree",      3,  "School Systems, Computer Lab Support"),
        new("CATERER",         "Catering Officer",        2, 2_500m,  4_500m,  "SUPPORT",          null,             false, "Diploma",                1,  "Meal Planning, Kitchen Operations"),
        new("DRIVER",          "School Driver",           2, 2_000m,  3_500m,  "SUPPORT",          null,             false, "High School + Licence",  1,  "Student Transport, Vehicle Maintenance"),
        new("SECURITY",        "Security Officer",        2, 2_000m,  3_500m,  "SUPPORT",          null,             false, "High School",            1,  "Campus Security, Visitor Control"),
        new("CLEANER",         "Cleaning Staff",          1, 1_500m,  2_500m,  "SUPPORT",          null,             false, "Basic Education",        0,  "Cleaning, Sanitation"),
        new("TEACHING_INTERN", "Teaching Practice Intern",1, 1_500m,  2_500m,  "ACADEMIC",         "LECTURER",       false, "Student",                0,  "Supervised Teaching")
    ];
}
