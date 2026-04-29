using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// HROperations.Leave-side child workflow that fills realistic leave history for the
/// employees created by <see cref="ISeedDemoEmployeesWorkflow"/>. Owned by
/// qimerp-leave-demo-seed task queue. Wraps the existing LeaveFullStressDemoSeedService.
/// </summary>
[Workflow("SeedDemoLeaveWorkflow")]
public interface ISeedDemoLeaveWorkflow
{
    [WorkflowRun]
    Task<SeedDemoLeaveResult> RunAsync(SeedDemoLeaveRequest request);
}

public sealed class SeedDemoLeaveRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public int? MaxEmployees { get; set; }
    public bool AllowInProduction { get; set; }
    public bool Force { get; set; }
    public required string SystemUserId { get; set; }
    public required string SystemUserEmail { get; set; }
    public required string SystemUserName { get; set; }
}

public sealed class SeedDemoLeaveResult
{
    public bool Success { get; set; }
    public int EmployeesConsidered { get; set; }
    public int LeaveTypesCreated { get; set; }
    public int LeaveBalancesCreated { get; set; }
    public int LeaveRequestsCreated { get; set; }
    public int TravelPermissionsCreated { get; set; }
    public int PlannerEntriesCreated { get; set; }
    public int LeaveRecallsCreated { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; } = new();
    public TimeSpan Elapsed { get; set; }
}
