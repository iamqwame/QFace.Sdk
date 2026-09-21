namespace QimErp.Shared.Common.TenantSetup;

public static class CountryCodeNormalizer
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GH"] = "GH",
        ["Ghana"] = "GH",
        ["NG"] = "NG",
        ["Nigeria"] = "NG",
        ["KE"] = "KE",
        ["Kenya"] = "KE",
        ["TZ"] = "TZ",
        ["Tanzania"] = "TZ"
    };

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Aliases.TryGetValue(value.Trim(), out var code) ? code : null;
    }
}
