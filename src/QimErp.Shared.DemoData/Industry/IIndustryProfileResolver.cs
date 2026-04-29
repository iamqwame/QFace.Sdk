namespace QimErp.Shared.DemoData.Industry;

/// <summary>
/// Resolves an <see cref="IIndustryProfile"/> by code (case-insensitive). Backed by
/// <c>IndustryRegistry</c> which holds the static catalogue of all known profiles.
/// </summary>
public interface IIndustryProfileResolver
{
    IIndustryProfile Resolve(string industryCode);
    bool TryResolve(string industryCode, out IIndustryProfile? profile);
    IReadOnlyList<IIndustryProfile> All { get; }
}
