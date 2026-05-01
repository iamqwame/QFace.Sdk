namespace QimErp.Shared.DemoData.Industry;

public sealed record OrgHierarchySpec(IReadOnlyList<OrgUnitNode> Nodes);

/// <summary>
/// One node in an industry profile's organisational hierarchy. Carries enough
/// metadata to populate every column an HR admin would see in the UI without
/// the demo seed leaving "—" placeholders. Description / Budget / CostCenter
/// / Purpose / Phone / Email default to empty/zero so the OrgHierarchyBuilder's
/// in-memory subdivisions (which don't have curated copy) still construct.
/// Curated baseline nodes from the industry profiles SHOULD set them; the
/// row factory falls back to procedural defaults when they're empty so the
/// final import row never carries blank cells.
/// </summary>
public sealed record OrgUnitNode(
    string Code,
    string Name,
    string? ParentCode,
    int Level,
    OrgUnitKind Kind,
    int TargetHeadcount,
    IReadOnlyList<string> EligibleJobTitleCodes,
    string Description = "",
    decimal BudgetMin = 0,
    decimal BudgetMax = 0,
    string CostCenter = "",
    string Purpose = "",
    string Phone = "",
    string Email = "");

public enum OrgUnitKind
{
    Executive,
    Function,
    Region,
    Area,
    Branch,
    Site,
    Team
}

/// <summary>
/// One job title in an industry profile's catalogue. Carries every column the
/// EmployeeImportProcessorService surfaces to the UI: salary band + level +
/// responsibilities + KPIs + paygrade + min education / experience + leave
/// entitlement. New fields default to empty so existing profiles compile while
/// they're being migrated; the row factory falls back to procedurally-derived
/// content when they're empty so no UI cell ever shows "—".
/// </summary>
public sealed record JobTitleSpec(
    string Code,
    string Name,
    int RankLevel,
    decimal MinSalaryGhs,
    decimal MaxSalaryGhs,
    string? OrgUnitCode,
    string? ReportsToTitleCode,
    bool IsManagerial,
    string MinEducation,
    int MinExperienceYears,
    string Skills,
    string Description = "",
    string PayGrade = "",
    string Responsibilities = "",
    string KeyPerformanceIndicators = "",
    int AnnualLeaveEntitlementDays = 0);

public sealed record StationLayout(
    StationSpec Headquarters,
    IReadOnlyList<StationSpec> Branches,
    IReadOnlyList<StationSpec> Satellites);

/// <summary>
/// One station / office / branch / satellite location. Carries enough metadata
/// that the UI never shows "—" for City / Country / Type / Address. Description
/// / Phone / Email default to empty so existing profiles compile while they're
/// being migrated; the row factory derives them procedurally when blank.
/// </summary>
public sealed record StationSpec(
    string Code,
    string Name,
    string StationType,
    string Region,
    string City,
    string? Address,
    int CapacityMin,
    int CapacityMax,
    string Description = "",
    string Phone = "",
    string Email = "");

public sealed record EmployeeDistributionSpec(
    IReadOnlyDictionary<int, double> ByRankLevel,
    IReadOnlyDictionary<string, double>? ByOrgUnitCode);

public sealed record SalaryBandSpec(
    IReadOnlyDictionary<int, (decimal Min, decimal Max)> ByRankLevel);
