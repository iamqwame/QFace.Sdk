using Temporalio.Workflows;

namespace QimErp.Shared.Common.Activities.TenantActivity;

[Workflow("RecordTenantActivityWorkflow")]
public interface IRecordTenantActivityWorkflow
{
    [WorkflowRun]
    Task RunAsync(RecordTenantActivityRequest request);
}
