namespace QimErp.Shared.DemoData.Ghana;

/// <summary>
/// Ethnic groups of Ghana with population shares (2021 census basis). Used for the
/// <c>EthnicGroup</c> field on the Employee entity.
/// </summary>
public static class GhanaEthnicity
{
    /// <summary>Ethnic group → approximate share of population (sums to 1.0).</summary>
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
