using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Activity interface that each module WebApi implements to sync employee data
/// into its own local database when an employee is created, updated, or deleted in CoreHR.
///
/// Each module WebApi registers a Temporal worker on "qimerp-{module}-employee-sync"
/// and a scoped implementation of this interface.
///
/// EmployeeSyncWorkflow fans out to all module queues in parallel, calling the
/// appropriate method based on the operation type.
///
/// Contract for implementers:
/// - Default behavior is MIRROR-ONLY: create/update/deactivate a local read-model copy of the
///   CoreHR employee. Most modules (Payroll, Learning, Benefit, Talent, WorkforcePlanning,
///   Performance, EmployeeEngagement, Surveys) stop here — this is correct as-is, not a gap.
/// - A module that needs POST-MIRROR CONFIGURATION (e.g. Leave's EnsureEmployeeLeaveConfiguredAsync,
///   which creates the employee's EmployeeLeave balance row) must call its idempotent Ensure*
///   primitive from ALL FOUR of these places, gated by ModuleSyncActivityGuard.ShouldProcess:
///     1. SyncEmployeeCreatedAsync — the new-mirror-inserted branch
///     2. SyncEmployeeCreatedAsync — the mirror-already-exists branch (re-sync / partial prior sync)
///     3. SyncEmployeesCreatedBulkAsync — for EVERY incoming employee ID, not just newly-inserted ones
///        (this is also what InstallCatalogItemWorkflow's module-install employee backfill calls)
///     4. The module's own TenantSetupActivity.SyncEmployeeAsync (tenant-onboarding's first-employee
///        insert path), on both the already-exists and newly-inserted branches
///   See QimErp.HROperations's Leave module for the reference implementation of this pattern.
/// </summary>
public interface IEmployeeSyncActivity
{
    [Activity]
    Task SyncEmployeeCreatedAsync(EmployeeChangedEvent employee);

    [Activity]
    Task SyncEmployeeUpdatedAsync(EmployeeChangedEvent employee);

    /// <param name="employeeId">Primary key of the employee to deactivate/delete.</param>
    /// <param name="tenantId">Tenant scope.</param>
    /// <param name="employeeEmail">Email — used by IAM to locate and deactivate the user account.</param>
    [Activity]
    Task SyncEmployeeDeletedAsync(Guid employeeId, string tenantId, string employeeEmail, List<string>? selectedModules);

    /// <summary>
    /// Called when an employee is terminated in CoreHR.
    /// Marks the employee as deactivated in the module's local database so they are excluded
    /// from future payroll runs. A TODO exists to replace with a TerminatedPendingFinalPayment
    /// state once that domain state is introduced.
    /// </summary>
    [Activity]
    Task SyncEmployeeTerminatedAsync(EmployeeChangedEvent employeeChangedEvent);
}
