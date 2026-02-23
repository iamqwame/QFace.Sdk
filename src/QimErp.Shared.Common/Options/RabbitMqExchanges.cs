namespace QimErp.Shared.Common.Options;

/// <summary>
/// RabbitMQ exchange names. Bind from "RabbitMq:Exchanges" section.
/// Env: RabbitMq__Exchanges__Notify, RabbitMq__Exchanges__WorkflowApprovalRequired, etc.
/// </summary>
public class RabbitMqExchanges
{
    public const string DefaultNotification = "qimerp.core.notify.prod_exchange";
    public const string DefaultWorkflowApprovalRequired = "qimerp.workflow.workflow_approval_required.prod_exchange";
    public const string DefaultWorkflowChanged = "qimerp.workflow.workflow_changed.prod_exchange";
    public const string DefaultWorkflowStatusChanged = "qimerp.workflow.workflow_status_changed.prod_exchange";
    public const string DefaultWorkflowCompleted = "qimerp.workflow.workflow_completed.prod_exchange";
    public const string DefaultWorkflowApprovalRequest = "qimerp.workflow.workflow_approval_request.prod_exchange";

    public string Notification { get; set; } = DefaultNotification;
    public string WorkflowApprovalRequired { get; set; } = DefaultWorkflowApprovalRequired;
    public string WorkflowChanged { get; set; } = DefaultWorkflowChanged;
    public string WorkflowStatusChanged { get; set; } = DefaultWorkflowStatusChanged;
    public string WorkflowCompleted { get; set; } = DefaultWorkflowCompleted;
    public string WorkflowApprovalRequest { get; set; } = DefaultWorkflowApprovalRequest;
}
