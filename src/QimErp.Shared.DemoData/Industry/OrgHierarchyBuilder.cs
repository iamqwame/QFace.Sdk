namespace QimErp.Shared.DemoData.Industry;

/// <summary>
/// Pure function: takes an industry's baseline (L1-L4) org units and an employee
/// distribution, and grows a sized tree to accommodate the target headcount, going
/// up to 15 levels deep for large corporates by adding regional / area / branch /
/// team sub-nodes under heavy departments.
///
/// Deterministic for a given (industry, tier, count, seed): same inputs → byte-identical tree.
/// </summary>
public static class OrgHierarchyBuilder
{
    /// <summary>Maximum depth the builder will produce (matches CalBank-style 15-level org charts).</summary>
    public const int MaxDepth = 15;

    /// <summary>
    /// A leaf-team node tries to land between this many employees — sets the span at which
    /// we stop subdividing. 12 ≈ a comfortable manager span (1 supervisor + ~10 reports).
    /// </summary>
    private const int TargetLeafSize = 12;

    /// <summary>Subdivide any node whose share of the headcount exceeds this. Below it, treat as a leaf.</summary>
    private const int SubdivideThreshold = 30;

    /// <summary>Span-of-control range for sub-node fan-out: how many children we add per subdivision.</summary>
    private const int MinFanOut = 3;
    private const int MaxFanOut = 8;

    public sealed record BaselineUnit(
        string Code,
        string Name,
        string? ParentCode,
        OrgUnitKind Kind,
        IReadOnlyList<string> EligibleJobTitleCodes);

    /// <summary>
    /// Builds the tree.
    /// </summary>
    /// <param name="baselineUnits">L1-L4 anchor units from the industry profile (roots + departments).</param>
    /// <param name="distribution">Employee share per baseline unit code (must sum to ~1.0 across roots' children).</param>
    /// <param name="targetEmployees">Total demo headcount.</param>
    /// <param name="seed">RNG seed for deterministic subdivision naming.</param>
    public static OrgHierarchySpec Build(
        IReadOnlyList<BaselineUnit> baselineUnits,
        IReadOnlyDictionary<string, double> distribution,
        int targetEmployees,
        int seed)
    {
        if (baselineUnits.Count == 0)
            throw new ArgumentException("At least one baseline unit is required.", nameof(baselineUnits));
        if (targetEmployees <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetEmployees));

        var rng = new Random(seed);
        var nodes = new List<OrgUnitNode>();
        var levelByCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Step 1 — emit the baseline units as L1..LN by walking from roots.
        foreach (var unit in baselineUnits)
        {
            var level = unit.ParentCode is null ? 1 : levelByCode[unit.ParentCode] + 1;
            levelByCode[unit.Code] = level;

            var share = distribution.TryGetValue(unit.Code, out var s) ? s : 0.0;
            var headcount = (int)Math.Round(share * targetEmployees);

            nodes.Add(new OrgUnitNode(
                Code: unit.Code,
                Name: unit.Name,
                ParentCode: unit.ParentCode,
                Level: level,
                Kind: unit.Kind,
                TargetHeadcount: headcount,
                EligibleJobTitleCodes: unit.EligibleJobTitleCodes));
        }

        // Step 2 — subdivide any node above the threshold by adding regional/area/branch/team children.
        // We iterate until no node exceeds the threshold or we hit MaxDepth.
        for (var pass = 0; pass < MaxDepth; pass++)
        {
            var candidates = nodes
                .Where(n => n.TargetHeadcount > SubdivideThreshold && n.Level < MaxDepth)
                .ToList();
            if (candidates.Count == 0) break;

            foreach (var parent in candidates)
            {
                var children = SubdivideNode(parent, rng).ToList();
                if (children.Count == 0) continue;

                // Replace parent's headcount with the sum-of-children expectation; the parent
                // becomes a "container" node with 0 direct headcount.
                var idx = nodes.FindIndex(n => n.Code == parent.Code);
                nodes[idx] = parent with { TargetHeadcount = 0 };
                nodes.AddRange(children);
                foreach (var child in children) levelByCode[child.Code] = child.Level;
            }
        }

