namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Temporal workflow interface for admin reference data sync fan-out.
/// Implemented by AdminDataSyncWorkflow in QimErp.CoreHr.Employee.WebApi.
/// Referenced here so admin services (in Shared) can start the workflow
/// without a cross-project dependency on the WebApi.
/// </summary>
public interface IAdminDataSyncWorkflow
{
    Task RunAsync(AdminDataSyncRequest request);
}
