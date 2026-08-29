using FluentAssertions;
using Microsoft.AspNetCore.Http;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Contracts;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;
using Xunit;

namespace QimErp.Shared.Common.Tests.TenantSetup;

public class TenantModuleAccessServiceTests
{
    private const string TenantId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    [Fact]
    public async Task IsModuleEnabledAsync_BaseModelModule_ReturnsTrue_WithoutCache()
    {
        var service = CreateService(new InMemoryDistributedCacheService());

        var enabled = await service.IsModuleEnabledAsync(TenantId, ModuleKeys.Leave);

        enabled.Should().BeTrue();
    }

    [Fact]
    public async Task IsModuleEnabledAsync_OptionalModule_ReturnsFalse_WhenSnapshotMissing()
    {
        var service = CreateService(new InMemoryDistributedCacheService());

        var enabled = await service.IsModuleEnabledAsync(TenantId, ModuleKeys.Payroll);

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task IsModuleEnabledAsync_OptionalModule_ReturnsTrue_WhenSnapshotContainsModule()
    {
        var cache = new InMemoryDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantModuleSnapshot(Guid.Parse(TenantId)),
            new TenantModuleSnapshotEntry(1, [ModuleKeys.CoreHR, ModuleKeys.Leave, ModuleKeys.Payroll]),
            expiration: null);

        var service = CreateService(cache);

        var enabled = await service.IsModuleEnabledAsync(TenantId, ModuleKeys.Payroll);

        enabled.Should().BeTrue();
    }

    [Fact]
    public async Task IsModuleEnabledAsync_OptionalModule_ReturnsFalse_WhenSnapshotExcludesModule()
    {
        var cache = new InMemoryDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantModuleSnapshot(Guid.Parse(TenantId)),
            new TenantModuleSnapshotEntry(1, [ModuleKeys.CoreHR, ModuleKeys.Leave]),
            expiration: null);

        var service = CreateService(cache);

        var enabled = await service.IsModuleEnabledAsync(TenantId, ModuleKeys.Payroll);

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task GetInstalledModuleKeysAsync_SameRequest_ReadsDistributedCacheOnce()
    {
        var cache = new CountingDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantModuleSnapshot(Guid.Parse(TenantId)),
            new TenantModuleSnapshotEntry(1, [ModuleKeys.Payroll]),
            expiration: null);

        var httpContext = new DefaultHttpContext();
        var accessor = new FakeHttpContextAccessor { HttpContext = httpContext };
        var service = CreateService(cache, accessor);

        var first = await service.GetInstalledModuleKeysAsync(TenantId);
        var second = await service.GetInstalledModuleKeysAsync(TenantId);

        first.Should().ContainSingle().Which.Should().Be(ModuleKeys.Payroll);
        second.Should().ContainSingle().Which.Should().Be(ModuleKeys.Payroll);
        cache.ReadCount.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IsModuleEnabledAsync_ReturnsFalse_WhenTenantIdIsMissing(string? tenantId)
    {
        var service = CreateService(new InMemoryDistributedCacheService());

        var enabled = await service.IsModuleEnabledAsync(tenantId, ModuleKeys.Payroll);

        enabled.Should().BeFalse();
    }

    [Theory]
    [InlineData(ModuleKeys.CoreHR)]
    [InlineData(ModuleKeys.Leave)]
    public async Task IsModuleEnabledAsync_BaseModelModule_ReturnsFalse_WhenTenantIdIsMissing(string moduleKey)
    {
        var service = CreateService(new InMemoryDistributedCacheService());

        var enabled = await service.IsModuleEnabledAsync(null, moduleKey);

        enabled.Should().BeFalse();
    }

    [Fact]
    public async Task IsModuleEnabledAsync_ReturnsFalse_WhenTenantIdIsMissing_EvenWhenSnapshotExists()
    {
        var cache = new InMemoryDistributedCacheService();
        await cache.SetAsync(
            SharedCacheKeys.TenantModuleSnapshot(Guid.Parse(TenantId)),
            new TenantModuleSnapshotEntry(1, [ModuleKeys.CoreHR, ModuleKeys.Leave, ModuleKeys.Payroll]),
            expiration: null);

        var service = CreateService(cache);

        var enabled = await service.IsModuleEnabledAsync(string.Empty, ModuleKeys.Payroll);

        enabled.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IsModuleEnabledAsync_ReturnsTrue_WhenNoModuleIsRequired(string moduleKey)
    {
        var service = CreateService(new InMemoryDistributedCacheService());

        var enabled = await service.IsModuleEnabledAsync(TenantId, moduleKey);

        enabled.Should().BeTrue();
    }

    private static TenantModuleAccessService CreateService(
        IDistributedCacheService cache,
        IHttpContextAccessor? httpContextAccessor = null) =>
        new(cache, httpContextAccessor ?? new FakeHttpContextAccessor());
}

internal sealed class FakeHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}

internal sealed class CountingDistributedCacheService : IDistributedCacheService
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

internal sealed class InMemoryDistributedCacheService : IDistributedCacheService
{
    private readonly Dictionary<string, object> _store = new(StringComparer.Ordinal);

    public Task<T?> GetAsync<T>(string key) =>
        Task.FromResult(_store.TryGetValue(key, out var value) ? (T?)value : default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _store[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern) => Task.CompletedTask;

    public Task<bool> ExistsAsync(string key) =>
        Task.FromResult(_store.ContainsKey(key));

    public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) =>
        throw new NotSupportedException();

    public Task<T?> GetAsync<T>(string key, string? region = null) => GetAsync<T>(key);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, string? region = null)
    {
        _store[key] = value!;
        return Task.CompletedTask;
    }

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
