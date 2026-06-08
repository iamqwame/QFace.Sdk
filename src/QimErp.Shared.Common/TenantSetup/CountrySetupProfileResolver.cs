namespace QimErp.Shared.Common.TenantSetup;

public sealed class CountrySetupProfileResolver(
    IEnumerable<ICountrySetupProfile> profiles,
    ILogger<CountrySetupProfileResolver> logger) : ICountrySetupProfileResolver
{
    private readonly Dictionary<string, ICountrySetupProfile> _byCode =
        profiles.ToDictionary(p => p.CountryCode.ToUpperInvariant(), p => p);

    public ICountrySetupProfile Resolve(string? countryCode)
    {
        var code = (countryCode ?? "GH").Trim().ToUpperInvariant();
        if (_byCode.TryGetValue(code, out var profile))
            return profile;

        logger.LogWarning(
            "[CountrySetup] Country '{Code}' not yet fully supported — using Ghana statutory defaults. " +
            "Verify SSNIT rates, PAYE brackets and public holidays are correct for this tenant.",
            code);

        // Fall back to Ghana as the reference implementation
        return _byCode.TryGetValue("GH", out var ghana)
            ? ghana
            : throw new InvalidOperationException("Ghana (GH) country profile must be registered.");
    }
}
