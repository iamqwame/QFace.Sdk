using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// Owned VO helpers expose optional includeIndex for Id indexes on Configure / ConfigureOptional.
/// </summary>
public class ValueObjectIncludeIndexData26GuardTests
{
    private static readonly string[] VoExtensionFiles =
    [
        "EntityTypeBuilderEmployeeValueObjectExtensions.cs",
        "EntityTypeBuilderVendorValueObjectExtensions.cs",
        "EntityTypeBuilderCustomerValueObjectExtensions.cs",
        "EntityTypeBuilderProductValueObjectExtensions.cs",
        "EntityTypeBuilderJobTitleValueObjectExtensions.cs",
        "EntityTypeBuilderOrganizationalUnitValueObjectExtensions.cs",
        "EntityTypeBuilderRankValueObjectExtensions.cs",
        "EntityTypeBuilderStationValueObjectExtensions.cs",
        "EntityTypeBuilderJobStatusValueObjectExtensions.cs",
        "EntityTypeBuilderCostCenterValueObjectExtensions.cs",
        "EntityTypeBuilderChartOfAccountValueObjectExtensions.cs",
    ];

    [Fact]
    public void All_VO_extension_Configure_methods_expose_includeIndex()
    {
        foreach (var fileName in VoExtensionFiles)
        {
            var path = FindSourceFile(fileName, "QimErp.Shared.Common");
            path.Should().NotBeNull(because: "{0} must exist under Shared.Common", fileName);

            var content = File.ReadAllText(path!);
            content.Should().Contain("bool includeIndex = false", because: "{0} must declare includeIndex", fileName);
            content.Should().Contain("if (includeIndex)", because: "{0} must gate HasIndex on includeIndex", fileName);
            content.Should().Contain("HasIndex(p => p.Id)", because: "{0} must index Id when includeIndex is true", fileName);

            var includeIndexDeclarations = CountOccurrences(content, "bool includeIndex = false");
            includeIndexDeclarations.Should().BeGreaterThanOrEqualTo(2,
                because: "{0} must expose includeIndex on both Configure and ConfigureOptional", fileName);
        }
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }

    private static string? FindSourceFile(string fileName, string projectFolder)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var project = dir.GetDirectories(projectFolder, SearchOption.AllDirectories)
                .FirstOrDefault(d => !d.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.Name.EndsWith("Tests", StringComparison.Ordinal));
            if (project is not null)
            {
                var match = project.GetFiles(fileName, SearchOption.AllDirectories).FirstOrDefault();
                if (match is not null) return match.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
