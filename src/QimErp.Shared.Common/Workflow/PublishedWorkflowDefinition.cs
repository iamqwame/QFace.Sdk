namespace QimErp.Shared.Common.Workflow;

/// <summary>
/// Tenant-scoped published workflow definition stored in Platform PostgreSQL and Redis cache.
/// </summary>
public class PublishedWorkflowDefinition
{
    public string TenantId { get; set; } = string.Empty;
    public string WorkflowCode { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public Guid? TemplateId { get; set; }
    public WorkflowDefinition Definition { get; set; } = new();
}
