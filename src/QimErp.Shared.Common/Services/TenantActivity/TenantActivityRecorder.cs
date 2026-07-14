using Microsoft.AspNetCore.Http;
using QFace.Sdk.ActorSystems;
using QimErp.Shared.Common.Activities.TenantActivity;
using QimErp.Shared.Common.Actors;

namespace QimErp.Shared.Common.Services.TenantActivity;

public class TenantActivityRecorder(
    IActorService actorService,
    IHttpContextAccessor httpContextAccessor) : ITenantActivityRecorder
{
    public void Record(RecordTenantActivityRequest request)
    {
        var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : request.CorrelationId;

        var requestContext = AuditRequestContext.TryCapture(httpContextAccessor);
        var metadataJson = AuditMetadataMerger.Merge(request.MetadataJson, requestContext);

        var normalized = request with
        {
            CorrelationId = correlationId,
            MetadataJson = metadataJson
        };
        var workflowId = $"tenant-activity-{normalized.TenantId}-{correlationId}";
        actorService.Tell<TenantActivityPublisherActor>(new TenantActivityPublishMessage(normalized, workflowId));
    }
}
