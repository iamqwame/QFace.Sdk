using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

public sealed class SdkData19GuardTests
{
    // DECISIONS.md 2026-10-15/16 — residual DATA-19 Exception category IDs (string presence only).
    private static readonly string[] ExceptionCategoryIds =
    [
        "owned-line",
        "snapshot",
        "Outbox",
        "actor-name column drop",
    ];

    [Fact]
    public void ActorNameWriteStopTests_exists()
    {
        var path = FindTestFile("ActorNameWriteStopTests.cs");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("CreatedByName.Should().BeEmpty()");
        content.Should().Contain("LastModifiedByName.Should().BeEmpty()");
    }

    [Fact]
    public void AuditableEntity_write_stop_forces_empty_actor_names()
    {
        var path = FindSourceFile("AuditableEntity.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("public AuditableEntity OnCreate");
        content.Should().Contain("public AuditableEntity OnModify");
        content.Should().Contain("public AuditableEntity AddAuditMetadata");

        CountOccurrences(content, "CreatedByName = string.Empty").Should().BeGreaterThanOrEqualTo(2);
        CountOccurrences(content, "LastModifiedByName = string.Empty").Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Decisions_exception_category_ids_are_named_for_review()
    {
        ExceptionCategoryIds.Should().Contain("owned-line");
        ExceptionCategoryIds.Should().Contain("snapshot");
        ExceptionCategoryIds.Should().Contain("Outbox");
        ExceptionCategoryIds.Should().Contain("actor-name column drop");
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

    private static string? FindTestFile(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var matches = dir.GetFiles(fileName, SearchOption.AllDirectories)
                .Where(f => f.FullName.Contains("Tests", StringComparison.Ordinal)
                            && !f.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                            && !f.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .ToList();
            if (matches.Count > 0) return matches[0].FullName;
            dir = dir.Parent;
        }

        return null;
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
