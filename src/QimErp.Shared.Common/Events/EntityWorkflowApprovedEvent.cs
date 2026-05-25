namespace QimErp.Shared.Common.Events;

public class EntityWorkflowApprovedEvent : DomainEvent
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId   { get; set; } = string.Empty;
    public string Module     { get; set; } = string.Empty;
    public string? WorkflowCode { get; set; }

    public EntityWorkflowApprovedEvent() { }

    private EntityWorkflowApprovedEvent(
        string entityType,
        string entityId,
        string module,
        string tenantId,
        string userEmail,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        EntityType = entityType;
        EntityId   = entityId;
        Module     = module;
    }

    public static EntityWorkflowApprovedEvent Create(
        string entityType,
        string entityId,
        string module,
        string tenantId,
        string userEmail,
        string? workflowCode = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new EntityWorkflowApprovedEvent(entityType, entityId, module, tenantId, userEmail, triggeredBy, userName)
        {
            WorkflowCode = workflowCode
        };
    }
}
