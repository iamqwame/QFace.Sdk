using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Database.Configurations;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Tests.Workflow;
using QimErp.Shared.Common.Workflow;
using QimErp.Shared.Common.Workflow.Entities;
using QimErp.Shared.Common.Workflow.Enums;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

/// <summary>
/// Covers the WorkflowEventMessage / <see cref="IWorkflowTriggerBridge"/> pre-commit path of the
/// outbox backstop: the dispatch attempt already happened synchronously by the time the
/// OutboxMessage row is built, so IsProcessed is set directly at enqueue time (no second
/// SaveChanges needed, unlike the post-commit MediatR path in
/// <see cref="AuditEntitySaveChangesInterceptorOutboxTests"/>).
/// </summary>
public sealed class OutboxWorkflowBridgeTests
{
    private const string TenantId = "e2e-tenant-id";
    private const string ModuleName = "TestModule";

    // Extends the existing Workflow test harness's TestWorkflowDbContext so its model also maps
    // OutboxMessage — the real TestWorkflowDbContext deliberately has no outbox mapping so the
    // original WorkflowInterceptorE2ETests keep exercising a context that does NOT support outbox.
    private sealed class OutboxCapableWorkflowDbContext(DbContextOptions<TestWorkflowDbContext> options)
        : TestWorkflowDbContext(options)
    {
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        }
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public Harness(string moduleName = ModuleName)
        {
            ConfigCache = new FakeWorkflowConfigCacheService();
            DefinitionProvider = new FakeWorkflowDefinitionProvider(TenantId);
            Bridge = new RecordingWorkflowTriggerBridge();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Workflow:Module"] = moduleName })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ICurrentUserService>(new FakeCurrentUserService(TenantId));
            services.AddSingleton<ITenantContext>(new TenantContext());
            services.AddSingleton(ConfigCache);
            services.AddSingleton<IWorkflowConfigCacheService>(sp => sp.GetRequiredService<FakeWorkflowConfigCacheService>());
            services.AddSingleton(DefinitionProvider);
            services.AddSingleton<IWorkflowDefinitionProvider>(sp => sp.GetRequiredService<FakeWorkflowDefinitionProvider>());
            services.AddSingleton(Bridge);
            services.AddSingleton<IWorkflowTriggerBridge>(sp => sp.GetRequiredService<RecordingWorkflowTriggerBridge>());
            services.AddSingleton<ITenantModuleAccessService>(new FakeTenantModuleAccessService());
            services.AddSingleton<IWorkflowService, WorkflowService>();
            services.Configure<SystemOptions>(options =>
            {
                options.DefaultUserId = "test-user";
                options.DefaultSystemEmail = "tester@qimerp.com";
                options.DefaultUserName = "Tester";
            });

            _serviceProvider = services.BuildServiceProvider();

            var interceptor = new AuditEntitySaveChangesInterceptor(
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _serviceProvider.GetRequiredService<ILogger<AuditEntitySaveChangesInterceptor>>(),
                _serviceProvider,
                configuration);

            var options2 = new DbContextOptionsBuilder<TestWorkflowDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            Context = new OutboxCapableWorkflowDbContext(options2);
        }

        public OutboxCapableWorkflowDbContext Context { get; }
        public FakeWorkflowConfigCacheService ConfigCache { get; }
        public FakeWorkflowDefinitionProvider DefinitionProvider { get; }
        public RecordingWorkflowTriggerBridge Bridge { get; }

        public ValueTask DisposeAsync()
        {
            Context.Dispose();
            _serviceProvider.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private static TestWorkflowEntity NewEntity()
    {
        var entity = new TestWorkflowEntity { Title = "Initial", Amount = 100, Status = "Active" };
        entity.EnableWorkflowProcessing();
        entity.TenantId = TenantId;
        return entity;
    }

    private static EntityWorkflowConfig CreateConfig(bool enableCreate, string createCode) => new()
    {
        EnableWorkflowForCreate = enableCreate,
        CreateWorkflowCode = createCode
    };

    private static PublishedWorkflowDefinition ActiveDefinition(string stepCode) => new()
    {
        TenantId = TenantId,
        WorkflowCode = stepCode,
        EntityType = nameof(TestWorkflowEntity),
        IsActive = true,
        Definition = new WorkflowDefinition
        {
            Steps = [new WorkflowStep { StepCode = stepCode, Name = stepCode, Order = 1 }]
        }
    };

    [Fact(DisplayName = "7: Bridge dispatch succeeding (pre-commit) enqueues an OutboxMessage already marked IsProcessed=true")]
    public async Task BridgeDispatchSucceeds_OutboxRowAlreadyMarkedProcessed()
    {
        await using var harness = new Harness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        harness.DefinitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));

        var entity = NewEntity();
        harness.Context.Add(entity);
        await harness.Context.SaveChangesAsync();

