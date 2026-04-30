namespace QimErp.Shared.DemoData.Industry;

public static class OrgHierarchyBuilder
{
    public const int MaxDepth = 15;
    private const int TargetLeafSize = 12;
    private const int SubdivideThreshold = 30;
    private const int MinFanOut = 3;
    private const int MaxFanOut = 8;

    public sealed record BaselineUnit(
        string Code,
        string Name,
        string? ParentCode,
        OrgUnitKind Kind,
        IReadOnlyList<string> EligibleJobTitleCodes);

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

                // Once subdivided, the parent becomes a container with 0 direct headcount;
                // the sum lives on its newly-emitted children.
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
