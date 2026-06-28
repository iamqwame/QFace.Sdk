using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities.TenantActivity;

public interface ITenantActivityRecordActivity
{
    [Activity("RecordTenantActivity:IAM")]
    Task RecordAsync(RecordTenantActivityRequest request, CancellationToken cancellationToken = default);
}
