using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Talent-side child workflow that seeds demo talent profiles, succession
/// plans, skills inventory, and career paths for the seeded tenant.
/// Owned by qimerp-corehr-talent-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoTalentWorkflow")]
public interface ISeedDemoTalentWorkflow
{
    [WorkflowRun]
    Task<SeedDemoTalentResult> RunAsync(SeedDemoTalentRequest request);
}

public sealed class SeedDemoTalentRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoTalentResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
