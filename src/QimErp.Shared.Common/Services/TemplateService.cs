using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QFace.Sdk.BlobStorage.Models;
using QFace.Sdk.BlobStorage.Services;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services;

public interface ITemplateService
{
    Task<string> RenderEmailTemplateAsync(string templateName, Dictionary<string, string> replacements);
    Task<string> LoadTemplateAsync(string templatePath);
    Task InvalidateCacheAsync(string templateName);
}

/// <summary>
/// Template service that loads templates from S3 with Redis caching.
/// </summary>
public class TemplateService(
    IFileUploadService storage,
    IOptions<BlobStorageOptions> options,
    IDistributedCacheService cache,
    IConfiguration configuration,
    ILogger<TemplateService> logger) : ITemplateService
{

    private static string GetCacheKey(string templateName) => $"Templates:Emails:{templateName}.html";

    private static bool _redisDisabledWarningLogged;

    /// <summary>
    /// Renders an email template by loading it from S3 (with Redis caching) and replacing tokens with provided values.
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="replacements"></param>
    /// <returns></returns>
    public async Task<string> RenderEmailTemplateAsync(string templateName, Dictionary<string, string> replacements)
    {
        try
        {
            var templatePath = Path.Combine("Templates", "Emails", $"{templateName}.html");
            var template = await LoadTemplateAsync(templatePath);
            return ReplaceTokens(template, replacements);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rendering email template: {TemplateName}", templateName);
            throw;
        }
    }

    /// <summary>
    /// Loads a template from S3 with Redis caching.
    /// </summary>
    public async Task<string> LoadTemplateAsync(string templatePath)
    {
        var templateName = Path.GetFileNameWithoutExtension(Path.GetFileName(templatePath));
        var key = GetCacheKey(templateName);

        if (!_redisDisabledWarningLogged && !configuration.GetValue<bool>("RedisCache:Enabled", true))
        {
            _redisDisabledWarningLogged = true;
            logger.LogWarning(
                "Template caching requires Redis. RedisCache:Enabled is false; templates will be fetched from S3 on every request.");
        }

        var cached = await cache.GetAsync<string>(key);
        if (!string.IsNullOrEmpty(cached))
        {
            logger.LogDebug("Template loaded from cache: {TemplatePath}", templatePath);
            return cached;
        }

        var prefix = options.Value.GetTemplateStoragePrefix();
        var s3Key = $"{prefix}/{Path.GetFileName(templatePath)}";
        var content = await storage.GetObjectContentAsync(s3Key);

        if (content == null)
            throw new FileNotFoundException($"Template file not found: {templatePath}");

        var cacheMinutes = options.Value.TemplateStorage?.CacheMinutes ?? 15;
        var effectiveTtl = cacheMinutes > 0 ? TimeSpan.FromMinutes(cacheMinutes) : TimeSpan.FromMinutes(15);

        await cache.SetAsync(key, content, effectiveTtl);
        logger.LogDebug("Template cached: {TemplatePath}", templatePath);

        return content;
    }

    /// <summary>
    /// Invalidates the cache for the specified template.
    /// </summary>
    public async Task InvalidateCacheAsync(string templateName)
    {
        var key = GetCacheKey(templateName);
        await cache.RemoveAsync(key);
        logger.LogDebug("Cache invalidated for template: {TemplateName}", templateName);
    }

    private static string ReplaceTokens(string template, Dictionary<string, string> replacements)
    {
        if (replacements == null || replacements.Count == 0)
            return template;

        var result = template;
        foreach (var replacement in replacements)
        {
            var token = $"{{{{{replacement.Key}}}}}";
            result = result.Replace(token, replacement.Value ?? string.Empty);
        }
        return result;
    }
}
