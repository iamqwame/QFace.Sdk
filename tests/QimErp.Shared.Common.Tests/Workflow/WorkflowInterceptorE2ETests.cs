using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.Workflow;
using QimErp.Shared.Common.Workflow;
using QimErp.Shared.Common.Workflow.Entities;
using QimErp.Shared.Common.Workflow.Enums;
using Xunit;

namespace QimErp.Shared.Common.Tests.Workflow;

public class WorkflowInterceptorE2ETests
{
    private const string TenantId = "e2e-tenant-id";
    private const string ModuleName = "TestModule";

    [Fact]
    public async Task E2E1_Create_SetsInProgress_CreateCode_AndTriggersTemporal()
    {
        await using var harness = CreateHarness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        harness.DefinitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));

        var entity = NewEntity();
        harness.Context.Add(entity);
        await harness.Context.SaveChangesAsync();

        entity.WorkflowStatus.Should().Be(WorkflowStatus.InProgress);
        entity.WorkflowCode.Should().Be("CREATE-WF");
        entity.CurrentWorkflowHistoryId.Should().NotBeNull();

        harness.Bridge.Messages.Should().ContainSingle();
        harness.Bridge.Messages[0].WorkflowCode.Should().Be("CREATE-WF");
        harness.Bridge.Messages[0].Module.Should().Be(ModuleName);
        harness.Bridge.Messages[0].CurrentState.Should().Be("CREATE-STEP");
    }

    [Fact]
    public async Task E2E2_UpdateRouteA_MatchesAmountField()
    {
        await using var harness = CreateHarness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(
            updateRoutes:
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "UPDATE-ROUTE-A",
                    SignificantFields = [nameof(TestWorkflowEntity.Amount)]
                }
            ]));
        harness.DefinitionProvider.SetDefinition("UPDATE-ROUTE-A", ActiveDefinition("ROUTE-A-STEP"));

        var entity = await SeedApprovedEntityAsync(harness);
        entity.Amount = 9000;
        await harness.Context.SaveChangesAsync();

        entity.WorkflowStatus.Should().Be(WorkflowStatus.InProgress);
        entity.WorkflowCode.Should().Be("UPDATE-ROUTE-A");
    }

    [Fact]
    public async Task E2E3_UpdateRouteB_MatchesStatusField()
    {
        await using var harness = CreateHarness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(
            updateRoutes:
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "UPDATE-ROUTE-B",
                    SignificantFields = [nameof(TestWorkflowEntity.Status)]
                }
            ]));
        harness.DefinitionProvider.SetDefinition("UPDATE-ROUTE-B", ActiveDefinition("ROUTE-B-STEP"));

        var entity = await SeedApprovedEntityAsync(harness);
        entity.Status = "Suspended";
        await harness.Context.SaveChangesAsync();

        entity.WorkflowStatus.Should().Be(WorkflowStatus.InProgress);
        entity.WorkflowCode.Should().Be("UPDATE-ROUTE-B");
    }

    [Fact]
    public async Task E2E4_NoMatchingRoute_SavesWithoutWorkflow()
    {
        await using var harness = CreateHarness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(
            updateRoutes:
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "UPDATE-ROUTE-A",
                    SignificantFields = [nameof(TestWorkflowEntity.Amount)]
                }
            ]));

        var entity = await SeedApprovedEntityAsync(harness);
        var originalHistoryId = entity.CurrentWorkflowHistoryId;

        entity.Title = "Non-routed change";
        await harness.Context.SaveChangesAsync();

        entity.WorkflowStatus.Should().Be(WorkflowStatus.Approved);
        entity.WorkflowCode.Should().BeNull();
        entity.CurrentWorkflowHistoryId.Should().Be(originalHistoryId);
        harness.Bridge.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task E2E5_Priority_SelectsFirstMatchingRoute()
    {
        await using var harness = CreateHarness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(
            updateRoutes:
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "HIGH-PRIORITY-WF",
                    SignificantFields = [nameof(TestWorkflowEntity.Amount)]
                },
                new WorkflowOperationRoute
                {
                    Priority = 2,
                    WorkflowCode = "LOW-PRIORITY-WF",
                    SignificantFields = [nameof(TestWorkflowEntity.Amount), nameof(TestWorkflowEntity.Status)]
                }
            ]));
        harness.DefinitionProvider.SetDefinition("HIGH-PRIORITY-WF", ActiveDefinition("HIGH-STEP"));

        var entity = await SeedApprovedEntityAsync(harness);
        entity.Amount = 1500;
        entity.Status = "Changed";
        await harness.Context.SaveChangesAsync();

        entity.WorkflowCode.Should().Be("HIGH-PRIORITY-WF");
    }

    [Fact]
    public async Task E2E6_InProgressStatus_BlocksUpdateWorkflow()
    {
        await using var harness = CreateHarness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(
            updateRoutes:
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "UPDATE-ROUTE-A",
                    SignificantFields = [nameof(TestWorkflowEntity.Amount)]
                }
            ]));

        var entity = NewEntity();
        entity.WorkflowStatus = WorkflowStatus.InProgress;
        harness.Context.Add(entity);
        await harness.Context.SaveChangesAsync();
        harness.Bridge.Messages.Clear();

        entity.Amount = 500;
        var act = () => harness.Context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be edited*");
        harness.Bridge.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task E2E7_DeleteRoute_InitiatesDeleteWorkflow()
    {
        await using var harness = CreateHarness();
        harness.ConfigCache.SetConfig(ModuleName, CreateConfig(
            deleteRoutes:
            [
                new WorkflowOperationRoute
                {
                    Priority = 1,
                    WorkflowCode = "DELETE-ROUTE-WF"
                }
            ]));
        harness.DefinitionProvider.SetDefinition("DELETE-ROUTE-WF", ActiveDefinition("DELETE-STEP"));

        var entity = NewEntity();
        harness.Context.Add(entity);
        await harness.Context.SaveChangesAsync();

        harness.Context.Remove(entity);
        await harness.Context.SaveChangesAsync();

        entity.WorkflowStatus.Should().Be(WorkflowStatus.InProgress);
        entity.WorkflowCode.Should().Be("DELETE-ROUTE-WF");
    }

    [Fact]
    public async Task E2E8_ModuleFromAppSettings_RequiredForConfigLookup()
    {
        await using var wrongModuleHarness = CreateHarness(moduleName: "WrongModule");
        wrongModuleHarness.ConfigCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        wrongModuleHarness.DefinitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));

        var entity = NewEntity();
        wrongModuleHarness.Context.Add(entity);
        await wrongModuleHarness.Context.SaveChangesAsync();

        entity.WorkflowStatus.Should().Be(WorkflowStatus.NotStarted);
        wrongModuleHarness.Bridge.Messages.Should().BeEmpty();

        await using var correctModuleHarness = CreateHarness(moduleName: ModuleName);
        correctModuleHarness.ConfigCache.SetConfig(ModuleName, CreateConfig(enableCreate: true, createCode: "CREATE-WF"));
        correctModuleHarness.DefinitionProvider.SetDefinition("CREATE-WF", ActiveDefinition("CREATE-STEP"));

        var routedEntity = NewEntity();
        correctModuleHarness.Context.Add(routedEntity);
        await correctModuleHarness.Context.SaveChangesAsync();

        routedEntity.WorkflowStatus.Should().Be(WorkflowStatus.InProgress);
        routedEntity.WorkflowCode.Should().Be("CREATE-WF");
        correctModuleHarness.Bridge.Messages.Should().ContainSingle();
    }

    private static InterceptorE2EHarness CreateHarness(string moduleName = ModuleName) =>
        new(TenantId, moduleName);

    private static TestWorkflowEntity NewEntity()
    {
        var entity = new TestWorkflowEntity
        {
            Title = "Initial",
            Amount = 100,
            Status = "Active"
        };
        entity.EnableWorkflowProcessing();
        entity.TenantId = TenantId;
        return entity;
    }

    private static async Task<TestWorkflowEntity> SeedApprovedEntityAsync(InterceptorE2EHarness harness)
    {
        var entity = NewEntity();
        entity.EnableWorkflowAfterSeeding();
        harness.Context.Add(entity);
        await harness.Context.SaveChangesAsync();
        harness.Context.ChangeTracker.Clear();

        var reloaded = await harness.Context.Entities.FirstAsync(e => e.Id == entity.Id);
        reloaded.EnableWorkflowProcessing();
        return reloaded;
    }

    private static EntityWorkflowConfig CreateConfig(
        bool enableCreate = false,
        string? createCode = null,
        List<WorkflowOperationRoute>? updateRoutes = null,
        List<WorkflowOperationRoute>? deleteRoutes = null)
    {
        return new EntityWorkflowConfig
        {
            EnableWorkflowForCreate = enableCreate,
            CreateWorkflowCode = createCode,
            EnableWorkflowForUpdate = updateRoutes is { Count: > 0 },
            UpdateWorkflowRoutes = updateRoutes ?? [],
            EnableWorkflowForDelete = deleteRoutes is { Count: > 0 },
            DeleteWorkflowRoutes = deleteRoutes ?? []
        };
    }

    private static PublishedWorkflowDefinition ActiveDefinition(string stepCode) =>
        new()
        {
            TenantId = TenantId,
            WorkflowCode = stepCode,
            EntityType = nameof(TestWorkflowEntity),
            IsActive = true,
            Definition = new WorkflowDefinition
            {
                Steps =
                [
                    new WorkflowStep
                    {
                        StepCode = stepCode,
                        Name = stepCode,
                        Order = 1
                    }
                ]
            }
        };
}

