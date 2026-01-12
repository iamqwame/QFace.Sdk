using QFace.Sdk.RabbitMq.Services;

namespace QimErp.Shared.Common.Actors;

/// <summary>
/// Actor for publishing workflow-related events to RabbitMQ
/// This actor is called by the interceptor to publish workflow events asynchronously
/// </summary>
public class WorkflowEventPublisherActor : BaseActor
{

    public WorkflowEventPublisherActor(
        ILogger<WorkflowEventPublisherActor> logger,
        IServiceProvider serviceProvider)
    {
       

        ReceiveAsync<WorkflowEventMessage>(async message =>
        {
            logger.LogInformation("📤 [WorkflowEventPublisher] Publishing workflow event for {EntityType} {EntityId} with workflow code {WorkflowCode}, WorkflowId={WorkflowId}",
                message.EntityType, message.EntityId, message.WorkflowCode, message.WorkflowId);

            try
            {
                using var scope = serviceProvider.CreateScope();
                var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                var workflowEvent = new WorkflowApprovalRequiredEvent(
                    tenantId: message.TenantId,
                    userEmail: message.InitiatedBy ?? "system@qimerp.com",
                    triggeredBy: message.TriggeredBy,
                    userName: message.UserName)
                {
                    EntityType = message.EntityType,
                    EntityId = message.EntityId,
                    EntityName = message.EntityName,
                    WorkflowId = message.WorkflowId,
                    WorkflowCode = message.WorkflowCode,
                    RequiredApprovalLevel = message.RequiredApprovalLevel,
                    InitiatedBy = message.InitiatedBy,
                    Module = message.Module,
                    EntityData = message.EntityData,
                    CurrentState = message.CurrentState,
                    NextStepCode = message.NextStepCode
                };

                var exchangeName = "qimerp.workflow.workflow_approval_required.prod_exchange";
                var routingKey = "workflow.approval.required";
                
                logger.LogDebug("Publishing to Exchange={ExchangeName}, RoutingKey={RoutingKey}, EntityType={EntityType}, EntityId={EntityId}, WorkflowId={WorkflowId}",
                    exchangeName, routingKey, message.EntityType, message.EntityId, message.WorkflowId);
                
                await publisher.PublishAsync(workflowEvent, exchangeName, routingKey);

                logger.LogInformation("✅ [WorkflowEventPublisher] Successfully published workflow approval required event for {EntityType} {EntityId} to Exchange={ExchangeName} with RoutingKey={RoutingKey}, WorkflowId={WorkflowId}",
                    message.EntityType, message.EntityId, exchangeName, routingKey, message.WorkflowId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ [WorkflowEventPublisher] Failed to publish workflow event for {EntityType} {EntityId}: {ErrorMessage}. Exchange={ExchangeName}, RoutingKey={RoutingKey}",
                    message.EntityType, message.EntityId, ex.Message, "qimerp.workflow.workflow_approval_required.prod_exchange", "workflow.approval.required");
                throw;
            }
        });
    }
}

/// <summary>
/// Message for workflow event publishing
/// </summary>
public class WorkflowEventMessage
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string WorkflowCode { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public string? RequiredApprovalLevel { get; set; }
    public string? InitiatedBy { get; set; }
    public string Module { get; set; } = string.Empty;
    public Dictionary<string, object> EntityData { get; set; } = new();
    public string TenantId { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
    public string? UserName { get; set; }
    public string? CurrentState { get; set; }
    public string? NextStepCode { get; set; }
}
