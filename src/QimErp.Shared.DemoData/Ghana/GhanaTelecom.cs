namespace QimErp.Shared.DemoData.Ghana;

/// <summary>
/// Ghanaian mobile carrier prefix bands (post +233-0). The existing
/// RandomDataGenerator picked random "20–60" which yields invalid prefixes;
/// these are the real allocations as of 2024.
/// </summary>
public static class GhanaTelecom
{
    public enum Carrier { MTN, Telecel, AT }

    /// <summary>MTN Ghana (largest network).</summary>
    public static readonly IReadOnlyList<string> MtnPrefixes  = ["24", "25", "53", "54", "55", "59"];

    /// <summary>Telecel (formerly Vodafone).</summary>
    public static readonly IReadOnlyList<string> TelecelPrefixes = ["20", "50"];

    /// <summary>AT (Airtel-Tigo merger).</summary>
    public static readonly IReadOnlyList<string> AtPrefixes = ["26", "27", "56", "57"];

    /// <summary>All valid two-digit prefixes regardless of carrier.</summary>
    public static readonly IReadOnlyList<string> AllPrefixes =
        MtnPrefixes.Concat(TelecelPrefixes).Concat(AtPrefixes).ToList();

    /// <summary>Approximate market share — used to weight random carrier selection (MTN ~60%, Telecel ~20%, AT ~20%).</summary>
    public static readonly IReadOnlyDictionary<Carrier, double> MarketShare =
        new Dictionary<Carrier, double>
        {
            [Carrier.MTN]     = 0.60,
            [Carrier.Telecel] = 0.20,
            [Carrier.AT]      = 0.20
        };

    public static IReadOnlyList<string> PrefixesFor(Carrier carrier) => carrier switch
    {
        Carrier.MTN => MtnPrefixes,
        Carrier.Telecel => TelecelPrefixes,
        Carrier.AT => AtPrefixes,
        _ => AllPrefixes
    };
}
