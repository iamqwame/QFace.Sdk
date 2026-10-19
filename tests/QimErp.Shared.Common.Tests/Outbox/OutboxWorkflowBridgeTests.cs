using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Database.Configurations;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Outbox.Transports;
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
/// DATA-09: workflow approval hand-off enqueues <c>WorkflowApprovalRequired</c> outbox rows
/// (Temporal transport). The Akka relay delivers via <see cref="IWorkflowTriggerBridge"/> —
/// SaveChanges itself never starts Temporal.
/// </summary>
public sealed class OutboxWorkflowBridgeTests
{
    private const string TenantId = "e2e-tenant-id";
    private const string ModuleName = "TestModule";

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

    private sealed class BoundOutbox : IOutbox
    {
        private EfOutbox? _inner;

        public void Bind(DbContext db, ITransportRouting routing, TimeProvider clock) =>
            _inner = new EfOutbox(db, routing, clock);

        public void Add(IDomainEvent domainEvent, Guid entityId, long entityVersion = 0) =>
            Require().Add(domainEvent, entityId, entityVersion);

        public void Add(string eventType, string tenantId, Guid entityId, long entityVersion, object payload) =>
            Require().Add(eventType, tenantId, entityId, entityVersion, payload);

        private EfOutbox Require() =>
            _inner ?? throw new InvalidOperationException("Outbox not bound.");
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public Harness(string moduleName = ModuleName, bool registerOutbox = true)
        {
            ConfigCache = new FakeWorkflowConfigCacheService();
            DefinitionProvider = new FakeWorkflowDefinitionProvider(TenantId);
            Bridge = new RecordingWorkflowTriggerBridge();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Workflow:Module"] = moduleName })
                .Build();

            var bound = new BoundOutbox();
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
            services.Configure<OutboxOptions>(_ => { });
            if (registerOutbox)
                services.AddSingleton<IOutbox>(bound);

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
            if (registerOutbox)
            {
                var routing = new TransportRouting(_serviceProvider.GetRequiredService<IOptions<OutboxOptions>>());
                bound.Bind(Context, routing, TimeProvider.System);
            }
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

    [Fact(DisplayName = "7: Create workflow enqueues WorkflowApprovalRequired (temporal) unprocessed")]
    public async Task CreateWorkflow_EnqueuesTemporalApprovalOutboxRow()
    {
        await using var harness = new Harness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        harness.DefinitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));

        var entity = NewEntity();
        harness.Context.Add(entity);
        await harness.Context.SaveChangesAsync();

        harness.Bridge.Messages.Should().BeEmpty(
            "SaveChanges must not call IWorkflowTriggerBridge — the relay delivers Temporal later");

        var approval = await harness.Context.OutboxMessages
            .Where(r => r.EventType == WorkflowApprovalOutboxTemporalHandler.EventTypeName)
            .ToListAsync();
        approval.Should().ContainSingle();
        approval[0].Transport.Should().Be(OutboxTransports.Temporal);
        approval[0].TenantId.Should().Be(TenantId);
        approval[0].ProcessedAt.Should().BeNull();
        approval[0].Payload.Should().Contain("CREATE-WF");
    }

    [Fact(DisplayName = "8: Without IOutbox, SaveChanges still completes and no approval row is written")]
    public async Task OutboxNotRegistered_SaveSucceeds_NoApprovalRow()
    {
        await using var harness = new Harness(registerOutbox: false);
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        harness.DefinitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));

        var entity = NewEntity();
        harness.Context.Add(entity);
        await harness.Context.SaveChangesAsync();

        entity.WorkflowStatus.Should().Be(WorkflowStatus.InProgress);
        (await harness.Context.OutboxMessages.ToListAsync()).Should().BeEmpty();
    }

    [Fact(DisplayName = "9: A DbContext without OutboxMessage mapping still saves when IOutbox is absent")]
    public async Task ContextWithoutOutboxMapping_DoesNotThrow()
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
        var bridge = new RecordingWorkflowTriggerBridge();
        services.AddSingleton<IWorkflowTriggerBridge>(bridge);
        services.AddSingleton<ITenantModuleAccessService>(new FakeTenantModuleAccessService());
        services.AddSingleton<IWorkflowService, WorkflowService>();
        services.Configure<SystemOptions>(o =>
        {
            o.DefaultUserId = "test-user";
            o.DefaultSystemEmail = "t@q.com";
            o.DefaultUserName = "T";
        });

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
        await act.Should().NotThrowAsync(
            "contexts without OutboxMessage mapping and without IOutbox must still save");

        bridge.Messages.Should().BeEmpty("Temporal hand-off is outbox/relay only");
        entity.WorkflowStatus.Should().Be(WorkflowStatus.InProgress);
    }
}
