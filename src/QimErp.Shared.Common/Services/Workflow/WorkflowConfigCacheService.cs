using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Read-only cache service for workflow configurations.
/// Retrieves configuration data from Redis distributed cache only.
/// </summary>
public class WorkflowConfigCacheService : IWorkflowConfigCacheService
{
    private readonly IDistributedCacheService _cache;
    private readonly ILogger<WorkflowConfigCacheService> _logger;

    public WorkflowConfigCacheService(
        IDistributedCacheService cache,
        ILogger<WorkflowConfigCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<EntityWorkflowConfig?> GetEntityConfigAsync(string module, string entityType)
    {
        try
        {
            var cacheKey = AppConstant.Cache.Keys.WorkflowConfiguration(module, entityType);
            var config = await _cache.GetAsync<EntityWorkflowConfig>(cacheKey);

            if (config == null)
            {
                _logger.LogDebug("No workflow configuration found in cache for module: {Module}, entity type: {EntityType}", module, entityType);
            }
            else
            {
                _logger.LogDebug("Retrieved workflow configuration from cache for module: {Module}, entity type: {EntityType}", module, entityType);
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow configuration from cache for module: {Module}, entity type: {EntityType}", module, entityType);
            return null;
        }
    }

    public async Task<bool> IsWorkflowEnabledAsync(string module, string entityType, string operation)
    {
        var config = await GetEntityConfigAsync(module, entityType);
        if (config == null) return false;

        return operation.ToUpper() switch
        {
            "CREATE" => config.EnableWorkflowForCreate,
            "UPDATE" => config.EnableWorkflowForUpdate,
            "DELETE" => config.EnableWorkflowForDelete,
            _ => false
        };
    }

    public async Task<string?> GetWorkflowCodeAsync(string module, string entityType, string operation)
    {
        var config = await GetEntityConfigAsync(module, entityType);
        if (config == null) return null;

        return operation.ToUpper(CultureInfo.CurrentCulture) switch
        {
            "CREATE" => config.CreateWorkflowCode,
            "UPDATE" => config.UpdateWorkflowCode,
            "DELETE" => config.DeleteWorkflowCode,
            _ => null
        };
    }

    public async Task<List<WorkflowTriggerCondition>> GetTriggerConditionsAsync(string module, string entityType, string operation)
    {
        var config = await GetEntityConfigAsync(module, entityType);
        if (config == null) return [];

        return operation.ToUpper(CultureInfo.CurrentCulture) switch
        {
            "CREATE" => config.CreateTriggerConditions,
            "UPDATE" => config.UpdateTriggerConditions,
            "DELETE" => config.DeleteTriggerConditions,
            _ => []
        };
    }
}
