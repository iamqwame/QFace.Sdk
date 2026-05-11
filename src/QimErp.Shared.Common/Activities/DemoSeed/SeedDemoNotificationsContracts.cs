using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Notifications-side child workflow that seeds demo notification templates,
/// channel configurations, and historical delivery logs for the seeded tenant.
/// Owned by qimerp-platform-notifications-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoNotificationsWorkflow")]
public interface ISeedDemoNotificationsWorkflow
{
    [WorkflowRun]
    Task<SeedDemoNotificationsResult> RunAsync(SeedDemoNotificationsRequest request);
}

public sealed class SeedDemoNotificationsRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public bool AllowInProduction { get; set; }
    public string SystemUserId { get; set; } = "demo-seed-system";
    public string SystemUserEmail { get; set; } = "demo-seed@qimerp.com";
    public string SystemUserName { get; set; } = "DemoSeedOrchestrator";
}

public sealed class SeedDemoNotificationsResult
{
    public bool Success { get; set; }
    public bool AlreadySeeded { get; set; }
    public int RowsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Elapsed { get; set; }
}
