using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Contracts;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Extensions;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Database;

public sealed class ActorNameWriteStopTests
{
    private const string Tenant = "019e31ec-name-0000-0000-000000000001";

    private sealed class NameTestDbContext(
        DbContextOptions<NameTestDbContext> options,
        ITenantContext tenantContext)
        : ApplicationDbContext<NameTestDbContext>(options, tenantContext)
    {
    }

    private sealed class SimpleResponse : WorkflowEnabledResponse;

    private sealed class SimpleBuilder(EntityCodeConfig entity)
        : ResponseBuilder<EntityCodeConfig, SimpleResponse>(entity);

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _root;
        private readonly IServiceScope _scope;

        public NameTestDbContext Db { get; }

        public Harness()
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
            userSvc.SetContext(Tenant, "tester@qimerp.com", "Real Name");
            sp.GetRequiredService<ITenantContext>().SetTenant(Tenant);

            var interceptor = sp.GetRequiredService<AuditEntitySaveChangesInterceptor>();
            var opts = new DbContextOptionsBuilder<NameTestDbContext>()
                .UseInMemoryDatabase($"names-{Guid.NewGuid()}")
                .AddInterceptors(interceptor)
                .Options;

            Db = new NameTestDbContext(opts, sp.GetRequiredService<ITenantContext>());
        }

        public void Dispose()
        {
            Db.Dispose();
            _scope.Dispose();
            _root.Dispose();
        }
    }

    [Fact]
    public async Task Interceptor_create_persists_empty_actor_names_and_keeps_userId_email()
    {
        using var harness = new Harness();

        var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
        harness.Db.EntityCodeConfigs.Add(entity);
        await harness.Db.SaveChangesAsync();

        entity.CreatedByUserId.Should().NotBeNullOrWhiteSpace();
        entity.CreatedByEmail.Should().Be("tester@qimerp.com");
        entity.CreatedByName.Should().BeEmpty();
        entity.LastModifiedByName.Should().BeEmpty();
        entity.EntityStatusCreatedByName.Should().BeEmpty();
        entity.EntityStatusLastModifiedByName.Should().BeEmpty();
    }

    [Fact]
    public async Task Interceptor_modify_persists_empty_LastModifiedByName()
    {
        using var harness = new Harness();

        var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
        harness.Db.EntityCodeConfigs.Add(entity);
        await harness.Db.SaveChangesAsync();

        entity.UpdateFormat("EMP", "-", true, 5);
        await harness.Db.SaveChangesAsync();

        entity.LastModifiedByUserId.Should().NotBeNullOrWhiteSpace();
        entity.LastModifiedByEmail.Should().Be("tester@qimerp.com");
        entity.LastModifiedByName.Should().BeEmpty();
    }

    [Fact]
    public void Direct_OnCreate_ignores_name_argument()
    {
        var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
        entity.OnCreate("user-1", "user@example.com", "Real Name");

        entity.CreatedByUserId.Should().Be("user-1");
        entity.CreatedByEmail.Should().Be("user@example.com");
        entity.CreatedByName.Should().BeEmpty();
        entity.LastModifiedByName.Should().BeEmpty();
        entity.EntityStatusCreatedByName.Should().BeEmpty();
    }

    [Fact]
    public void ResponseBuilder_Create_maps_empty_names_without_throw()
    {
        var entity = EntityCodeConfig.Create(Tenant, "Employee", prefix: "EMP");
        entity.OnCreate("user-1", "user@example.com", "Real Name");

        var response = new SimpleBuilder(entity).Create();

        response.CreatedByName.Should().BeEmpty();
        response.LastModifiedByName.Should().BeEmpty();
        response.CreatedByEmail.Should().Be("user@example.com");
    }
}
