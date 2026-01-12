using Microsoft.Extensions.Caching.Memory;
using QimErp.Shared.Common.Database;

namespace QimErp.Shared.Common.Services;

public abstract class AppSettingsService<TContext> : IAppSettingsService
    where TContext : ApplicationDbContext<TContext>
{
    protected readonly TContext _context;
    private readonly ILogger<AppSettingsService<TContext>> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "app_setting_";
    private const int CacheExpirationMinutes = 30;

    protected AppSettingsService(TContext context, ILogger<AppSettingsService<TContext>> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    protected abstract DbSet<AppSetting> AppSettings { get; }

    public async Task<T?> GetSettingAsync<T>(string key, T? defaultValue = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";
        
        if (_cache.TryGetValue(cacheKey, out T? cachedValue))
        {
            return cachedValue;
        }

        try
        {
            // Ensure database is ready
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning default value for setting {Key}", key);
                return defaultValue;
            }

            var setting = await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                return defaultValue;
            }

            var value = setting.GetValue<T>();
            _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheExpirationMinutes));
            
            return value ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setting {Key}. This might be due to database schema issues.", key);
            return defaultValue;
        }
    }

    public async Task<string?> GetStringSettingAsync(string key, string? defaultValue = null)
    {
        return await GetSettingAsync(key, defaultValue);
    }

    public async Task<string[]?> GetArraySettingAsync(string key, string[]? defaultValue = null)
    {
        return await GetSettingAsync<string[]>(key, defaultValue);
    }

    public async Task<Dictionary<string, object>?> GetObjectSettingAsync(string key, Dictionary<string, object>? defaultValue = null)
    {
        return await GetSettingAsync<Dictionary<string, object>>(key, defaultValue);
    }

    public async Task<bool> GetBooleanSettingAsync(string key, bool defaultValue = false)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning default value for setting {Key}", key);
                return defaultValue;
            }

            var setting = await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            return setting?.GetBooleanValue() ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get boolean setting {Key}. This might be due to database schema issues.", key);
            return defaultValue;
        }
    }

    public async Task<int> GetIntSettingAsync(string key, int defaultValue = 0)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning default value for setting {Key}", key);
                return defaultValue;
            }

            var setting = await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            return setting?.GetIntValue() ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get int setting {Key}. This might be due to database schema issues.", key);
            return defaultValue;
        }
    }

    public async Task<decimal> GetDecimalSettingAsync(string key, decimal defaultValue = 0)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning default value for setting {Key}", key);
                return defaultValue;
            }

            var setting = await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            return setting?.GetDecimalValue() ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get decimal setting {Key}. This might be due to database schema issues.", key);
            return defaultValue;
        }
    }

    public async Task SetSettingAsync<T>(string key, T value, string category, string description)
    {
        try
        {
            // Ensure database is ready
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, skipping setting {Key}", key);
                return;
            }

            var existingSetting = await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (existingSetting != null)
            {
                if (value is string stringValue)
                    existingSetting.UpdateValue(stringValue);
                else if (value is string[] arrayValue)
                    existingSetting.UpdateArrayValue(arrayValue);
                else
                    existingSetting.UpdateObjectValue(value!);
            }
            else
            {
                AppSetting newSetting;
                if (value is string stringValue)
                    newSetting = AppSetting.Create(key, stringValue, category);
                else if (value is string[] arrayValue)
                    newSetting = AppSetting.CreateArray(key, arrayValue, category);
                else
                    newSetting = AppSetting.CreateObject(key, value!, category);

                newSetting.WithDescription(description);

                await AppSettings.AddAsync(newSetting);
            }

            await _context.SaveChangesAsync();
            _cache.Remove($"{CacheKeyPrefix}{key}");
            
            _logger.LogInformation("Setting {Key} updated", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set setting {Key}. This might be due to database schema issues.", key);
            // Don't throw the exception to prevent application startup failure
        }
    }

    public async Task SetStringSettingAsync(string key, string value, string category, string description)
    {
        await SetSettingAsync(key, value, category, description);
    }

    public async Task SetArraySettingAsync(string key, string[] values, string category, string description)
    {
        await SetSettingAsync(key, values, category, description);
    }

    public async Task SetObjectSettingAsync(string key, object value, string category, string description)
    {
        await SetSettingAsync(key, value, category, description);
    }

    public async Task SetBooleanSettingAsync(string key, bool value, string category, string description)
    {
        await SetSettingAsync(key, value, category, description);
    }

    public async Task BulkSetSettingsAsync(Dictionary<string, object> settings, string category, string description = "")
    {
        if (settings == null || settings.Count == 0)
            return;

        try
        {
            // Ensure database is ready
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, skipping bulk settings insert");
                return;
            }

            // ✅ Performance Fix 1: Use proper SQL translation with ToList() and create lookup dictionary
            var settingKeys = settings.Keys.ToList();
            var existingSettingsDict = (await AppSettings
                    .Where(s => settingKeys.Contains(s.Key))
                    .ToListAsync())
                .ToDictionary(s => s.Key, s => s);

            // ✅ Performance Fix 2: Efficient O(1) dictionary lookup instead of O(n) FirstOrDefault in loop
            foreach (var (key, value) in settings)
            {
                if (existingSettingsDict.TryGetValue(key, out var existingSetting))
                {
                    // Update existing setting
                    if (value is string stringValue)
                        existingSetting.UpdateValue(stringValue);
                    else if (value is string[] arrayValue)
                        existingSetting.UpdateArrayValue(arrayValue);
                    else if (value is Dictionary<string, object> dictValue)
                        existingSetting.UpdateObjectValue(dictValue);
                    else
                        existingSetting.UpdateObjectValue(value);
                    
                    if (!string.IsNullOrEmpty(description))
                        existingSetting.WithDescription(description);
                }
                else
                {
                    // Create new setting
                    AppSetting newSetting;
                    if (value is string stringVal)
                        newSetting = AppSetting.Create(key, stringVal, category);
                    else if (value is string[] arrayVal)
                        newSetting = AppSetting.CreateArray(key, arrayVal, category);
                    else if (value is Dictionary<string, object> dictVal)
                        newSetting = AppSetting.CreateObject(key, dictVal, category);
                    else
                        newSetting = AppSetting.CreateObject(key, value, category);

                    if (!string.IsNullOrEmpty(description))
                        newSetting.WithDescription(description);

                    await AppSettings.AddAsync(newSetting);
                }
            }

            await _context.SaveChangesAsync();

            // Clear cache for all affected settings
            foreach (var key in settingKeys)
            {
                _cache.Remove($"{CacheKeyPrefix}{key}");
            }

            _logger.LogInformation("Bulk inserted/updated {Count} settings in category {Category}", settings.Count, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk set settings. This might be due to database schema issues.");
            // Don't throw the exception to prevent application startup failure
        }
    }

    public async Task<List<AppSetting>> GetAllSettingsAsync(string? category = null)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning empty list for settings");
                return new List<AppSetting>();
            }

            var query = AppSettings.AsQueryable();
            
            if (!category.IsEmpty())
                query = query.Where(s => s.Category == category);

            return await query.OrderBy(s => s.Category).ThenBy(s => s.Key).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all settings. This might be due to database schema issues.");
            return new List<AppSetting>();
        }
    }

    public async Task<AppSetting?> GetSettingEntityAsync(string key)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning null for setting {Key}", key);
                return null;
            }

            return await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setting entity {Key}. This might be due to database schema issues.", key);
            return null;
        }
    }

    public async Task DeleteSettingAsync(string key)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, skipping delete for setting {Key}", key);
                return;
            }

            var setting = await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting != null)
            {
                setting.MarkAsDeleted();
                await _context.SaveChangesAsync();
                _cache.Remove($"{CacheKeyPrefix}{key}");
                
                _logger.LogInformation("Setting {Key} deleted", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete setting {Key}. This might be due to database schema issues.", key);
        }
    }

    public async Task<bool> SettingExistsAsync(string key)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning false for setting {Key}", key);
                return false;
            }

            return await AppSettings
                .AnyAsync(s => s.Key == key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if setting {Key} exists. This might be due to database schema issues.", key);
            return false;
        }
    }
} 