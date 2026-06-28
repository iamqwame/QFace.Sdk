using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.Temporal.Abstractions;
using QimErp.Shared.Common.Activities.TenantActivity;
using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Actors;

public record TenantActivityPublishMessage(RecordTenantActivityRequest Request, string WorkflowId);

public class TenantActivityPublisherActor : BaseActor
{
    public TenantActivityPublisherActor(IServiceProvider serviceProvider, ILogger<TenantActivityPublisherActor> logger)
    {
        ReceiveAsync<TenantActivityPublishMessage>(async message =>
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                tenantContext.SetTenant(message.Request.TenantId);

                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var suffix = configuration["Temporal:TaskQueueSuffix"] ?? "";
                var workflowStarter = scope.ServiceProvider.GetRequiredService<IWorkflowStarter>();

                await workflowStarter.StartOrIgnoreAsync<IRecordTenantActivityWorkflow>(
                    message.WorkflowId,
                    $"qimerp-iam-tenant-activity{suffix}",
                    wf => wf.RunAsync(message.Request));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex,
                    "Failed to start tenant activity workflow {WorkflowId} for tenant {TenantId}",
                    message.WorkflowId,
                    message.Request.TenantId);
            }
        });
    }
}