        return new OrgHierarchySpec(nodes);
    }

    private static IEnumerable<OrgUnitNode> SubdivideNode(OrgUnitNode parent, Random rng)
    {
        var nextKind = NextKind(parent.Kind);
        if (nextKind is null) yield break;

        // Decide fan-out so the resulting children are around the target leaf size.
        var idealFanOut = Math.Max(MinFanOut, Math.Min(MaxFanOut, parent.TargetHeadcount / TargetLeafSize));
        var actualFanOut = Math.Clamp(idealFanOut + rng.Next(-1, 2), MinFanOut, MaxFanOut);
        var perChild = parent.TargetHeadcount / actualFanOut;
        var remainder = parent.TargetHeadcount - perChild * actualFanOut;

        for (var i = 0; i < actualFanOut; i++)
        {
            var head = perChild + (i < remainder ? 1 : 0);
            var (suffix, label) = ChildLabel(parent.Kind, nextKind.Value, i);
            yield return new OrgUnitNode(
                Code: $"{parent.Code}.{suffix}",
                Name: $"{parent.Name} - {label}",
                ParentCode: parent.Code,
                Level: parent.Level + 1,
                Kind: nextKind.Value,
                TargetHeadcount: head,
                EligibleJobTitleCodes: parent.EligibleJobTitleCodes);
        }
    }

    /// <summary>
    /// Decides the next kind down from the given parent kind. Returns null if the parent
    /// is already a Team (we do not subdivide teams).
    /// </summary>
    private static OrgUnitKind? NextKind(OrgUnitKind parent) => parent switch
    {
        OrgUnitKind.Executive => OrgUnitKind.Function,
        OrgUnitKind.Function  => OrgUnitKind.Region,
        OrgUnitKind.Region    => OrgUnitKind.Area,
        OrgUnitKind.Area      => OrgUnitKind.Branch,
        OrgUnitKind.Branch    => OrgUnitKind.Team,
        OrgUnitKind.Site      => OrgUnitKind.Team,
        OrgUnitKind.Team      => null,
        _                     => null
    };

    private static readonly string[] RegionLabels =
    [
        "Greater Accra", "Ashanti", "Northern", "Western", "Central",
        "Eastern", "Volta", "Upper East", "Upper West", "Bono", "Ahafo"
    ];

    private static readonly string[] AreaLabels =
    [
        "Central Area", "Northern Area", "Southern Area", "Eastern Area", "Western Area",
        "Industrial Area", "Commercial Area", "Suburban Area"
    ];

    private static readonly string[] BranchLabels =
    [
        "Main Branch", "Adum Branch", "Tema Branch", "Spintex Branch", "Madina Branch",
        "Kasoa Branch", "Kumasi Branch", "Takoradi Branch", "Tamale Branch", "Cape Coast Branch",
        "Sunyani Branch", "Ho Branch", "Koforidua Branch", "Wa Branch", "Bolgatanga Branch",
        "East Legon Branch", "Osu Branch", "Lapaz Branch", "Achimota Branch", "Airport City Branch"
    ];

    private static (string Suffix, string Label) ChildLabel(OrgUnitKind parentKind, OrgUnitKind childKind, int index)
    {
        return childKind switch
        {
            OrgUnitKind.Region => (
                Slug(RegionLabels[index % RegionLabels.Length]),
                RegionLabels[index % RegionLabels.Length]),
            OrgUnitKind.Area => (
                $"AREA{index + 1:D2}",
                AreaLabels[index % AreaLabels.Length]),
            OrgUnitKind.Branch => (
                $"BR{index + 1:D3}",
                BranchLabels[index % BranchLabels.Length]),
            OrgUnitKind.Team => (
                $"T{index + 1:D2}",
                $"Team {index + 1}"),
            _ => ($"S{index + 1:D2}", $"Section {index + 1}")
        };
    }

    private static string Slug(string s) => s.ToUpperInvariant().Replace(' ', '_');
}
