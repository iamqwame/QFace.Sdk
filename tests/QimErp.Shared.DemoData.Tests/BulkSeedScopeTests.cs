using FluentAssertions;
using QimErp.Shared.Common.Services;
using Xunit;

namespace QimErp.Shared.DemoData.Tests;

public class BulkSeedScopeTests
{
    [Fact]
    public void IsSuppressed_DefaultsToFalse()
    {
        BulkSeedScope.IsSuppressed.Should().BeFalse();
    }

    [Fact]
    public void Enter_FlipsToTrue()
    {
        using var _ = BulkSeedScope.Enter();
        BulkSeedScope.IsSuppressed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_RestoresPreviousValue()
    {
        using (BulkSeedScope.Enter())
        {
            BulkSeedScope.IsSuppressed.Should().BeTrue();
        }

        BulkSeedScope.IsSuppressed.Should().BeFalse();
    }

    [Fact]
    public void Enter_IsReentrantSafe()
    {
        using (BulkSeedScope.Enter())
        {
            BulkSeedScope.IsSuppressed.Should().BeTrue();

            using (BulkSeedScope.Enter())
            {
                BulkSeedScope.IsSuppressed.Should().BeTrue();
            }

            // Inner dispose must not flip outer scope back to false.
            BulkSeedScope.IsSuppressed.Should().BeTrue();
        }

        BulkSeedScope.IsSuppressed.Should().BeFalse();
    }

    [Fact]
    public async Task Enter_IsAsyncLocal_DoesNotLeakAcrossTasks()
    {
        var enteredGate = new TaskCompletionSource();
        var observedGate = new TaskCompletionSource();
        bool? observedValue = null;

        var enterer = Task.Run(async () =>
        {
            using var _ = BulkSeedScope.Enter();
            enteredGate.SetResult();
            // Hold the scope open until the observer task records its value.
            await observedGate.Task;
        });

        var observer = Task.Run(async () =>
        {
            await enteredGate.Task;
            observedValue = BulkSeedScope.IsSuppressed;
            observedGate.SetResult();
        });

        await Task.WhenAll(enterer, observer);

        observedValue.Should().BeFalse();
        BulkSeedScope.IsSuppressed.Should().BeFalse();
    }

    [Fact]
    public async Task Enter_FlowsAcrossAwaits()
    {
        using var _ = BulkSeedScope.Enter();
        await Task.Yield();
        BulkSeedScope.IsSuppressed.Should().BeTrue();
    }
}
