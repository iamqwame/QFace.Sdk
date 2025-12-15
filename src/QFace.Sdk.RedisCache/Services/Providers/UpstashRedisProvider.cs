using System.Net.Http.Headers;
using System.Text;
using QFace.Sdk.RedisCache.Models;

namespace QFace.Sdk.RedisCache.Services.Providers;

/// <summary>
/// Upstash Redis provider implementation using HTTP/REST API
/// </summary>
public class UpstashRedisProvider : IRedisProvider
{
    private readonly UpstashOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UpstashRedisProvider> _logger;

    public UpstashRedisProvider(
        UpstashOptions options,
        IHttpClientFactory httpClientFactory,
        ILogger<UpstashRedisProvider> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (string.IsNullOrEmpty(options.Url))
            throw new ArgumentException("Upstash URL is required", nameof(options));
        if (string.IsNullOrEmpty(options.Token))
            throw new ArgumentException("Upstash Token is required", nameof(options));
        
        _httpClient = httpClientFactory.CreateClient("UpstashRedis");
        _httpClient.BaseAddress = new Uri(options.Url);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var result = await ExecuteCommandAsync<string>("GET", new[] { key });
        if (string.IsNullOrEmpty(result))
            return default(T);
        
        return Deserialize<T>(result!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serialized = Serialize(value);
        var args = new List<string> { key, serialized };
        
        if (expiration.HasValue)
        {
            args.Add("EX");
            args.Add(((int)expiration.Value.TotalSeconds).ToString());
        }
        
        await ExecuteCommandAsync<string>("SET", args.ToArray());
    }

    public async Task<bool> RemoveAsync(string key)
    {
        var result = await ExecuteCommandAsync<long>("DEL", new[] { key });
        return result > 0;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var result = await ExecuteCommandAsync<long>("EXISTS", new[] { key });
        return result > 0;
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(params string[] keys)
    {
        if (keys == null || keys.Length == 0)
            return new Dictionary<string, T>();
        
        var result = await ExecuteCommandAsync<string[]>("MGET", keys);
        var dictionary = new Dictionary<string, T>();
        
        for (int i = 0; i < keys.Length; i++)
        {
            if (result != null && i < result.Length && !string.IsNullOrEmpty(result[i]))
            {
                dictionary[keys[i]] = Deserialize<T>(result[i]);
            }
        }
        
        return dictionary;
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null)
    {
        if (items == null || items.Count == 0)
            return;
        
        // Use pipeline for better performance
        var tasks = items.Select(item => SetAsync(item.Key, item.Value, expiration));
        await Task.WhenAll(tasks);
    }

    public async Task RemoveManyAsync(params string[] keys)
    {
        if (keys == null || keys.Length == 0)
            return;
        
        await ExecuteCommandAsync<long>("DEL", keys);
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serialized = Serialize(value);
        var args = new List<string> { key, serialized };
        
        if (expiration.HasValue)
        {
            args.Add("EX");
            args.Add(((int)expiration.Value.TotalSeconds).ToString());
        }
        
        var result = await ExecuteCommandAsync<long>("SETNX", args.ToArray());
        return result > 0;
    }

    public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
    {
        var result = await ExecuteCommandAsync<long>("TTL", new[] { key });
        if (result < 0)
            return null; // Key doesn't exist or has no expiration
        
        return TimeSpan.FromSeconds(result);
    }

    public async Task<bool> ExtendExpirationAsync(string key, TimeSpan expiration)
    {
        var result = await ExecuteCommandAsync<long>("EXPIRE", new[] { key, ((int)expiration.TotalSeconds).ToString() });
        return result > 0;
    }

    public async Task<T> HashGetAsync<T>(string key, string field)
    {
        var result = await ExecuteCommandAsync<string>("HGET", new[] { key, field });
        if (string.IsNullOrEmpty(result))
            return default(T);
        
        return Deserialize<T>(result);
    }

    public async Task<bool> HashSetAsync<T>(string key, string field, T value)
    {
        var serialized = Serialize(value);
        var result = await ExecuteCommandAsync<long>("HSET", new[] { key, field, serialized });
        return result >= 0;
    }

    public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
    {
        var result = await ExecuteCommandAsync<string[]>("HGETALL", new[] { key });
        if (result == null || result.Length == 0)
            return new Dictionary<string, T>();
        
        var dictionary = new Dictionary<string, T>();
        for (int i = 0; i < result.Length; i += 2)
        {
            if (i + 1 < result.Length)
            {
                dictionary[result[i]] = Deserialize<T>(result[i + 1]);
            }
        }
        
        return dictionary;
    }

    public async Task<long> ListPushAsync<T>(string key, T value)
    {
        var serialized = Serialize(value);
        return await ExecuteCommandAsync<long>("LPUSH", new[] { key, serialized });
    }

    public async Task<T> ListPopAsync<T>(string key)
    {
        var result = await ExecuteCommandAsync<string>("LPOP", new[] { key });
        if (string.IsNullOrEmpty(result))
            return default(T);
        
        return Deserialize<T>(result);
    }

    public async Task<List<T>> ListGetRangeAsync<T>(string key, long start, long stop)
    {
        var result = await ExecuteCommandAsync<string[]>("LRANGE", new[] { key, start.ToString(), stop.ToString() });
        if (result == null || result.Length == 0)
            return new List<T>();
        
        return result.Select(Deserialize<T>).ToList();
    }

    public async Task<bool> SetAddAsync<T>(string key, T value)
    {
        var serialized = Serialize(value);
        var result = await ExecuteCommandAsync<long>("SADD", new[] { key, serialized });
        return result > 0;
    }

    public async Task<bool> SetContainsAsync<T>(string key, T value)
    {
        var serialized = Serialize(value);
        var result = await ExecuteCommandAsync<long>("SISMEMBER", new[] { key, serialized });
        return result > 0;
    }

    public async Task<List<T>> SetGetAllAsync<T>(string key)
    {
        var result = await ExecuteCommandAsync<string[]>("SMEMBERS", new[] { key });
        if (result == null || result.Length == 0)
            return new List<T>();
        
        return result.Select(Deserialize<T>).ToList();
    }

    private async Task<TResult> ExecuteCommandAsync<TResult>(string command, string[] args)
    {
        int attempt = 0;
        Exception lastException = null;
        
        while (attempt <= _options.RetryCount)
        {
            try
            {
                // Upstash REST API expects an array format: ["COMMAND", "arg1", "arg2", ...]
                var commandArray = new List<object> { command };
                if (args != null && args.Length > 0)
                {
                    commandArray.AddRange(args);
                }
                
                var json = JsonConvert.SerializeObject(commandArray);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogDebug("Upstash Redis request: {Command} with args: {Args}", command, string.Join(", ", args ?? Array.Empty<string>()));
                
                var response = await _httpClient.PostAsync("", content);
                
                var responseJson = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Upstash Redis API error: Status {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseJson);
                    throw new HttpRequestException($"Upstash Redis API returned {response.StatusCode}: {responseJson}");
                }
                
                // Upstash REST API always returns JSON with either "result" or "error" field
                // Format: { "result": <value> } or { "error": "<error message>" }
                try
                {
                    var responseObj = JsonConvert.DeserializeObject<UpstashResponse<TResult>>(responseJson);
                    
                    if (responseObj == null)
                    {
                        _logger.LogWarning("Upstash returned null response: {Response}", responseJson);
                        return default(TResult);
                    }
                    
                    // Check for error response
                    if (responseObj.error != null)
                    {
                        _logger.LogError("Upstash Redis API error: {Error}", responseObj.error);
                        throw new InvalidOperationException($"Upstash Redis API error: {responseObj.error}");
                    }
                    
                    // Return the result
                    if (responseObj.result != null)
                    {
                        // For value types, check if it's the default value
                        if (typeof(TResult).IsValueType)
                        {
                            if (!EqualityComparer<TResult>.Default.Equals(responseObj.result, default(TResult)))
                            {
                                return responseObj.result;
                            }
                        }
                        else
                        {
                            // For reference types, check for null
                            return responseObj.result;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize Upstash response: {Response}", responseJson);
                    throw;
                }
                
                return default(TResult);
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;
                
                if (attempt <= _options.RetryCount)
                {
                    var delay = (int)(_options.RetryBaseDelayMs * Math.Pow(2, attempt - 1));
                    _logger.LogWarning(ex, 
                        "Upstash Redis command failed (attempt {Attempt}/{MaxRetries}): {Command}. Retrying in {Delay}ms...",
                        attempt, _options.RetryCount + 1, command, delay);
                    
                    await Task.Delay(delay);
                }
            }
        }
        
        _logger.LogError(lastException, "Upstash Redis command failed after {Retries} attempts: {Command}", 
            _options.RetryCount + 1, command);
        throw new InvalidOperationException($"Failed to execute Redis command '{command}' after {_options.RetryCount + 1} attempts", lastException);
    }

    private string Serialize<T>(T value)
    {
        if (value == null)
            return null;
        
        if (value is string str)
            return str;
        
        return JsonConvert.SerializeObject(value);
    }

    private T Deserialize<T>(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default(T);
        
        if (typeof(T) == typeof(string))
            return (T)(object)value;
        
        try
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize value: {Value}", value);
            return default(T);
        }
    }

    private class UpstashResponse<T>
    {
        public T? result { get; set; }
        public string? error { get; set; }
    }
}

