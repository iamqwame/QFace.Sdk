using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.People;

/// <summary>
/// Versioned contract for Leave to update the People-owned, read-only
/// "who is out" projection. It contains only the values required by that
/// projection and never carries caller credentials or employee PII.
/// </summary>
public sealed class LeaveDashboardProjectionSyncRequest
{
    public Guid EventId { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public Guid LeaveRequestId { get; init; }
    public Guid EmployeeId { get; init; }
    public string LeaveTypeName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public bool IsApproved { get; init; }
    public DateTime OccurredAtUtc { get; init; }
}

[Workflow("LeaveDashboardProjectionSyncWorkflow")]
public interface ILeaveDashboardProjectionSyncWorkflow
{
    [WorkflowRun]
    Task RunAsync(LeaveDashboardProjectionSyncRequest request);
}
