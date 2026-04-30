namespace QimErp.Shared.DemoData.Ghana;

public static class GhanaTelecom
{
    public enum Carrier { MTN, Telecel, AT }

    public static readonly IReadOnlyList<string> MtnPrefixes  = ["24", "25", "53", "54", "55", "59"];
    public static readonly IReadOnlyList<string> TelecelPrefixes = ["20", "50"];
    public static readonly IReadOnlyList<string> AtPrefixes = ["26", "27", "56", "57"];

    public static readonly IReadOnlyList<string> AllPrefixes =
        MtnPrefixes.Concat(TelecelPrefixes).Concat(AtPrefixes).ToList();

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
