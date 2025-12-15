using StackExchange.Redis;
using QFace.Sdk.RedisCache.Models;

namespace QFace.Sdk.RedisCache.Services.Providers;

/// <summary>
/// StackExchange.Redis provider implementation using TCP connection
/// </summary>
public class StackExchangeRedisProvider : IRedisProvider
{
    private readonly StackExchangeOptions _options;
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private readonly ILogger<StackExchangeRedisProvider> _logger;

    public StackExchangeRedisProvider(
        StackExchangeOptions options,
        ILogger<StackExchangeRedisProvider> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (string.IsNullOrEmpty(options.ConnectionString))
            throw new ArgumentException("ConnectionString is required", nameof(options));
        
        var configurationOptions = ConfigurationOptions.Parse(options.ConnectionString);
        configurationOptions.ConnectTimeout = options.ConnectTimeout;
        configurationOptions.SyncTimeout = options.SyncTimeout;
        configurationOptions.AbortOnConnectFail = options.AbortOnConnectFail;
        
        _connection = ConnectionMultiplexer.Connect(configurationOptions);
        _database = _connection.GetDatabase(options.Database);
        
        _logger.LogInformation("StackExchange.Redis connected to {ConnectionString}, Database: {Database}", 
            options.ConnectionString, options.Database);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (!value.HasValue)
            return default(T);
        
        return Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serialized = Serialize(value);
        await _database.StringSetAsync(key, serialized, expiration);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(params string[] keys)
    {
        if (keys == null || keys.Length == 0)
            return new Dictionary<string, T>();
        
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        var values = await _database.StringGetAsync(redisKeys);
        
        var dictionary = new Dictionary<string, T>();
        for (int i = 0; i < keys.Length; i++)
        {
            if (values[i].HasValue)
            {
                dictionary[keys[i]] = Deserialize<T>(values[i]);
            }
        }
        
        return dictionary;
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null)
    {
        if (items == null || items.Count == 0)
            return;
        
        var keyValuePairs = items.Select(kvp => 
            new KeyValuePair<RedisKey, RedisValue>(kvp.Key, Serialize(kvp.Value))).ToArray();
        
        await _database.StringSetAsync(keyValuePairs);
        
        if (expiration.HasValue)
        {
            var tasks = items.Keys.Select(key => _database.KeyExpireAsync(key, expiration.Value));
            await Task.WhenAll(tasks);
        }
    }

    public async Task RemoveManyAsync(params string[] keys)
    {
        if (keys == null || keys.Length == 0)
            return;
        
        var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
        await _database.KeyDeleteAsync(redisKeys);
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serialized = Serialize(value);
        return await _database.StringSetAsync(key, serialized, expiration, When.NotExists);
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        return await _database.KeyTimeToLiveAsync(key);
    }

    public async Task<bool> ExtendExpirationAsync(string key, TimeSpan expiration)
    {
        return await _database.KeyExpireAsync(key, expiration);
    }

    public async Task<T> HashGetAsync<T>(string key, string field)
    {
        var value = await _database.HashGetAsync(key, field);
        if (!value.HasValue)
            return default(T);
        
        return Deserialize<T>(value);
    }

    public async Task<bool> HashSetAsync<T>(string key, string field, T value)
    {
        var serialized = Serialize(value);
        return await _database.HashSetAsync(key, field, serialized);
    }

    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
    {
        var hash = await _database.HashGetAllAsync(key);
        if (hash == null || hash.Length == 0)
            return new Dictionary<string, T>();
        
        return hash.ToDictionary(
            entry => entry.Name.ToString(),
            entry => Deserialize<T>(entry.Value)
        );
    }

    public async Task<long> ListPushAsync<T>(string key, T value)
    {
        var serialized = Serialize(value);
        return await _database.ListLeftPushAsync(key, serialized);
    }

    public async Task<T> ListPopAsync<T>(string key)
    {
        var value = await _database.ListLeftPopAsync(key);
        if (!value.HasValue)
            return default(T);
        
        return Deserialize<T>(value);
    }

    public async Task<List<T>> ListGetRangeAsync<T>(string key, long start, long stop)
    {
        var values = await _database.ListRangeAsync(key, start, stop);
        if (values == null || values.Length == 0)
            return new List<T>();
        
        return values.Select(Deserialize<T>).ToList();
    }

    public async Task<bool> SetAddAsync<T>(string key, T value)
    {
        var serialized = Serialize(value);
        return await _database.SetAddAsync(key, serialized);
    }

    public async Task<bool> SetContainsAsync<T>(string key, T value)
    {
        var serialized = Serialize(value);
        return await _database.SetContainsAsync(key, serialized);
    }

    public async Task<List<T>> SetGetAllAsync<T>(string key)
    {
        var values = await _database.SetMembersAsync(key);
        if (values == null || values.Length == 0)
            return new List<T>();
        
        return values.Select(Deserialize<T>).ToList();
    }

    private string Serialize<T>(T value)
    {
        if (value == null)
            return null;
        
        if (value is string str)
            return str;
        
        return JsonConvert.SerializeObject(value);
    }

    private T Deserialize<T>(RedisValue value)
    {
        if (!value.HasValue || value.IsNullOrEmpty)
            return default(T);
        
        var stringValue = value.ToString();
        
        if (typeof(T) == typeof(string))
            return (T)(object)stringValue;
        
        try
        {
            return JsonConvert.DeserializeObject<T>(stringValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize value: {Value}", stringValue);
            return default(T);
        }
    }
}

