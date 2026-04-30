namespace QimErp.Shared.DemoData.Industry;

public sealed record OrgHierarchySpec(IReadOnlyList<OrgUnitNode> Nodes);

public sealed record OrgUnitNode(
    string Code,
    string Name,
    string? ParentCode,
    int Level,
    OrgUnitKind Kind,
    int TargetHeadcount,
    IReadOnlyList<string> EligibleJobTitleCodes);

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
    string Skills);

public sealed record StationLayout(
    StationSpec Headquarters,
    IReadOnlyList<StationSpec> Branches,
    IReadOnlyList<StationSpec> Satellites);

public sealed record StationSpec(
    string Code,
    string Name,
    string StationType,
    string Region,
    string City,
    string? Address,
    int CapacityMin,
    int CapacityMax);

public sealed record EmployeeDistributionSpec(
    IReadOnlyDictionary<int, double> ByRankLevel,
    IReadOnlyDictionary<string, double>? ByOrgUnitCode);

public sealed record SalaryBandSpec(
    IReadOnlyDictionary<int, (decimal Min, decimal Max)> ByRankLevel);
