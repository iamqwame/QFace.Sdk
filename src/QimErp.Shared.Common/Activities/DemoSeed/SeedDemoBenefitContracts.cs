using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Benefit-side child workflow that seeds demo benefit plans, enrollments,
/// and dependents for the seeded tenant.
/// Owned by qimerp-hroperations-benefit-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoBenefitWorkflow")]
public interface ISeedDemoBenefitWorkflow
{
    [WorkflowRun]
    Task<SeedDemoBenefitResult> RunAsync(SeedDemoBenefitRequest request);
}

public sealed class SeedDemoBenefitRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoBenefitResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
