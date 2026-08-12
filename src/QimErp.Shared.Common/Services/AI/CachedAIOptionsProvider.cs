using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services.AI;

/// <summary>
/// IAIOptionsProvider resolution order: (1) the calling tenant's own provider config, if one
/// has been saved via IAM's tenant AI-provider settings endpoint, (2) this host's local AI:*
/// config, (3) the shared Redis fallback seeded by IAM's PlatformAIConfigSeedService.
/// Registered as a singleton but resolves scoped services (IDistributedCacheService,
/// ICurrentUserService) via a DI scope per call.
/// </summary>
public sealed class CachedAIOptionsProvider(
    IOptions<AIOptions> localOptions,
    IServiceScopeFactory scopeFactory,
    ILogger<CachedAIOptionsProvider> logger) : IAIOptionsProvider
{
    private static readonly TimeSpan FreshWindow = TimeSpan.FromMinutes(5);
    private const string GlobalCacheKey = "__global__";

    // Keyed by tenant id (or GlobalCacheKey for the platform-wide fallback) — a single shared
    // slot would let one tenant's resolved options leak to another for up to FreshWindow.
    private readonly ConcurrentDictionary<string, (AIOptions Options, DateTimeOffset CachedAt)> _cache = new();

    public async Task<AIOptions> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = ResolveTenantId();
        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenantOptions = await TryGetTenantOptionsAsync(tenantId);
            if (tenantOptions is not null)
            {
                return tenantOptions;
            }
        }

        var local = localOptions.Value;
        if (HasAnyProviderKey(local))
        {
            return local;
        }

        return await GetGlobalOptionsAsync();
    }

    private string? ResolveTenantId()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            return currentUserService.GetTenantId();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not resolve tenant context for AI options — falling back to platform default");
            return null;
        }
    }

    private async Task<AIOptions?> TryGetTenantOptionsAsync(string tenantId)
    {
        if (_cache.TryGetValue(tenantId, out var fresh) && DateTimeOffset.UtcNow - fresh.CachedAt < FreshWindow)
        {
            return fresh.Options;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCacheService>();
            var entry = await cache.GetAsync<AIOptions>(SharedCacheKeys.TenantAIProviderConfig(tenantId));

            if (entry is not null)
            {
                _cache[tenantId] = (entry, DateTimeOffset.UtcNow);
            }

            // null here means "tenant has no override configured" — the caller falls through
            // to the local/global default, which is the correct behavior, not a failure.
            return entry;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read tenant AI provider config from cache for tenant {TenantId}", tenantId);
            return _cache.TryGetValue(tenantId, out var stale) ? stale.Options : null;
        }
    }

    private async Task<AIOptions> GetGlobalOptionsAsync()
    {
        if (_cache.TryGetValue(GlobalCacheKey, out var fresh) && DateTimeOffset.UtcNow - fresh.CachedAt < FreshWindow)
        {
            return fresh.Options;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCacheService>();
            var entry = await cache.GetAsync<AIOptions>(SharedCacheKeys.PlatformAIProviderConfig());

            if (entry is not null)
            {
                _cache[GlobalCacheKey] = (entry, DateTimeOffset.UtcNow);
                return entry;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read platform AI provider config from cache");
        }

        if (_cache.TryGetValue(GlobalCacheKey, out var stale))
        {
            logger.LogWarning("Serving stale cached AI provider config — cache read failed or returned nothing");
            return stale.Options;
        }

        throw new InvalidOperationException(
            "No AI provider configuration available locally or from the shared cache. " +
            "Ensure IAM's PlatformAIConfigSeedService has run at least once.");
    }

    private static bool HasAnyProviderKey(AIOptions options) =>
        !string.IsNullOrEmpty(options.OpenAI.ApiKey)
        || !string.IsNullOrEmpty(options.Anthropic.ApiKey)
        || !string.IsNullOrEmpty(options.GoogleGemini.ApiKey)
        || !string.IsNullOrEmpty(options.DeepSeek.ApiKey);
}
