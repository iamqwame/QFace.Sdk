namespace QFace.Sdk.RedisCache.Services;

/// <summary>
/// Main implementation of Redis cache service
/// </summary>
public class RedisCacheService : IRedisCacheService
{
    private readonly Providers.IRedisProvider _provider;
    private readonly Models.RedisCacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        Providers.IRedisProvider provider,
        IOptions<Models.RedisCacheOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var fullKey = GetFullKey(key);
        return await _provider.GetAsync<T>(fullKey);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var fullKey = GetFullKey(key);
        var expiry = expiration ?? _options.DefaultExpiration;
        await _provider.SetAsync(fullKey, value, expiry);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        var fullKey = GetFullKey(key);
        return await _provider.RemoveAsync(fullKey);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var fullKey = GetFullKey(key);
        return await _provider.ExistsAsync(fullKey);
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(params string[] keys)
    {
        var fullKeys = keys.Select(GetFullKey).ToArray();
        return await _provider.GetManyAsync<T>(fullKeys);
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null)
    {
        var fullKeyItems = items.ToDictionary(
            kvp => GetFullKey(kvp.Key),
            kvp => kvp.Value
        );
        var expiry = expiration ?? _options.DefaultExpiration;
        await _provider.SetManyAsync(fullKeyItems, expiry);
    }

    public async Task RemoveManyAsync(params string[] keys)
    {
        var fullKeys = keys.Select(GetFullKey).ToArray();
        await _provider.RemoveManyAsync(fullKeys);
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var fullKey = GetFullKey(key);
        var expiry = expiration ?? _options.DefaultExpiration;
        return await _provider.SetIfNotExistsAsync(fullKey, value, expiry);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var fullKey = GetFullKey(key);
        var cached = await _provider.GetAsync<T>(fullKey);
        
        if (cached != null && !EqualityComparer<T>.Default.Equals(cached, default(T)))
        {
            return cached;
        }
        
        var value = await factory();
        if (value != null)
        {
            var expiry = expiration ?? _options.DefaultExpiration;
            await _provider.SetAsync(fullKey, value, expiry);
        }
        
        return value;
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        var fullKey = GetFullKey(key);
        return await _provider.GetTimeToLiveAsync(fullKey);
    }

    public async Task<bool> ExtendExpirationAsync(string key, TimeSpan expiration)
    {
        var fullKey = GetFullKey(key);
        return await _provider.ExtendExpirationAsync(fullKey, expiration);
    }

    public async Task<T> HashGetAsync<T>(string key, string field)
    {
        var fullKey = GetFullKey(key);
        return await _provider.HashGetAsync<T>(fullKey, field);
    }

    public async Task<bool> HashSetAsync<T>(string key, string field, T value)
    {
        var fullKey = GetFullKey(key);
        return await _provider.HashSetAsync(fullKey, field, value);
    }

    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
    {
        var fullKey = GetFullKey(key);
        return await _provider.HashGetAllAsync<T>(fullKey);
    }

    public async Task<long> ListPushAsync<T>(string key, T value)
    {
        var fullKey = GetFullKey(key);
        return await _provider.ListPushAsync(fullKey, value);
    }

    public async Task<T> ListPopAsync<T>(string key)
    {
        var fullKey = GetFullKey(key);
        return await _provider.ListPopAsync<T>(fullKey);
    }

    public async Task<List<T>> ListGetRangeAsync<T>(string key, long start, long stop)
    {
        var fullKey = GetFullKey(key);
        return await _provider.ListGetRangeAsync<T>(fullKey, start, stop);
    }

    public async Task<bool> SetAddAsync<T>(string key, T value)
    {
        var fullKey = GetFullKey(key);
        return await _provider.SetAddAsync(fullKey, value);
    }

    public async Task<bool> SetContainsAsync<T>(string key, T value)
    {
        var fullKey = GetFullKey(key);
        return await _provider.SetContainsAsync(fullKey, value);
    }

    public async Task<List<T>> SetGetAllAsync<T>(string key)
    {
        var fullKey = GetFullKey(key);
        return await _provider.SetGetAllAsync<T>(fullKey);
    }

    private string GetFullKey(string key)
    {
        if (string.IsNullOrEmpty(_options.KeyPrefix))
            return key;
        
        return $"{_options.KeyPrefix}{key}";
    }
}

