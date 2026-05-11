using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// TenantBilling-side child workflow that seeds demo subscriptions, invoices,
/// payment methods, and usage records for the seeded tenant.
/// Owned by qimerp-iam-tenant-billing-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoTenantBillingWorkflow")]
public interface ISeedDemoTenantBillingWorkflow
{
    [WorkflowRun]
    Task<SeedDemoTenantBillingResult> RunAsync(SeedDemoTenantBillingRequest request);
}

public sealed class SeedDemoTenantBillingRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoTenantBillingResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
