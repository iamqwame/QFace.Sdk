using QFace.Sdk.RedisCache.Services;

namespace QimErp.Shared.Common.Services.Cache;

public class RedisCacheService(
    IRedisCacheService redisCacheService, 
    ILogger<RedisCacheService> logger,
    IConfiguration configuration)
    : IDistributedCacheService
{
    private readonly bool _cacheEnabled = GetCacheEnabled(configuration);
    
    private static bool GetCacheEnabled(IConfiguration configuration)
    {
        // Read Enabled from RedisCache section (defaults to true if not specified)
        var enabled = configuration.GetValue<bool>("RedisCache:Enabled", true);
        return enabled;
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        return await GetAsync<T>(key, null);
    }

    public async Task<T?> GetAsync<T>(string key, string? region = null)
    {
        if (!_cacheEnabled)
        {
            logger.LogDebug("Cache disabled - skipping GetAsync for key: {Key}", key);
            return default;
        }

        try
        {
            var fullKey = GetFullKey(key, region);
            var cachedValue = await redisCacheService.GetAsync<T>(fullKey);
            
            // SDK returns Task<T> which may be default(T) if not found
            // Check if it's actually a cache hit by using ExistsAsync
            if (EqualityComparer<T>.Default.Equals(cachedValue, default(T)))
            {
                // Double-check with ExistsAsync to distinguish between cache miss and null value
                var exists = await redisCacheService.ExistsAsync(fullKey);
                if (!exists)
                {
                    logger.LogDebug("Cache miss for key: {Key}", fullKey);
                    return default;
                }
            }

            logger.LogDebug("Cache hit for key: {Key}", fullKey);
            return cachedValue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        await SetAsync(key, value, expiration, null);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? region = null)
    {
        if (!_cacheEnabled)
        {
            logger.LogDebug("Cache disabled - skipping SetAsync for key: {Key}", key);
            return;
        }

        try
        {
            var fullKey = GetFullKey(key, region);
            // Enforce TTL - if no expiration provided, use default
            var ttl = expiration ?? TimeSpan.FromMinutes(15); // Default 15 minutes
            
            await redisCacheService.SetAsync(fullKey, value, ttl);
            logger.LogDebug("Cached value for key: {Key} with TTL: {Ttl}", fullKey, ttl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        await RemoveAsync(key, null);
    }

    public async Task RemoveAsync(string key, string? region = null)
    {
        if (!_cacheEnabled)
        {
            logger.LogDebug("Cache disabled - skipping RemoveAsync for key: {Key}", key);
            return;
        }

        try
        {
            var fullKey = GetFullKey(key, region);
            await redisCacheService.RemoveAsync(fullKey);
            logger.LogDebug("Removed cache for key: {Key}", fullKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        await RemoveByPatternAsync(pattern, null);
    }

    public async Task RemoveByPatternAsync(string pattern, string? region = null)
    {
        if (!_cacheEnabled)
        {
            logger.LogDebug("Cache disabled - skipping RemoveByPatternAsync for pattern: {Pattern}", pattern);
            return;
        }

        try
        {
            // Note: Pattern-based deletion is not directly supported by the SDK
            // This would require a custom implementation using Redis commands
            // For now, we'll log this as a limitation
            logger.LogWarning("Pattern-based cache removal not implemented for Redis. Pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await ExistsAsync(key, null);
    }

    public async Task<bool> ExistsAsync(string key, string? region = null)
    {
        if (!_cacheEnabled)
        {
            logger.LogDebug("Cache disabled - ExistsAsync returning false for key: {Key}", key);
            return false;
        }

        try
        {
            var fullKey = GetFullKey(key, region);
            return await redisCacheService.ExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        return await GetOrSetAsync(key, factory, expiration, null);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, string? region = null)
    {
        if (!_cacheEnabled)
        {
            logger.LogDebug("Cache disabled - GetOrSetAsync calling factory directly for key: {Key}", key);
            return await factory();
        }

        try
        {
            var fullKey = GetFullKey(key, region);
            // SDK's GetOrSetAsync handles null values correctly
            var ttl = expiration ?? TimeSpan.FromMinutes(15); // Default 15 minutes
            return await redisCacheService.GetOrSetAsync(fullKey, factory, ttl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}, calling factory directly", key);
            return await factory();
        }
    }

    private static string GetFullKey(string key, string? region)
    {
        return string.IsNullOrEmpty(region) ? key : $"{region}:{key}";
    }
}
