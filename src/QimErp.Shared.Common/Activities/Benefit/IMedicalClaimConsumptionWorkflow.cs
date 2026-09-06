namespace QimErp.Shared.Common.Activities.Benefit;

/// <summary>
/// Implemented by MedicalClaimConsumptionWorkflow in QimErp.HrOperations.Benefit.WebApi.
/// Referenced by claim-owning modules so they can start the workflow via IWorkflowStarter
/// without a cross-project dependency on the Benefit WebApi.
/// Task queue: "qimerp-benefit-medical-claim-consumption"
/// </summary>
public interface IMedicalClaimConsumptionWorkflow
{
    Task RunAsync(MedicalClaimConsumptionSyncRequest request);
}
