using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

public sealed class SdkData17JsonbGuardTests
{
    private static readonly (string FileName, string PropertySnippet)[] KnownJsonbMappings =
    [
        ("AuditableEntityConfiguration.cs", "e => e.CustomFields"),
        ("AppSettingConfiguration.cs", "as_ => as_.ValidationRules"),
        ("OutboxMessageConfiguration.cs", "x => x.Payload"),
        ("TaxationSharedConfiguration.cs", "e => e.InvoiceDistributions"),
        ("TaxationSharedConfiguration.cs", "e => e.RefundDistributions"),
    ];

    private static readonly string[] ForbiddenJsonQueryNeedles =
    [
        "EF.Functions.Json",
        "JsonContains",
        "JsonValue",
        "->>",
        "jsonb_",
        "NpgsqlJsonDbFunctionsExtensions",
    ];

    private static readonly string[] FeatureScanRoots =
    [
        "Features",
        Path.Combine("AppSettings", "Features"),
    ];

    [Theory]
    [MemberData(nameof(KnownMappingCases))]
    public void Shared_Common_jsonb_mappings_remain_jsonb(string fileName, string propertySnippet)
    {
        var path = FindSourceFile(fileName, "QimErp.Shared.Common");
        path.Should().NotBeNull(because: fileName);
        var content = File.ReadAllText(path!);

        content.Should().Contain($"Property({propertySnippet})", because: $"{fileName} must map {propertySnippet}");
        AssertPropertyHasJsonbColumnType(content, propertySnippet, fileName);
    }

    [Fact]
    public void Shared_Common_jsonb_inventory_matches_allowlist()
    {
        var configsDir = FindProjectDirectory("QimErp.Shared.Common");
        configsDir.Should().NotBeNull();
        var configFiles = Directory.GetFiles(
                Path.Combine(configsDir!, "Database", "Configurations"),
                "*.cs",
                SearchOption.AllDirectories)
            .Where(p => !p.EndsWith(".Designer.cs", StringComparison.Ordinal))
            .ToList();

        var found = new HashSet<(string File, string Prop)>(StringTupleComparer.Instance);
        var propertyLambda = new Regex(
            @"Property\(\s*[a-zA-Z_][\w]*\s*=>\s*[a-zA-Z_][\w]*\.(?<prop>[A-Za-z_][\w]*)\s*\)",
            RegexOptions.Compiled);

        foreach (var file in configFiles)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("HasColumnType(\"jsonb\")", StringComparison.Ordinal)
                && !content.Contains("HasColumnType(\"json\")", StringComparison.Ordinal))
                continue;

            var lines = content.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains("HasColumnType(\"jsonb\")", StringComparison.Ordinal)
                    && !lines[i].Contains("HasColumnType(\"json\")", StringComparison.Ordinal))
                    continue;

                for (var j = i; j >= Math.Max(0, i - 12); j--)
                {
                    var m = propertyLambda.Match(lines[j]);
                    if (!m.Success) continue;
                    found.Add((Path.GetFileName(file), m.Groups["prop"].Value));
                    break;
                }
            }
        }

        var expected = KnownJsonbMappings
            .Select(m => (m.FileName, m.PropertySnippet.Split('.')[^1]))
            .ToHashSet(StringTupleComparer.Instance);

        found.Should().BeEquivalentTo(expected,
            because: "DATA-17: every Shared.Common jsonb/json mapping must be on the Path A allowlist");
    }

    [Fact]
    public void Features_have_no_jsonb_query_predicates()
    {
        var project = FindProjectDirectory("QimErp.Shared.Common");
        project.Should().NotBeNull();

        var offenders = new List<string>();
        foreach (var rootName in FeatureScanRoots)
        {
            var root = Path.Combine(project!, rootName);
            if (!Directory.Exists(root)) continue;

            foreach (var file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(file);
                foreach (var needle in ForbiddenJsonQueryNeedles)
                {
                    if (content.Contains(needle, StringComparison.Ordinal))
                        offenders.Add($"{RelativeTo(project!, file)}: {needle}");
                }
            }
        }

        offenders.Should().BeEmpty(
            because: "DATA-17: jsonb columns are blob-only — no EF.Functions.Json* / jsonb SQL filters in Features");
    }

    public static TheoryData<string, string> KnownMappingCases()
    {
        var data = new TheoryData<string, string>();
        foreach (var (file, prop) in KnownJsonbMappings)
            data.Add(file, prop);
        return data;
    }

    private static void AssertPropertyHasJsonbColumnType(string content, string propertySnippet, string label)
    {
        var marker = $"Property({propertySnippet})";
        var idx = content.IndexOf(marker, StringComparison.Ordinal);
        idx.Should().BeGreaterThanOrEqualTo(0, because: label);

        var window = content[idx..Math.Min(content.Length, idx + 400)];
        (window.Contains("HasColumnType(\"jsonb\")", StringComparison.Ordinal)
         || window.Contains("HasColumnType(\"json\")", StringComparison.Ordinal))
            .Should().BeTrue(because: $"{label} {propertySnippet} must remain jsonb/json (DATA-17)");
    }

    private static string RelativeTo(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string? FindProjectDirectory(string projectFolder)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var project = dir.GetDirectories(projectFolder, SearchOption.AllDirectories)
                .FirstOrDefault(d => !d.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.Name.EndsWith("Tests", StringComparison.Ordinal));
            if (project is not null) return project.FullName;
            dir = dir.Parent;
        }

        return null;
    }

    private static string? FindSourceFile(string fileName, string projectFolder)
    {
        var project = FindProjectDirectory(projectFolder);
        if (project is null) return null;
        return Directory.GetFiles(project, fileName, SearchOption.AllDirectories).FirstOrDefault();
    }

    private sealed class StringTupleComparer : IEqualityComparer<(string, string)>
    {
        public static readonly StringTupleComparer Instance = new();

        public bool Equals((string, string) x, (string, string) y) =>
            string.Equals(x.Item1, y.Item1, StringComparison.Ordinal)
            && string.Equals(x.Item2, y.Item2, StringComparison.Ordinal);

        public int GetHashCode((string, string) obj) =>
            HashCode.Combine(obj.Item1, obj.Item2);
    }
}
