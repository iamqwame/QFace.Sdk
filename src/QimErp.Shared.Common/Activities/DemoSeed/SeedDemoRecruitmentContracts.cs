using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Recruitment-side child workflow that seeds demo job postings, candidates,
/// pipeline stages, interviews, and offers for the seeded tenant.
/// Owned by qimerp-hroperations-recruitment-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoRecruitmentWorkflow")]
public interface ISeedDemoRecruitmentWorkflow
{
    [WorkflowRun]
    Task<SeedDemoRecruitmentResult> RunAsync(SeedDemoRecruitmentRequest request);
}

public sealed class SeedDemoRecruitmentRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoRecruitmentResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
