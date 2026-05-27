namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Read-only cache service for workflow configurations.
/// Used by interceptors to check workflow rules without direct database access.
/// All configuration data is read from Redis distributed cache.
/// </summary>
public interface IWorkflowConfigCacheService
{
    Task<EntityWorkflowConfig?> GetEntityConfigAsync(string module, string entityType, string? tenantId = null);

    Task<bool> IsWorkflowEnabledAsync(string module, string entityType, string operation, string? tenantId = null);

    Task<string?> GetWorkflowCodeAsync(string module, string entityType, string operation, string? tenantId = null);

    Task<List<WorkflowTriggerCondition>> GetTriggerConditionsAsync(
        string module,
        string entityType,
        string operation,
        string? tenantId = null);
}
