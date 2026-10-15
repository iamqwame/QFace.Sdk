using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class AuditableEntityVersionTests
{
    private const string Tenant = "019e31ec-ver-0000-0000-000000000001";

    private sealed class VersionTestDbContext(
        DbContextOptions<VersionTestDbContext> options,
        ITenantContext tenantContext)
        : ApplicationDbContext<VersionTestDbContext>(options, tenantContext)
    {
    }

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public VersionTestDbContext Db { get; }
        public UserContextService UserSvc { get; }

        public Harness(string? dbName = null)
        {
            var services = new ServiceCollection();
            services.AddHttpContextAccessor();
            services.AddScoped<UserContextService>();
            services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddLogging();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddScoped<AuditEntitySaveChangesInterceptor>();

            _root = services.BuildServiceProvider();
            _scope = _root.CreateScope();
            var sp = _scope.ServiceProvider;

            UserSvc = sp.GetRequiredService<UserContextService>();
            UserSvc.SetContext(Tenant, "tester@qimerp.com", "Test User");
            sp.GetRequiredService<ITenantContext>().SetTenant(Tenant);

            var interceptor = sp.GetRequiredService<AuditEntitySaveChangesInterceptor>();
            var opts = new DbContextOptionsBuilder<VersionTestDbContext>()
                .UseInMemoryDatabase(dbName ?? $"version-{Guid.NewGuid()}")
                .AddInterceptors(interceptor)
                .Options;

            Db = new VersionTestDbContext(opts, sp.GetRequiredService<ITenantContext>());
        }

        public void Dispose()
        {
            Db.Dispose();
            _scope.Dispose();
            _root.Dispose();
        }
    }

    [Fact]
    public async Task Insert_auditable_entity_Version_is_1()
    {
        using var harness = new Harness();

        var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
        harness.Db.EntityCodeConfigs.Add(entity);
        await harness.Db.SaveChangesAsync();

        entity.Version.Should().Be(1);
    }

    [Fact]
    public async Task Update_auditable_entity_increments_Version()
    {
        using var harness = new Harness();

        var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
        harness.Db.EntityCodeConfigs.Add(entity);
        await harness.Db.SaveChangesAsync();

        entity.UpdateFormat("EMP", "-", true, 5);
        await harness.Db.SaveChangesAsync();

        entity.Version.Should().Be(2);
    }

    [Fact]
    public async Task Stale_Version_throws_DbUpdateConcurrencyException()
    {
        var dbName = $"version-concurrency-{Guid.NewGuid()}";

        Guid id;
        using (var harness = new Harness(dbName))
        {
            var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
            harness.Db.EntityCodeConfigs.Add(entity);
            await harness.Db.SaveChangesAsync();
            id = entity.Id;
        }

        using var writerA = new Harness(dbName);
        using var writerB = new Harness(dbName);

        var a = await writerA.Db.EntityCodeConfigs.SingleAsync(e => e.Id == id);
        var b = await writerB.Db.EntityCodeConfigs.SingleAsync(e => e.Id == id);

        a.UpdateFormat("EMP", "-", true, 6);
        await writerA.Db.SaveChangesAsync();

        b.UpdateFormat("EMP", "-", true, 7);
        var act = async () => await writerB.Db.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }

    [Fact]
    public async Task BulkSeedScope_stamps_audit_and_keeps_Version_1_on_insert()
    {
        using var harness = new Harness();

        using (BulkSeedScope.Enter())
        {
            var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
            harness.Db.EntityCodeConfigs.Add(entity);
            await harness.Db.SaveChangesAsync();

            entity.Version.Should().Be(1);
            entity.CreatedByUserId.Should().NotBeNullOrWhiteSpace();
            entity.CreatedByEmail.Should().Be("tester@qimerp.com");
        }
    }

    /// <summary>
    /// Reproduces the InventoryOnHand/Requisition shape: long AuditableEntity.Version plus a
    /// separate uint row-version property. Interceptor must increment only the long Version.
    /// </summary>
    private sealed class RowVersionEntity : GuidAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public uint RowVersion { get; private set; }
    }

    /// <summary>
    /// Legacy shape: uint Version hides AuditableEntity.Version and is mapped as a row-version.
    /// Interceptor must skip increment (ClrType is not long).
    /// </summary>
    private sealed class UIntVersionEntity : GuidAuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public new uint Version { get; private set; }
    }

    private sealed class VersionEdgeDbContext : DbContext
    {
        public VersionEdgeDbContext(DbContextOptions<VersionEdgeDbContext> options) : base(options) { }

        public DbSet<RowVersionEntity> RowVersionEntities => Set<RowVersionEntity>();
        public DbSet<UIntVersionEntity> UIntVersionEntities => Set<UIntVersionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RowVersionEntity>(b =>
            {
                b.Property(e => e.Version)
                    .IsRequired()
                    .HasDefaultValue(1L)
                    .IsConcurrencyToken();
                b.Property(e => e.RowVersion).IsRowVersion();
                b.Ignore(e => e.CustomFields);
            });

            modelBuilder.Entity<UIntVersionEntity>(b =>
            {
                b.Property(e => e.Version).IsRowVersion();
                b.Ignore(e => e.CustomFields);
            });
        }
    }

    private sealed class VersionEdgeHarness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public VersionEdgeDbContext Db { get; }

        public VersionEdgeHarness()
        {
            var services = new ServiceCollection();
            services.AddHttpContextAccessor();
            services.AddScoped<UserContextService>();
            services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddLogging();
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddScoped<AuditEntitySaveChangesInterceptor>();

            _root = services.BuildServiceProvider();
            _scope = _root.CreateScope();
            var sp = _scope.ServiceProvider;

            var userSvc = sp.GetRequiredService<UserContextService>();
            userSvc.SetContext(Tenant, "tester@qimerp.com", "Test User");
            sp.GetRequiredService<ITenantContext>().SetTenant(Tenant);

            var interceptor = sp.GetRequiredService<AuditEntitySaveChangesInterceptor>();
            var opts = new DbContextOptionsBuilder<VersionEdgeDbContext>()
                .UseInMemoryDatabase($"version-edge-{Guid.NewGuid()}")
                .AddInterceptors(interceptor)
                .Options;

            Db = new VersionEdgeDbContext(opts);
        }

        public void Dispose()
        {
            Db.Dispose();
            _scope.Dispose();
            _root.Dispose();
        }
    }

    [Fact]
    public async Task Update_with_RowVersion_still_increments_long_Version()
    {
        using var harness = new VersionEdgeHarness();

        var entity = new RowVersionEntity { Name = "stock" };
        entity.WithTenantId(Tenant).AsActive();
        harness.Db.RowVersionEntities.Add(entity);
        await harness.Db.SaveChangesAsync();

        entity.Version.Should().Be(1);

        entity.Name = "stock-updated";
        await harness.Db.SaveChangesAsync();

        entity.Version.Should().Be(2);
    }

    [Fact]
    public async Task Update_with_uint_Version_rowversion_does_not_throw()
    {
        using var harness = new VersionEdgeHarness();

        var entity = new UIntVersionEntity { Name = "req" };
        entity.WithTenantId(Tenant).AsActive();
        harness.Db.UIntVersionEntities.Add(entity);
        await harness.Db.SaveChangesAsync();

        entity.Name = "req-updated";
        var act = async () => await harness.Db.SaveChangesAsync();

        await act.Should().NotThrowAsync(
            "interceptor must skip increment when Version ClrType is uint (row-version / shadow)");
    }
}
