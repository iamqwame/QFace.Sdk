using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// WorkforcePlanning-side child workflow that seeds demo headcount plans,
/// scenarios, and forecasts for the seeded tenant.
/// Owned by qimerp-corehr-workforce-planning-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoWorkforcePlanningWorkflow")]
public interface ISeedDemoWorkforcePlanningWorkflow
{
    [WorkflowRun]
    Task<SeedDemoWorkforcePlanningResult> RunAsync(SeedDemoWorkforcePlanningRequest request);
}

public sealed class SeedDemoWorkforcePlanningRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoWorkforcePlanningResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
