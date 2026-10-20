using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-25: Shared.Common ships BlindIndex + DataClassification for PII exact-lookup beachheads.
/// </summary>
public class Data25BlindIndexGuardTests
{
    [Fact]
    public void BlindIndex_matches_DATA25_HMAC_contract()
    {
        var path = FindSourceFile("BlindIndex.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("DATA-25");
        content.Should().Contain("HMACSHA256");
        content.Should().Contain("ToLowerInvariant");
        content.Should().Contain("Compute");
    }

    [Fact]
    public void DataClassification_enum_covers_Pii_and_Sensitive()
    {
        var path = FindSourceFile("DataClassification.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("DATA-25");
        content.Should().Contain("Pii");
        content.Should().Contain("Sensitive");
    }

    [Fact]
    public void PiiFieldProtector_ships_AesGcm_protect_unprotect()
    {
        var path = FindSourceFile("PiiFieldProtector.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("DATA-25");
        content.Should().Contain("AesGcm");
        content.Should().Contain("Protect");
        content.Should().Contain("Unprotect");
    }

    [Fact]
    public void AddPiiFieldProtector_fails_closed_in_Production()
    {
        var path = FindSourceFile("PiiFieldProtectorServiceCollectionExtensions.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("AddPiiFieldProtector");
        content.Should().Contain("IsProduction");
        content.Should().Contain("PiiFieldProtector:KeyBase64");
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
