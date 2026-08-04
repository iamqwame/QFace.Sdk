using Microsoft.Extensions.Options;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Services.AI;

/// <summary>
/// IAIOptionsProvider that falls back to the shared Redis cache (seeded by IAM's
/// PlatformAIConfigSeedService) when the host has no local AI:* provider key of its own.
/// Registered as a singleton but resolves the scoped IDistributedCacheService via a
/// DI scope per cache read.
/// </summary>
public sealed class CachedAIOptionsProvider(
    IOptions<AIOptions> localOptions,
    IServiceScopeFactory scopeFactory,
    ILogger<CachedAIOptionsProvider> logger) : IAIOptionsProvider
{
    private static readonly TimeSpan FreshWindow = TimeSpan.FromMinutes(5);

    private readonly object _lock = new();
    private AIOptions? _cached;
    private DateTimeOffset _cachedAt;

    public async Task<AIOptions> GetOptionsAsync(CancellationToken cancellationToken = default)
    {
        var local = localOptions.Value;
        if (HasAnyProviderKey(local))
        {
            return local;
        }

        lock (_lock)
        {
            if (_cached is not null && DateTimeOffset.UtcNow - _cachedAt < FreshWindow)
            {
                return _cached;
            }
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCacheService>();
            var entry = await cache.GetAsync<AIOptions>(SharedCacheKeys.PlatformAIProviderConfig());

            if (entry is not null)
            {
                lock (_lock)
                {
                    _cached = entry;
                    _cachedAt = DateTimeOffset.UtcNow;
                }

                return entry;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read platform AI provider config from cache");
        }

        lock (_lock)
        {
            if (_cached is not null)
            {
                logger.LogWarning("Serving stale cached AI provider config — cache read failed or returned nothing");
                return _cached;
            }
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
