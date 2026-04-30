namespace QimErp.Shared.DemoData.Ghana;

// Population shares per the 2021 Ghana census; weights sum to 1.0.
public static class GhanaEthnicity
{
    public static readonly IReadOnlyList<(string Group, double Weight)> Distribution =
    [
        ("Akan",          0.4750),
        ("Mole-Dagbon",   0.1690),
        ("Ewe",           0.1390),
        ("Ga-Adangbe",    0.0750),
        ("Gurma",         0.0530),
        ("Guan",          0.0370),
        ("Grusi",         0.0260),
        ("Mande",         0.0110),
        ("Hausa",         0.0080),
        ("Other",         0.0070)
    ];

    public static readonly IReadOnlyList<string> Groups =
        Distribution.Select(d => d.Group).ToList();
}
