namespace QimErp.Shared.Common.Activities.People;

/// <summary>
/// Temporal workflow interface for People employee-promotion-sync.
/// Implemented by EmployeePromotionSyncWorkflow in QimErp.CoreHr.People.WebApi.
/// Referenced by CoreHr.Performance publishers so they can start the workflow
/// via IWorkflowStarter.
/// Task queue: "qimerp-corehr-people-promotion-sync"
/// </summary>
public interface IEmployeePromotionSyncWorkflow
{
    Task RunAsync(EmployeePromotionSyncRequest request);
}
