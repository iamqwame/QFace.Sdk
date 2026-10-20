using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Workflow;

/// <summary>
/// DATA-20: workflow-enabled aggregates must inherit Version concurrency from AuditableEntityConfiguration.
/// </summary>
public class WorkflowEnabledEntityConfigurationData20GuardTests
{
    [Fact]
    public void WorkflowEnabledEntityConfiguration_inherits_auditable_and_calls_base_configure()
    {
        var path = FindSourceFile("WorkflowEnabledEntityConfiguration.cs");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("DATA-20");
        content.Should().Contain(": AuditableEntityConfiguration<TEntity>");
        content.Should().Contain("base.Configure(builder)");
    }

    private static string? FindSourceFile(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var match = dir.GetFiles(fileName, SearchOption.AllDirectories)
                .FirstOrDefault(f => f.FullName.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !f.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
            if (match is not null) return match.FullName;
            dir = dir.Parent;
        }

        return null;
    }
}
