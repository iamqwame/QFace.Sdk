using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.TenantActivity;

public interface ITenantActivityRecordActivity
{
    // No CancellationToken parameter — Temporal serializes every argument in the workflow's
    // ExecuteActivityAsync(expression) call, and CancellationToken exposes a WaitHandle
    // (backed by an unmanaged IntPtr) that the default JSON payload converter cannot
    // serialize. That silently failed every activity attempt, so no tenant activity was
    // ever actually persisted. The activity doesn't need cancellation support today; add
    // it back via ActivityExecutionContext.Current.CancellationToken inside the method body
    // if that ever changes, not as a caller-supplied parameter.
    [Activity("RecordTenantActivity:IAM")]
    Task RecordAsync(RecordTenantActivityRequest request);
}
