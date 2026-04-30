namespace QimErp.Shared.DemoData.Industry;

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
        => _profiles.TryGetValue(industryCode, out profile);

    public IReadOnlyList<IIndustryProfile> All => _profiles.Values.ToList();
}
