using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.DemoSeed;

/// <summary>
/// Payroll-side child workflow that seeds realistic payroll configuration and
/// per-employee salary structures, provident funds, loans, and advances for
/// the employees already synced to Payroll from CoreHr.
/// Owned by qimerp-payroll-demo-seed task queue.
/// </summary>
[Workflow("SeedDemoPayrollWorkflow")]
public interface ISeedDemoPayrollWorkflow
{
    [WorkflowRun]
    Task<SeedDemoPayrollResult> RunAsync(SeedDemoPayrollRequest request);
}

public sealed class SeedDemoPayrollRequest
{
    public required string TenantId { get; set; }
    public bool DryRun { get; set; }
    public int? MaxEmployees { get; set; }
    public bool AllowInProduction { get; set; }
    public bool Force { get; set; }
    public required string SystemUserId { get; set; }
    public required string SystemUserEmail { get; set; }
    public required string SystemUserName { get; set; }
}

public sealed class SeedDemoPayrollResult
{
    public bool Success { get; set; }
    public int EmployeesConsidered { get; set; }
    public int ConfigurationsCreated { get; set; }
    public int GradesCreated { get; set; }
    public int SalaryStructuresCreated { get; set; }
    public int ProvidentFundsCreated { get; set; }
    public int LoansCreated { get; set; }
    public int AdvancesCreated { get; set; }
    public int AllowancesCreated { get; set; }
    public int DeductionsCreated { get; set; }
    public int PayrollRunsCreated { get; set; }
    public int PayrollItemsCreated { get; set; }
    public int PayslipsCreated { get; set; }
    public int ReportsCreated { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; } = new();
    public TimeSpan Elapsed { get; set; }
}