internal sealed class InterceptorE2EHarness : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public InterceptorE2EHarness(string tenantId, string moduleName)
    {
        ConfigCache = new FakeWorkflowConfigCacheService();
        DefinitionProvider = new FakeWorkflowDefinitionProvider(tenantId);
        Bridge = new RecordingWorkflowTriggerBridge();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Workflow:Module"] = moduleName
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ICurrentUserService>(new FakeCurrentUserService(tenantId));
        services.AddSingleton<ITenantContext>(new TenantContext());
        services.AddSingleton(ConfigCache);
        services.AddSingleton<IWorkflowConfigCacheService>(sp => sp.GetRequiredService<FakeWorkflowConfigCacheService>());
        services.AddSingleton(DefinitionProvider);
        services.AddSingleton<IWorkflowDefinitionProvider>(sp => sp.GetRequiredService<FakeWorkflowDefinitionProvider>());
        services.AddSingleton(Bridge);
        services.AddSingleton<IWorkflowTriggerBridge>(sp => sp.GetRequiredService<RecordingWorkflowTriggerBridge>());
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

        var options = new DbContextOptionsBuilder<TestWorkflowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        Context = new InterceptorTestDbContext(options);
    }

    public InterceptorTestDbContext Context { get; }
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

internal sealed class InterceptorTestDbContext(DbContextOptions<TestWorkflowDbContext> options)
    : TestWorkflowDbContext(options);

