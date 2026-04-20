using System.Collections.Concurrent;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Read-only cache service for workflow configurations.
/// Retrieves configuration data from Redis distributed cache only.
/// Uses an in-process L1 dictionary so that each module+entity combination
/// hits Redis at most once per process lifetime — critical for bulk seeding
/// operations that create 100 000+ entities and would otherwise hammer Redis
/// with a lookup for the same static config key on every entity construction.
/// </summary>
public class WorkflowConfigCacheService : IWorkflowConfigCacheService
{
    private readonly IDistributedCacheService _cache;
    private readonly ILogger<WorkflowConfigCacheService> _logger;

    /// <summary>
    /// In-process L1 cache keyed by the full Redis key.
    /// Workflow configuration is static for the process lifetime —
    /// it only changes on a re-deploy, at which point the process restarts.
    /// Using <see langword="null"/> as a sentinel for "no config / workflow disabled"
    /// so cache misses are also remembered and Redis is never called twice.
    /// </summary>
    private readonly ConcurrentDictionary<string, EntityWorkflowConfig?> _localCache = new();

    public WorkflowConfigCacheService(
        IDistributedCacheService cache,
        ILogger<WorkflowConfigCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<EntityWorkflowConfig?> GetEntityConfigAsync(string module, string entityType)
    {
        var cacheKey = $"qface:qimerp:workflow:config_{module}_{entityType}";

        // L1: in-process hit — zero Redis round-trips after the first call per entity type.
        if (_localCache.TryGetValue(cacheKey, out var cached))
        {
            _logger.LogDebug("L1 hit for workflow config key: {Key}", cacheKey);
            return cached;
        }

        try
        {
            var config = await _cache.GetAsync<EntityWorkflowConfig>(cacheKey);

            // Store in L1 regardless of null/non-null.
            // null means "no workflow configured" — also worth remembering so we don't retry Redis.
            _localCache[cacheKey] = config;

            if (config == null)
                _logger.LogDebug("No workflow configuration found in cache for module: {Module}, entity type: {EntityType}", module, entityType);
            else
                _logger.LogDebug("Retrieved workflow configuration from cache for module: {Module}, entity type: {EntityType}", module, entityType);

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
