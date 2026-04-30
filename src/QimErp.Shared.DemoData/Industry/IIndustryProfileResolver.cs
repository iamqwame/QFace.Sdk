namespace QimErp.Shared.DemoData.Industry;

public interface IIndustryProfileResolver
{
    IIndustryProfile Resolve(string industryCode);
    bool TryResolve(string industryCode, out IIndustryProfile? profile);
    IReadOnlyList<IIndustryProfile> All { get; }
}
