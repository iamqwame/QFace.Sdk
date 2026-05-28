namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Optional marker for history/chart rows that must not affect parent projections
/// until workflow approval completes.
/// </summary>
public interface IWorkflowDeferredActivation
{
    bool IsEffective { get; }
}
