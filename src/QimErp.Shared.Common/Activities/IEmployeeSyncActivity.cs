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
    Task SyncEmployeeDeletedAsync(Guid employeeId, string tenantId, string employeeEmail);
}