        harness.Bridge.Messages.Should().ContainSingle("the existing pre-commit Temporal bridge timing must be preserved");

        // One row for the WorkflowEventMessage dispatched via the bridge, one for the
        // WorkflowStatusChangedEvent domain event destined for the post-commit MediatR path.
        var rows = await harness.Context.OutboxMessages.ToListAsync();
        rows.Should().HaveCount(2);

        var bridgeRow = rows.Single(r => r.EventType!.Contains(nameof(WorkflowEventMessage)));
        bridgeRow.IsProcessed.Should().BeTrue("the bridge call already succeeded synchronously before the row was enqueued");

        var envelope = OutboxEnvelope.Unwrap(bridgeRow.Payload);
        envelope!.TenantId.Should().Be(TenantId);
        envelope.Data.Should().Contain("CREATE-WF");
    }

    [Fact(DisplayName = "8: Bridge not registered leaves the OutboxMessage row unprocessed for the backstop processor")]
    public async Task BridgeNotRegistered_OutboxRowStaysUnprocessed()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Workflow:Module"] = ModuleName })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ICurrentUserService>(new FakeCurrentUserService(TenantId));
        services.AddSingleton<ITenantContext>(new TenantContext());
        var configCache = new FakeWorkflowConfigCacheService();
        configCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        services.AddSingleton<IWorkflowConfigCacheService>(configCache);
        var definitionProvider = new FakeWorkflowDefinitionProvider(TenantId);
        definitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));
        services.AddSingleton<IWorkflowDefinitionProvider>(definitionProvider);
        services.AddSingleton<ITenantModuleAccessService>(new FakeTenantModuleAccessService());
        services.AddSingleton<IWorkflowService, WorkflowService>();
        services.Configure<SystemOptions>(o => { o.DefaultUserId = "test-user"; o.DefaultSystemEmail = "t@q.com"; o.DefaultUserName = "T"; });
        // Deliberately NOT registering IWorkflowTriggerBridge.

        await using var sp = services.BuildServiceProvider();
        var interceptor = new AuditEntitySaveChangesInterceptor(
            sp.GetRequiredService<ICurrentUserService>(),
            sp.GetRequiredService<ILogger<AuditEntitySaveChangesInterceptor>>(),
            sp,
            configuration);

        var options = new DbContextOptionsBuilder<TestWorkflowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        await using var context = new OutboxCapableWorkflowDbContext(options);

        var entity = NewEntity();
        context.Add(entity);
        await context.SaveChangesAsync();

        var row = await context.OutboxMessages.SingleAsync(r => r.EventType!.Contains(nameof(WorkflowEventMessage)));
        row.IsProcessed.Should().BeFalse("no bridge was available to dispatch the message synchronously");
    }

    [Fact(DisplayName = "9: A DbContext whose model does not map OutboxMessage is unaffected (back-compat with WorkflowInterceptorE2ETests-style contexts)")]
    public async Task ContextWithoutOutboxMapping_DoesNotThrow()
    {
        // Uses the real TestWorkflowDbContext (no Outbox mapping) directly — the exact shape
        // WorkflowInterceptorE2ETests already relies on. Must behave exactly as before.
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Workflow:Module"] = ModuleName })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ICurrentUserService>(new FakeCurrentUserService(TenantId));
        services.AddSingleton<ITenantContext>(new TenantContext());
        var configCache = new FakeWorkflowConfigCacheService();
        configCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        services.AddSingleton<IWorkflowConfigCacheService>(configCache);
        var definitionProvider = new FakeWorkflowDefinitionProvider(TenantId);
        definitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));
        services.AddSingleton<IWorkflowDefinitionProvider>(definitionProvider);
        var bridge = new RecordingWorkflowTriggerBridge();
        services.AddSingleton<IWorkflowTriggerBridge>(bridge);
        services.AddSingleton<ITenantModuleAccessService>(new FakeTenantModuleAccessService());
        services.AddSingleton<IWorkflowService, WorkflowService>();
        services.Configure<SystemOptions>(o => { o.DefaultUserId = "test-user"; o.DefaultSystemEmail = "t@q.com"; o.DefaultUserName = "T"; });

        await using var sp = services.BuildServiceProvider();
        var interceptor = new AuditEntitySaveChangesInterceptor(
            sp.GetRequiredService<ICurrentUserService>(),
            sp.GetRequiredService<ILogger<AuditEntitySaveChangesInterceptor>>(),
            sp,
            configuration);

        var options = new DbContextOptionsBuilder<TestWorkflowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
        await using var context = new TestWorkflowDbContext(options);

        var entity = NewEntity();
        context.Add(entity);

        var act = async () => await context.SaveChangesAsync();
        await act.Should().NotThrowAsync("contexts without an OutboxMessage mapping must be unaffected by the backstop");

        bridge.Messages.Should().ContainSingle("the pre-commit Temporal trigger must still fire normally");
    }
}
