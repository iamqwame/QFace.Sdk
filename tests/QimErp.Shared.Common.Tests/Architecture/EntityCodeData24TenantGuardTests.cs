using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-24: EntityCodeService IQF paths fail closed on blank/whitespace TenantId.
/// </summary>
public class EntityCodeData24TenantGuardTests
{
    [Fact]
    public void EntityCodeService_fail_closes_before_IQF()
    {
        var path = FindSourceFile("EntityCodeService.cs");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("DATA-24");
        content.Should().Contain("EnsureTenantId");
        content.Should().Contain("IsNullOrWhiteSpace(tenantId)");
        content.Should().Contain("IgnoreQueryFilters");
    }

    private static string? FindSourceFile(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var match = dir.GetFiles(fileName, SearchOption.AllDirectories)
                .FirstOrDefault(f => f.FullName.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !f.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !f.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
            if (match is not null) return match.FullName;
            dir = dir.Parent;
        }

        return null;
    }
}
