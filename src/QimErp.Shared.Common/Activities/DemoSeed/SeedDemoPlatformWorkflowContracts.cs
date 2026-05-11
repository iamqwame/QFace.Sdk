using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// PlatformWorkflow-side child workflow that seeds demo workflow definitions,
/// approval chains, and runtime sample instances for the platform Workflow
/// module of the seeded tenant.
///
/// Named "PlatformWorkflow" rather than "Workflow" to avoid colliding with
/// the existing <c>QimErp.Shared.Common.Workflow</c> namespace and the
/// <c>WorkflowConstants</c>/<c>WorkflowConfigCacheService</c> types there.
/// Owned by qimerp-platform-workflow-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoPlatformWorkflowWorkflow")]
public interface ISeedDemoPlatformWorkflowWorkflow
{
    [WorkflowRun]
    Task<SeedDemoPlatformWorkflowResult> RunAsync(SeedDemoPlatformWorkflowRequest request);
}

public sealed class SeedDemoPlatformWorkflowRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoPlatformWorkflowResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
