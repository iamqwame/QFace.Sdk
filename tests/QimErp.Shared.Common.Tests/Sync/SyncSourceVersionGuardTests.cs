using FluentAssertions;
using QimErp.Shared.Common.Sync;
using Xunit;

namespace QimErp.Shared.Common.Tests.Sync;

public class SyncSourceVersionGuardTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(5, 0)]
    [InlineData(5, -1)]
    public void ShouldApplySnapshot_When_EventVersion_Not_Positive_Always_Applies(
        long currentSourceVersion,
        long eventVersion)
    {
        SyncSourceVersionGuard.ShouldApplySnapshot(currentSourceVersion, eventVersion).Should().BeTrue();
    }

    [Fact]
    public void ShouldApplySnapshot_When_Event_Newer_Applies()
    {
        SyncSourceVersionGuard.ShouldApplySnapshot(5, 6).Should().BeTrue();
    }

    [Fact]
    public void ShouldApplySnapshot_When_Event_Equal_Skips()
    {
        SyncSourceVersionGuard.ShouldApplySnapshot(5, 5).Should().BeFalse();
    }

    [Fact]
    public void ShouldApplySnapshot_When_Event_Stale_Skips()
    {
        SyncSourceVersionGuard.ShouldApplySnapshot(5, 3).Should().BeFalse();
    }
}
