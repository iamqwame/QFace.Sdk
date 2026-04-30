namespace QimErp.Shared.DemoData.Industry;

public interface IIndustryProfile
{
    string Code { get; }
    string DisplayName { get; }
    IReadOnlyList<string> SampleCompanyNames { get; }
    OrgHierarchySpec BuildOrgHierarchy(CompanyTier tier, int targetEmployees, int randomSeed);
    IReadOnlyList<JobTitleSpec> JobTitles { get; }
    StationLayout BuildStations(CompanyTier tier, int targetEmployees, int randomSeed);
    EmployeeDistributionSpec EmployeeDistribution { get; }
    SalaryBandSpec SalaryBands { get; }
}

public enum CompanyTier
{
    Startup,
    SME,
    Corporate,
    NonProfit
}