internal sealed class FakeCurrentUserService(string tenantId) : ICurrentUserService
{
    public bool IsAuthenticated => true;

    public string GetCorrelationId() => "test-correlation";
    public string GetUserId() => "test-user-id";
    public string? GetRole() => "Administrator";
    public List<string> GetUserRoles() => ["Administrator"];
    public string GetTenantId() => tenantId;
    public string? GetToken() => null;
    public IEnumerable<System.Security.Claims.Claim> GetClaims() => [];
    public string GetUserEmail() => "tester@qimerp.com";
    public string GetUserName() => "Tester";
    public string? GetDomainName() => null;
    public string? GetLanguage() => null;
    public string? GetTimeZone() => null;
    public string? GetCompanyName() => null;
    public string? GetEmployeeId() => null;
    public string? GetRankId() => null;
    public string? GetRankName() => null;
    public string? GetOrganizationalUnitId() => null;
    public string? GetOrganizationalUnitName() => null;
    public List<string> GetRoleIds() => [];
}

internal sealed class FakeWorkflowConfigCacheService : IWorkflowConfigCacheService
{
    private readonly Dictionary<(string Module, string EntityType), EntityWorkflowConfig> _configs = new();

    public void SetConfig(string module, EntityWorkflowConfig config, string entityType = nameof(TestWorkflowEntity))
    {
        _configs[(module, entityType)] = config;
    }

    public Task<EntityWorkflowConfig?> GetEntityConfigAsync(string module, string entityType, string? tenantId = null)
    {
        _configs.TryGetValue((module, entityType), out var config);
        return Task.FromResult<EntityWorkflowConfig?>(config);
    }

    public Task<bool> IsWorkflowEnabledAsync(string module, string entityType, string operation, string? tenantId = null)
    {
        if (!_configs.TryGetValue((module, entityType), out var config))
            return Task.FromResult(false);

        return Task.FromResult(operation.ToUpperInvariant() switch
        {
            "CREATE" => config.EnableWorkflowForCreate,
            "UPDATE" => config.EnableWorkflowForUpdate,
            "DELETE" => config.EnableWorkflowForDelete,
            _ => false
        });
    }

    public Task<string?> GetWorkflowCodeAsync(string module, string entityType, string operation, string? tenantId = null)
    {
        if (!_configs.TryGetValue((module, entityType), out var config))
            return Task.FromResult<string?>(null);

        return Task.FromResult(operation.ToUpperInvariant() switch
        {
            "CREATE" => config.CreateWorkflowCode,
            _ => null
        });
    }

    public Task<List<WorkflowTriggerCondition>> GetTriggerConditionsAsync(
        string module, string entityType, string operation, string? tenantId = null) =>
        Task.FromResult(new List<WorkflowTriggerCondition>());
}

internal sealed class FakeWorkflowDefinitionProvider(string tenantId) : IWorkflowDefinitionProvider
{
    private readonly Dictionary<string, PublishedWorkflowDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void SetDefinition(string workflowCode, PublishedWorkflowDefinition definition)
    {
        definition.TenantId = tenantId;
        definition.WorkflowCode = workflowCode;
        definition.EntityType ??= nameof(TestWorkflowEntity);
        _definitions[workflowCode] = definition;
    }

    public Task<PublishedWorkflowDefinition?> GetPublishedDefinitionAsync(
        string tenantId,
        string workflowCode,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        _definitions.TryGetValue(workflowCode, out var definition);
        return Task.FromResult(definition);
    }

    public Task<PublishedWorkflowDefinition?> GetPublishedDefinitionByEntityTypeAsync(
        string tenantId,
        string entityType,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<PublishedWorkflowDefinition?>(null);
}

internal sealed class RecordingWorkflowTriggerBridge : IWorkflowTriggerBridge
{
    public List<WorkflowEventMessage> Messages { get; } = [];

    public Task<bool> TryTriggerTemporalWorkflowAsync(
        WorkflowEventMessage message,
        CancellationToken cancellationToken = default)
    {
        Messages.Add(message);
        return Task.FromResult(true);
    }
}
