namespace QimErp.Shared.Common.Activities.Workflow;

/// <summary>
/// Ids-only actor sync envelope (DATA-11). Platform hydrates display fields via
/// People sync-projection; do not put names/emails on this contract.
/// </summary>
public sealed class WorkflowActorSyncEnvelope
{
    public WorkflowActorSourceType SourceType { get; set; }

    public Guid SourceId { get; set; }

    public string TenantId { get; set; } = string.Empty;

    public long? Version { get; set; }
}
