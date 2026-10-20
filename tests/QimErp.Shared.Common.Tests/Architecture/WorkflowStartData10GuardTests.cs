using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-10: Temporal starts use deterministic workflow Id + UseExisting conflict policy.
/// </summary>
public class WorkflowStartData10GuardTests
{
    [Fact]
    public void WorkflowStarter_StartOrIgnoreAsync_uses_UseExisting_conflict_policy()
    {
        var path = FindSourceFile("WorkflowStarter.cs", "QFace.Sdk.Temporal");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("StartOrIgnoreAsync");
        content.Should().Contain("WorkflowIdConflictPolicy.UseExisting");
    }

    [Fact]
    public void TemporalEventTransport_starts_with_UseExisting()
    {
        var path = FindSourceFile("TemporalEventTransport.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("WorkflowIdConflictPolicy.UseExisting");
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
