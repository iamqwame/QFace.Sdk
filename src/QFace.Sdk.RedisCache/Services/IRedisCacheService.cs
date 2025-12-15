namespace QFace.Sdk.RedisCache.Services;

/// <summary>
/// Main interface for Redis caching operations
/// </summary>
public interface IRedisCacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    Task<T> GetAsync<T>(string key);
    
    /// <summary>
    /// Sets a value in cache
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// Removes a key from cache
    /// </summary>
    Task<bool> RemoveAsync(string key);
    
    /// <summary>
    /// Checks if a key exists
    /// </summary>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// Gets multiple values from cache
    /// </summary>
    Task<Dictionary<string, T>> GetManyAsync<T>(params string[] keys);
    
    /// <summary>
    /// Sets multiple values in cache
    /// </summary>
    Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null);
    
    /// <summary>
    /// Removes multiple keys from cache
    /// </summary>
    Task RemoveManyAsync(params string[] keys);
    
    /// <summary>
    /// Sets a value only if the key does not exist
    /// </summary>
    Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// Gets a value from cache, or sets it using a factory function if not found
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    
    /// <summary>
    /// Gets the time to live for a key
    /// </summary>
    Task<TimeSpan?> GetTimeToLiveAsync(string key);
    
    /// <summary>
    /// Extends the expiration time for a key
    /// </summary>
    Task<bool> ExtendExpirationAsync(string key, TimeSpan expiration);
    
    /// <summary>
    /// Gets a hash field value
    /// </summary>
    Task<T> HashGetAsync<T>(string key, string field);
    
    /// <summary>
    /// Sets a hash field value
    /// </summary>
    Task<bool> HashSetAsync<T>(string key, string field, T value);
    
    /// <summary>
    /// Gets all hash fields and values
    /// </summary>
    Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);
    
    /// <summary>
    /// Pushes a value to a list
    /// </summary>
    Task<long> ListPushAsync<T>(string key, T value);
    
    /// <summary>
    /// Pops a value from a list
    /// </summary>
    Task<T> ListPopAsync<T>(string key);
    
    /// <summary>
    /// Gets a range of values from a list
    /// </summary>
    Task<List<T>> ListGetRangeAsync<T>(string key, long start, long stop);
    
    /// <summary>
    /// Adds a value to a set
    /// </summary>
    Task<bool> SetAddAsync<T>(string key, T value);
    
    /// <summary>
    /// Checks if a value exists in a set
    /// </summary>
    Task<bool> SetContainsAsync<T>(string key, T value);
    
    /// <summary>
    /// Gets all values from a set
    /// </summary>
    Task<List<T>> SetGetAllAsync<T>(string key);
}

