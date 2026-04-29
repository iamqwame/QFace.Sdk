namespace QimErp.Shared.DemoData.Ghana;

/// <summary>
/// Religious affiliation in Ghana with denomination breakdowns (2021 census basis).
/// Used for the <c>Religion</c> and <c>Denomination</c> fields on Employee.
/// </summary>
public static class GhanaReligion
{
    public static readonly IReadOnlyList<(string Religion, double Weight)> Distribution =
    [
        ("Christian",   0.7110),
        ("Islam",       0.1980),
        ("Traditional", 0.0310),
        ("None",        0.0470),
        ("Other",       0.0130)
    ];

    /// <summary>Christian denominations weighted within the Christian segment.</summary>
    public static readonly IReadOnlyList<(string Denomination, double Weight)> ChristianDenominations =
    [
        ("Pentecostal/Charismatic", 0.4170),
        ("Protestant",              0.1810),
        ("Catholic",                0.1010),
        ("Other Christian",         0.1120)
    ];

    /// <summary>Muslim sects within the Islamic segment.</summary>
    public static readonly IReadOnlyList<(string Denomination, double Weight)> MuslimDenominations =
    [
        ("Sunni",   0.8500),
        ("Ahmadi",  0.0900),
        ("Tijaniyya", 0.0400),
        ("Other Islamic", 0.0200)
    ];

    public static readonly IReadOnlyList<string> AllReligions =
        Distribution.Select(d => d.Religion).ToList();
}
