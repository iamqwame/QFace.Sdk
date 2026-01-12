namespace QimErp.Shared.Common.Services.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public interface IDistributedCacheService : ICacheService
{
    Task<T?> GetAsync<T>(string key, string? region = null);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? region = null);
    Task RemoveAsync(string key, string? region = null);
    Task RemoveByPatternAsync(string pattern, string? region = null);
    Task<bool> ExistsAsync(string key, string? region = null);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, string? region = null);
}
