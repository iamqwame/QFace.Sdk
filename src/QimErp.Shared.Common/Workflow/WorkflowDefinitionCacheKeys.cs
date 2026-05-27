namespace QimErp.Shared.Common.Workflow;

/// <summary>
/// Tenant-scoped Redis cache keys for workflow definitions and configuration.
/// </summary>
public static class WorkflowDefinitionCacheKeys
{
    private const string Prefix = "qface:qimerp:workflow:";

    public static string PublishedDefinition(string tenantId, string workflowCode, string entityType) =>
        $"{Prefix}definition:{tenantId}:{workflowCode}:{entityType}";

    public static string PublishedDefinitionByEntityType(string tenantId, string entityType) =>
        $"{Prefix}definition_by_entity:{tenantId}:{entityType}";

    public static string Configuration(string tenantId, string module, string entityType) =>
        $"{Prefix}config:{tenantId}:{module}:{entityType}";
}
