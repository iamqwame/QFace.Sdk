namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Read-only cache service for workflow configurations.
/// Used by interceptors to check workflow rules without direct database access.
/// All configuration data is read from Redis distributed cache.
/// </summary>
public interface IWorkflowConfigCacheService
{
    /// <summary>
    /// Gets the workflow configuration for a module and entity type from cache.
    /// </summary>
    Task<EntityWorkflowConfig?> GetEntityConfigAsync(string module, string entityType);

    /// <summary>
    /// Checks if workflow is enabled for a specific module, entity type and operation.
    /// </summary>
    Task<bool> IsWorkflowEnabledAsync(string module, string entityType, string operation);

    /// <summary>
    /// Gets the workflow code for a specific module, entity type and operation.
    /// </summary>
    Task<string?> GetWorkflowCodeAsync(string module, string entityType, string operation);

    /// <summary>
    /// Gets the trigger conditions for a specific module, entity type and operation.
    /// </summary>
    Task<List<WorkflowTriggerCondition>> GetTriggerConditionsAsync(string module, string entityType, string operation);
}
