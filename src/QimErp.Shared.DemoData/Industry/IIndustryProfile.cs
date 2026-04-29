namespace QimErp.Shared.DemoData.Industry;

/// <summary>
/// Industry-specific data shape and rules for demo-tenant seeding.
///
/// Implementations describe an industry (Banking, Software, Construction, ...) by:
///   - sample company names used to pick a realistic tenant name
///   - the org hierarchy template — parameterised by company tier and target headcount
///   - the job-title catalogue with rank levels and salary bands
///   - the station/branch layout (HQ + branches/sites/offices)
///   - employee distribution across rank levels and org units
///   - per-rank salary bands in local currency (GHS for v1)
///
/// One concrete implementation lives per industry; resolution is done via
/// <see cref="IIndustryProfileResolver"/>.
/// </summary>
public interface IIndustryProfile
{
    /// <summary>Stable identifier matching the IAM IndustryType code (e.g. "BANKING").</summary>
    string Code { get; }

    string DisplayName { get; }

    /// <summary>Human-friendly company names this industry has used for seeded tenants.</summary>
    IReadOnlyList<string> SampleCompanyNames { get; }

    /// <summary>
    /// Builds a sized org tree for the given tier and target employee count.
    /// Output node count and depth (1–15 levels) varies by tier.
    /// Pure: deterministic for a given (tier, count, seed).
    /// </summary>
    OrgHierarchySpec BuildOrgHierarchy(CompanyTier tier, int targetEmployees, int randomSeed);

    /// <summary>Job-title catalogue across all rank levels for this industry.</summary>
    IReadOnlyList<JobTitleSpec> JobTitles { get; }

    /// <summary>Station layout (headquarters + branches/sites + satellites).</summary>
    StationLayout BuildStations(CompanyTier tier, int targetEmployees, int randomSeed);

    /// <summary>Distribution of employees across rank levels and (optionally) org units.</summary>
    EmployeeDistributionSpec EmployeeDistribution { get; }

    /// <summary>Per-rank-level salary bands in GHS.</summary>
    SalaryBandSpec SalaryBands { get; }
}

public enum CompanyTier
{
    Startup,
    SME,
    Corporate,
    NonProfit
}
