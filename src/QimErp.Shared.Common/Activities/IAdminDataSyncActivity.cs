using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Activity interface implemented by each module that maintains a local copy of
/// CoreHR admin reference data (JobTitle, JobStatus, OrgUnit, Station).
///
/// Each implementing module registers a Temporal worker on its own task queue:
///   qimerp-corehr-employee-admin-sync  (Employee WebApi)
///   qimerp-payroll-admin-sync          (Payroll WebApi)
///
/// AdminDataSyncWorkflow fans out to all module queues in parallel.
/// </summary>
public interface IAdminDataSyncActivity
{

    [Activity] Task SyncJobTitleAsync(AdminDataSyncRequest request);
    [Activity] Task SyncJobStatusAsync(AdminDataSyncRequest request);
    [Activity] Task SyncOrganizationalUnitAsync(AdminDataSyncRequest request);
    [Activity] Task SyncStationAsync(AdminDataSyncRequest request);
}
