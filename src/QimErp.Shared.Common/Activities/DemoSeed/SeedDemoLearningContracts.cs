using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Learning-side child workflow that seeds demo learning paths, courses,
/// enrollments, certifications, and assessments for the seeded tenant.
/// Owned by qimerp-corehr-learning-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoLearningWorkflow")]
public interface ISeedDemoLearningWorkflow
{
    [WorkflowRun]
    Task<SeedDemoLearningResult> RunAsync(SeedDemoLearningRequest request);
}

public sealed class SeedDemoLearningRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoLearningResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
