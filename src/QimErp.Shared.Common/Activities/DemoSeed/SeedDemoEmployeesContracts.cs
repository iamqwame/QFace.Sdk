using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// CoreHr-side child workflow that scaffolds org structure and bulk-seeds N realistic
/// employees for a demo tenant. Owned by qimerp-corehr-demo-seed task queue. Invoked
/// from the platform parent <see cref="IDemoTenantSeedWorkflow"/>.
/// </summary>
[Workflow("SeedDemoEmployeesWorkflow")]
public interface ISeedDemoEmployeesWorkflow
{
    [WorkflowRun]
    Task<SeedDemoEmployeesResult> RunAsync(SeedDemoEmployeesRequest request);

    /// <summary>Live progress for parent-workflow signalling and status polling.</summary>
    [WorkflowQuery]
    SeedProgress GetProgress();
}

public sealed class SeedDemoEmployeesRequest
{
    public required string TenantId { get; set; }
    public required string Domain { get; set; }
    public required int Count { get; set; }
    public required string IndustryCode { get; set; }
    public required string CompanyTypeCode { get; set; }
    public string CountryCode { get; set; } = "GH";
    public Guid? SuperAdminEmployeeId { get; set; }
    public bool DryRun { get; set; }
    public int? RandomSeed { get; set; }
    public required string SystemUserId { get; set; }
    public required string SystemUserEmail { get; set; }
    public required string SystemUserName { get; set; }
    public required string WorkEmailDomain { get; set; }
    public string? ParentWorkflowId { get; set; }

    /// <summary>
    /// Company display name (e.g. "Cal Bank", "Analic Systems Ltd"). Threaded into the
    /// row factory so static profile descriptions / responsibilities / KPIs that carry
    /// the {Company} placeholder render naturally for each tenant. When missing, the
    /// row factory falls back to "the company" so descriptions still read sensibly.
    /// </summary>
    public string? CompanyName { get; set; }
}

public sealed class SeedDemoEmployeesResult
{
    public bool Success { get; set; }
    public int OrgUnitsCreated { get; set; }
    public int JobTitlesCreated { get; set; }
    public int StationsCreated { get; set; }
    public int EmployeesCreated { get; set; }
    public int EmployeesFailed { get; set; }
    public int SupervisorsAssigned { get; set; }
    public List<string> Errors { get; } = new();
    public TimeSpan Elapsed { get; set; }
}

public sealed class SeedProgress
{
    public string Phase { get; set; } = "Pending"; // Pending | OrgScaffolding | Employees | Supervisors | Done
    public int Total { get; set; }
    public int Completed { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public string? CurrentBatchInfo { get; set; }
}
