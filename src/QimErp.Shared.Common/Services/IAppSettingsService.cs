namespace QimErp.Shared.Common.Services;

public interface IAppSettingsService
{
    Task<T?> GetSettingAsync<T>(string key, T? defaultValue = default);
    Task<string?> GetStringSettingAsync(string key, string? defaultValue = null);
    Task<string[]?> GetArraySettingAsync(string key, string[]? defaultValue = null);
    Task<Dictionary<string, object>?> GetObjectSettingAsync(string key, Dictionary<string, object>? defaultValue = null);
    Task<bool> GetBooleanSettingAsync(string key, bool defaultValue = false);
    Task<int> GetIntSettingAsync(string key, int defaultValue = 0);
    Task<decimal> GetDecimalSettingAsync(string key, decimal defaultValue = 0);
    
    Task SetSettingAsync<T>(string key, T value, string category, string description);
    Task SetStringSettingAsync(string key, string value, string category, string description);
    Task SetArraySettingAsync(string key, string[] values, string category, string description);
    Task SetObjectSettingAsync(string key, object value, string category, string description);
    Task SetBooleanSettingAsync(string key, bool value, string category, string description);
    
    /// <summary>
    /// Bulk insert/update settings for a tenant. More efficient than calling SetSettingAsync individually.
    /// </summary>
    Task BulkSetSettingsAsync(Dictionary<string, object> settings, string category, string description = "");
    
    Task<List<AppSetting>> GetAllSettingsAsync(string? category = null);
    Task<AppSetting?> GetSettingEntityAsync(string key);
    Task DeleteSettingAsync(string key);
    /// <summary>Resolves with the tenant-default fallback — true when a value is readable, not when a row is owned.</summary>
    Task<bool> SettingExistsAsync(string key);

    /// <summary>Exact match on the current company's write target. Use this before creating an override.</summary>
    Task<bool> SettingExistsForCurrentCompanyAsync(string key);
} 