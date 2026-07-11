using FluentAssertions;
using Microsoft.AspNetCore.Http;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Contracts;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.TenantSetup;

public class TenantPluginAccessServiceTests
{
    private const string TenantId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    [Fact]
    public async Task IsPluginEnabledAsync_ReturnsFalse_WhenSnapshotMissing()
    {
        var service = CreateService(new InMemoryDistributedCacheService());

        var enabled = await service.IsPluginEnabledAsync(TenantId, PluginKeys.SsnitFiling);

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task IsPluginEnabledAsync_ReturnsTrue_WhenSnapshotContainsPlugin()
    {
        var cache = new InMemoryDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantPluginSnapshot(Guid.Parse(TenantId)),
            new TenantPluginSnapshotEntry(1, [PluginKeys.SsnitFiling, PluginKeys.Esign]),
            expiration: null);

        var service = CreateService(cache);

        var enabled = await service.IsPluginEnabledAsync(TenantId, PluginKeys.SsnitFiling);

        enabled.Should().BeTrue();
    }

    [Fact]
    public async Task IsPluginEnabledAsync_ReturnsFalse_WhenSnapshotExcludesPlugin()
    {
        var cache = new InMemoryDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantPluginSnapshot(Guid.Parse(TenantId)),
            new TenantPluginSnapshotEntry(1, [PluginKeys.Esign]),
            expiration: null);

        var service = CreateService(cache);

        var enabled = await service.IsPluginEnabledAsync(TenantId, PluginKeys.SsnitFiling);

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task GetInstalledPluginKeysAsync_SameRequest_ReadsDistributedCacheOnce()
    {
        var cache = new PluginCountingDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantPluginSnapshot(Guid.Parse(TenantId)),
            new TenantPluginSnapshotEntry(1, [PluginKeys.GhIpssExport]),
            expiration: null);

        var httpContext = new DefaultHttpContext();
        var accessor = new PluginFakeHttpContextAccessor { HttpContext = httpContext };
        var service = CreateService(cache, accessor);

        var first = await service.GetInstalledPluginKeysAsync(TenantId);
        var second = await service.GetInstalledPluginKeysAsync(TenantId);

        first.Should().ContainSingle().Which.Should().Be(PluginKeys.GhIpssExport);
        second.Should().ContainSingle().Which.Should().Be(PluginKeys.GhIpssExport);
        cache.ReadCount.Should().Be(1);
    }

    private static TenantPluginAccessService CreateService(
        IDistributedCacheService cache,
        IHttpContextAccessor? httpContextAccessor = null) =>
        new(cache, httpContextAccessor ?? new PluginFakeHttpContextAccessor());
}

internal sealed class PluginFakeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}

internal sealed class PluginCountingDistributedCacheService : IDistributedCacheService
{
    private readonly InMemoryDistributedCacheService _inner = new();
    public int ReadCount { get; private set; }

    public Task<T?> GetAsync<T>(string key)
    {
        ReadCount++;
        return _inner.GetAsync<T>(key);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) =>
        _inner.SetAsync(key, value, expiration);

    public Task RemoveAsync(string key) => _inner.RemoveAsync(key);

    public Task RemoveByPatternAsync(string pattern) => _inner.RemoveByPatternAsync(pattern);

    public Task<bool> ExistsAsync(string key) => _inner.ExistsAsync(key);

    public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) =>
        throw new NotSupportedException();

    public Task<T?> GetAsync<T>(string key, string? region = null) => GetAsync<T>(key);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? region = null) =>
        SetAsync(key, value, expiration);

    public Task RemoveAsync(string key, string? region = null) => RemoveAsync(key);

    public Task RemoveByPatternAsync(string pattern, string? region = null) => RemoveByPatternAsync(pattern);

    public Task<bool> ExistsAsync(string key, string? region = null) => ExistsAsync(key);

    public Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        string? region = null) =>
        throw new NotSupportedException();
}
