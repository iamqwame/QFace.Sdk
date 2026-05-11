using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// EmployeeEngagement-side child workflow that seeds demo engagement
/// programmes, pulses, recognitions, and announcements for the seeded tenant.
/// Owned by qimerp-hroperations-employee-engagement-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoEmployeeEngagementWorkflow")]
public interface ISeedDemoEmployeeEngagementWorkflow
{
    [WorkflowRun]
    Task<SeedDemoEmployeeEngagementResult> RunAsync(SeedDemoEmployeeEngagementRequest request);
}

public sealed class SeedDemoEmployeeEngagementRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoEmployeeEngagementResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
