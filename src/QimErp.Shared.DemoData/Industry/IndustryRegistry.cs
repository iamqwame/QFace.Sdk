namespace QimErp.Shared.DemoData.Industry;

/// <summary>
/// Singleton catalogue of all known industry profiles. Profiles register themselves
/// here so resolution is keyed off the IAM <c>IndustryType.Code</c>.
///
/// Concrete profiles (BankingIndustryProfile, SoftwareIndustryProfile, ...) are added
/// in Phase 2 of the demo-seeding rollout. This empty registry is shipped as the
/// scaffold so dependent services can take the dependency now.
/// </summary>
public sealed class IndustryRegistry : IIndustryProfileResolver
{
    private readonly Dictionary<string, IIndustryProfile> _profiles;

    public IndustryRegistry() : this(Enumerable.Empty<IIndustryProfile>()) { }

    public IndustryRegistry(IEnumerable<IIndustryProfile> profiles)
    {
        _profiles = profiles.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);
    }

    public IIndustryProfile Resolve(string industryCode)
    {
        if (_profiles.TryGetValue(industryCode, out var profile))
            return profile;
        throw new KeyNotFoundException(
            $"No industry profile registered for code '{industryCode}'. " +
            $"Known codes: {string.Join(", ", _profiles.Keys)}.");
    }

    public bool TryResolve(string industryCode, out IIndustryProfile? profile)
    {
        return _profiles.TryGetValue(industryCode, out profile);
    }

    public IReadOnlyList<IIndustryProfile> All => _profiles.Values.ToList();
}
