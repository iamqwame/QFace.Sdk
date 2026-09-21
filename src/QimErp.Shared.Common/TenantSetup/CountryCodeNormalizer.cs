namespace QimErp.Shared.Common.TenantSetup;

public static class CountryCodeNormalizer
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GH"] = "GH",
        ["GHANA"] = "GH",
        ["NG"] = "NG",
        ["NIGERIA"] = "NG",
        ["KE"] = "KE",
        ["KENYA"] = "KE",
        ["ZA"] = "ZA",
        ["SOUTH AFRICA"] = "ZA",
        ["RW"] = "RW",
        ["RWANDA"] = "RW",
        ["TZ"] = "TZ",
        ["TANZANIA"] = "TZ",
        ["UNITED REPUBLIC OF TANZANIA"] = "TZ",
        ["ZM"] = "ZM",
        ["ZAMBIA"] = "ZM",
        ["UG"] = "UG",
        ["UGANDA"] = "UG",
        ["SN"] = "SN",
        ["SENEGAL"] = "SN",
        ["CI"] = "CI",
        ["CÔTE D'IVOIRE"] = "CI",
        ["COTE D'IVOIRE"] = "CI",
        ["IVORY COAST"] = "CI",
        ["CM"] = "CM",
        ["CAMEROON"] = "CM",
        ["CAMEROUN"] = "CM",
        ["TG"] = "TG",
        ["TOGO"] = "TG",
        ["BF"] = "BF",
        ["BURKINA FASO"] = "BF",
        ["GB"] = "GB",
        ["UK"] = "GB",
        ["UNITED KINGDOM"] = "GB",
        ["GREAT BRITAIN"] = "GB",
        ["ENGLAND"] = "GB",
        ["US"] = "US",
        ["USA"] = "US",
        ["UNITED STATES"] = "US",
        ["UNITED STATES OF AMERICA"] = "US"
    };

    private static readonly Dictionary<string, string> DialCodes = new(StringComparer.Ordinal)
    {
        ["+233"] = "GH",
        ["+234"] = "NG",
        ["+254"] = "KE",
        ["+27"] = "ZA",
        ["+250"] = "RW",
        ["+255"] = "TZ",
        ["+260"] = "ZM",
        ["+256"] = "UG",
        ["+221"] = "SN",
        ["+225"] = "CI",
        ["+237"] = "CM",
        ["+228"] = "TG",
        ["+226"] = "BF",
        ["+44"] = "GB",
        ["+1"] = "US"
    };

    /// <summary>Accepts ISO-2, a country name or alias, a dial code ("+233") and IAM's dial-code-plus-ISO shape ("+233-GH"). Unknown input returns null.</summary>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        var dashIndex = trimmed.IndexOf('-');
        if (dashIndex >= 0 && dashIndex + 1 < trimmed.Length)
        {
            var iso = trimmed[(dashIndex + 1)..].Trim();
            if (Aliases.TryGetValue(iso, out var fromIso))
                return fromIso;

            trimmed = trimmed[..dashIndex].Trim();
        }

        if (Aliases.TryGetValue(trimmed, out var code))
            return code;

        return DialCodes.TryGetValue(trimmed, out var fromDial) ? fromDial : null;
    }
}
