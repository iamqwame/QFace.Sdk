namespace QimErp.Shared.Common.Options;

/// <summary>
/// RabbitMQ exchange configuration. Bind from "RabbitMq" section.
/// Env: RabbitMq__NotificationsExchange, RabbitMq__WorkflowApprovalRequiredExchange, etc.
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public const string DefaultNotificationsExchange = "qimerp.core.notify.prod_exchange";
    public const string DefaultWorkflowApprovalRequiredExchange = "qimerp.workflow.workflow_approval_required.prod_exchange";
    public const string DefaultWorkflowChangedExchange = "qimerp.workflow.workflow_changed.prod_exchange";
    public const string DefaultWorkflowStatusChangedExchange = "qimerp.workflow.workflow_status_changed.prod_exchange";
    public const string DefaultWorkflowCompletedExchange = "qimerp.workflow.workflow_completed.prod_exchange";

    public string NotificationsExchange { get; set; } = DefaultNotificationsExchange;
    public string WorkflowApprovalRequiredExchange { get; set; } = DefaultWorkflowApprovalRequiredExchange;
    public string WorkflowChangedExchange { get; set; } = DefaultWorkflowChangedExchange;
    public string WorkflowStatusChangedExchange { get; set; } = DefaultWorkflowStatusChangedExchange;
    public string WorkflowCompletedExchange { get; set; } = DefaultWorkflowCompletedExchange;
}
