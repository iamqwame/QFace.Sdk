using QimErp.Shared.Common.Activities.TenantActivity;

namespace QimErp.Shared.Common.Services.TenantActivity;

public interface ITenantActivityRecorder
{
    void Record(RecordTenantActivityRequest request);
}
