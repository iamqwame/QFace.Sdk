using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Performance-side child workflow that seeds demo performance cycles,
/// goals, reviews, and feedback for the seeded tenant.
/// Owned by qimerp-corehr-performance-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoPerformanceWorkflow")]
public interface ISeedDemoPerformanceWorkflow
{
    [WorkflowRun]
    Task<SeedDemoPerformanceResult> RunAsync(SeedDemoPerformanceRequest request);
}

public sealed class SeedDemoPerformanceRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoPerformanceResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
