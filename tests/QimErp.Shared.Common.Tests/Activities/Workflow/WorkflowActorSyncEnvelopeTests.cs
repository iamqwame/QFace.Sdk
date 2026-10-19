using FluentAssertions;
using QimErp.Shared.Common.Activities.Workflow;
using Xunit;

namespace QimErp.Shared.Common.Tests.Activities.Workflow;

public sealed class WorkflowActorSyncEnvelopeTests
{
    [Fact]
    public void Envelope_carries_ids_only_shape()
    {
        var props = typeof(WorkflowActorSyncEnvelope).GetProperties()
            .Select(p => p.Name)
            .OrderBy(n => n)
            .ToArray();

        props.Should().BeEquivalentTo(
            ["SourceId", "SourceType", "TenantId", "Version"],
            opts => opts.WithStrictOrdering());
    }

    [Fact]
    public void IWorkflowActorSyncActivity_exposes_V2_methods()
    {
        var names = typeof(IWorkflowActorSyncActivity).GetMethods()
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        names.Should().Contain("SyncActorCreatedV2Async");
        names.Should().Contain("SyncActorUpdatedV2Async");
        names.Should().Contain("SyncActorCreatedAsync");
        names.Should().Contain("SyncActorUpdatedAsync");
    }
}
