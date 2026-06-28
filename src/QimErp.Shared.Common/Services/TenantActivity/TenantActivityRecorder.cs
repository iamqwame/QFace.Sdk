using QFace.Sdk.ActorSystems;
using QimErp.Shared.Common.Activities.TenantActivity;
using QimErp.Shared.Common.Actors;

namespace QimErp.Shared.Common.Services.TenantActivity;

public class TenantActivityRecorder(IActorService actorService) : ITenantActivityRecorder
{
    public void Record(RecordTenantActivityRequest request)
    {
        var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : request.CorrelationId;

        var normalized = request with { CorrelationId = correlationId };
        var workflowId = $"tenant-activity-{normalized.TenantId}-{correlationId}";
        actorService.Tell<TenantActivityPublisherActor>(new TenantActivityPublishMessage(normalized, workflowId));
    }
}
