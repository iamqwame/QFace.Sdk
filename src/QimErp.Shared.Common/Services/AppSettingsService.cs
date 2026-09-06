using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Services.MultiTenancy;

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

    private static string EffectiveCompanyId => CompanyContext.CurrentScope.EffectiveCompanyId;

    // IMemoryCache is per-process, so generation-based eviction is best-effort across instances.
    private static readonly ConcurrentDictionary<string, long> TenantCacheGenerations = new();

    // IMemoryCache is a process-wide singleton shared by every tenant and company, so the key must
    // include both ids — otherwise one company's cached value is served to another's read/write.
    private string CacheKey(string key) =>
        $"{CacheKeyPrefix}{_context.CurrentTenantId}_g{TenantCacheGenerations.GetOrAdd(_context.CurrentTenantId ?? string.Empty, 0)}_{EffectiveCompanyId}_{key}";

    private void EvictTenantWide() =>
        TenantCacheGenerations.AddOrUpdate(_context.CurrentTenantId ?? string.Empty, 1, (_, current) => current + 1);

    private IQueryable<AppSetting> Candidates(string companyId) =>
        AppSettings.Where(s => s.CompanyId == string.Empty || s.CompanyId == companyId);

    // Resolve against EffectiveCompanyId, never the ambient read scope: AllowedCompanyIds admits every
    // company the caller can see, so the global filter alone would return an arbitrary company's row.
    // A TenantOnly shared row wins over any company row, including one written before the key was marked.
    private static AppSetting? Resolve(IEnumerable<AppSetting> rows, string companyId)
    {
        var shared = rows.FirstOrDefault(s => s.CompanyId.Length == 0);

        if (companyId.Length == 0 || shared?.Scope == AppSettingScope.TenantOnly)
            return shared;

        return rows.FirstOrDefault(s => s.CompanyId == companyId) ?? shared;
    }

    public async Task<T?> GetSettingAsync<T>(string key, T? defaultValue = default)
    {
        var cacheKey = CacheKey(key);

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

            var companyId = EffectiveCompanyId;
            var setting = Resolve(await Candidates(companyId).Where(s => s.Key == key).ToListAsync(), companyId);

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
        var cacheKey = CacheKey(key);
        if (_cache.TryGetValue(cacheKey, out bool cachedValue))
        {
            return cachedValue;
        }

        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning default value for setting {Key}", key);
                return defaultValue;
            }

            var companyId = EffectiveCompanyId;
            var setting = Resolve(await Candidates(companyId).Where(s => s.Key == key).ToListAsync(), companyId);

            var value = setting?.GetBooleanValue() ?? defaultValue;
            _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get boolean setting {Key}. This might be due to database schema issues.", key);
            return defaultValue;
        }
    }

    public async Task<int> GetIntSettingAsync(string key, int defaultValue = 0)
    {
        var cacheKey = CacheKey(key);
        if (_cache.TryGetValue(cacheKey, out int cachedValue))
        {
            return cachedValue;
        }

        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning default value for setting {Key}", key);
                return defaultValue;
            }

            var companyId = EffectiveCompanyId;
            var setting = Resolve(await Candidates(companyId).Where(s => s.Key == key).ToListAsync(), companyId);

            var value = setting?.GetIntValue() ?? defaultValue;
            _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get int setting {Key}. This might be due to database schema issues.", key);
            return defaultValue;
        }
    }

    public async Task<decimal> GetDecimalSettingAsync(string key, decimal defaultValue = 0)
    {
        var cacheKey = CacheKey(key);
        if (_cache.TryGetValue(cacheKey, out decimal cachedValue))
        {
            return cachedValue;
        }

        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning default value for setting {Key}", key);
                return defaultValue;
            }

            var companyId = EffectiveCompanyId;
            var setting = Resolve(await Candidates(companyId).Where(s => s.Key == key).ToListAsync(), companyId);

            var value = setting?.GetDecimalValue() ?? defaultValue;
            _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get decimal setting {Key}. This might be due to database schema issues.", key);
            return defaultValue;
        }
    }

    public async Task SetSettingAsync<T>(string key, T value, string category, string description)
    {
        var companyId = EffectiveCompanyId;

        if (companyId.Length > 0)
        {
            var authoritative = Resolve(await Candidates(companyId).Where(s => s.Key == key).ToListAsync(), companyId: string.Empty);
            if (authoritative?.Scope == AppSettingScope.TenantOnly)
                throw new AppSettingScopeViolationException(
                    $"Setting '{key}' is tenant-only and cannot be overridden per company.");
        }

        try
        {
            // Ensure database is ready
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, skipping setting {Key}", key);
                return;
            }

            var existingSetting = await AppSettings
                .FirstOrDefaultAsync(s => s.Key == key && s.CompanyId == companyId);

            if (existingSetting != null)
            {
                if (value is string stringValue)
                    existingSetting.UpdateValue(stringValue);
                else if (value is string[] arrayValue)
                    existingSetting.UpdateArrayValue(arrayValue);
                else
                    existingSetting.UpdateObjectValue(value!);

                await _context.SaveChangesAsync();
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

                if (companyId.Length > 0)
                {
                    newSetting.WithCompanyId(companyId);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    using (CompanyStampScope.EnterSharedAsTenantWideWriter($"Setting '{key}'"))
                        await _context.SaveChangesAsync();
                }
            }

            _cache.Remove(CacheKey(key));

            if (companyId.Length == 0)
                EvictTenantWide();

            _logger.LogInformation("Setting {Key} updated", key);
        }
        catch (Exception ex) when (ex is not CrossCompanyWriteException
                                       and not AppSettingScopeViolationException
                                       and not DbUpdateException)
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

        var companyId = EffectiveCompanyId;
        var settingKeys = settings.Keys.ToList();

        if (companyId.Length > 0)
        {
            var candidateRows = await Candidates(companyId)
                .Where(s => settingKeys.Contains(s.Key))
                .ToListAsync();

            var violatingKeys = candidateRows
                .GroupBy(s => s.Key)
                .Where(g => Resolve(g, companyId: string.Empty)?.Scope == AppSettingScope.TenantOnly)
                .Select(g => g.Key)
                .ToList();

            if (violatingKeys.Count > 0)
            {
                throw new AppSettingScopeViolationException(
                    $"Setting(s) '{string.Join(", ", violatingKeys)}' are tenant-only and cannot be overridden per company.");
            }
        }

        try
        {
            // Ensure database is ready
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, skipping bulk settings insert");
                return;
            }

            // ✅ Performance Fix 1: Use proper SQL translation with ToList() and create lookup dictionary
            var existingSettingsDict = (await AppSettings
                    .Where(s => settingKeys.Contains(s.Key) && s.CompanyId == companyId)
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

                    if (companyId.Length > 0)
                        newSetting.WithCompanyId(companyId);

                    await AppSettings.AddAsync(newSetting);
                }
            }

            if (companyId.Length > 0)
            {
                await _context.SaveChangesAsync();
            }
            else
            {
                using (CompanyStampScope.EnterSharedAsTenantWideWriter(
                           $"Setting(s) '{string.Join(", ", settingKeys)}'"))
                    await _context.SaveChangesAsync();
            }

            // Clear cache for all affected settings
            foreach (var key in settingKeys)
            {
                _cache.Remove(CacheKey(key));
            }

            if (companyId.Length == 0)
                EvictTenantWide();

            _logger.LogInformation("Bulk inserted/updated {Count} settings in category {Category}", settings.Count, category);
        }
        catch (Exception ex) when (ex is not CrossCompanyWriteException
                                       and not AppSettingScopeViolationException
                                       and not DbUpdateException)
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

            var companyId = EffectiveCompanyId;
            var query = Candidates(companyId);

            if (!category.IsEmpty())
                query = query.Where(s => s.Category == category);

            var rows = await query.ToListAsync();

            return rows
                .GroupBy(s => s.Key)
                .Select(g => Resolve(g, companyId))
                .Where(s => s is not null)
                .Select(s => s!)
                .OrderBy(s => s.Category).ThenBy(s => s.Key)
                .ToList();
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

            var companyId = EffectiveCompanyId;
            return Resolve(await Candidates(companyId).Where(s => s.Key == key).ToListAsync(), companyId);
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

            var companyId = EffectiveCompanyId;

            // Exact match, never the resolve fallback: a company delete must not remove the shared row.
            var setting = await AppSettings.FirstOrDefaultAsync(s => s.Key == key && s.CompanyId == companyId);

            if (setting != null)
            {
                setting.MarkAsDeleted();
                await _context.SaveChangesAsync();
                _cache.Remove(CacheKey(key));

                if (companyId.Length == 0)
                    EvictTenantWide();

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

            var companyId = EffectiveCompanyId;
            return Resolve(await Candidates(companyId).Where(s => s.Key == key).ToListAsync(), companyId) is not null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if setting {Key} exists. This might be due to database schema issues.", key);
            return false;
        }
    }

    public async Task<bool> SettingExistsForCurrentCompanyAsync(string key)
    {
        try
        {
            if (!_context.Database.CanConnect())
            {
                _logger.LogWarning("Database is not available, returning false for setting {Key}", key);
                return false;
            }

            var companyId = EffectiveCompanyId;
            return await AppSettings.AnyAsync(s => s.Key == key && s.CompanyId == companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if setting {Key} exists. This might be due to database schema issues.", key);
            return false;
        }
    }
}
