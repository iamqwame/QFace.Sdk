using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-27: GenerateBatchAsync batches sequence allocation on the unreserved fast path.
/// </summary>
public class EntityCodeData27GuardTests
{
    [Fact]
    public void EntityCodeService_batches_sequence_on_unreserved_fast_path()
    {
        var path = FindSourceFile("EntityCodeService.cs");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("DATA-27");
        content.Should().Contain("HasActiveReservationsAsync");
        content.Should().Contain("IncrementSequenceAsync(tenantId, companyId, entityType, count");
    }

    [Fact]
    public void EntityCodeService_documents_reservation_skip_residual()
    {
        var path = FindSourceFile("EntityCodeService.cs");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);
        content.Should().Contain("DATA-27 residual");
        content.Should().Contain("GenerateBatchWithReservationSkipAsync");
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
