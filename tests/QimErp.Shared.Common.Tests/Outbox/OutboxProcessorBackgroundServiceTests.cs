using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.Workflow;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

public sealed class OutboxProcessorBackgroundServiceTests
{
    private const string TenantId = "processor-tenant-1";

    private sealed record TestDomainEvent(string Payload) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string TenantId { get; init; } = OutboxProcessorBackgroundServiceTests.TenantId;
        public string? TriggeredBy { get; } = "tester";
    }

    /// <summary>Singleton spy shared across DI scopes so the test can observe what the scoped
    /// <see cref="RecordingPublisher"/> saw on each call, even though a new scoped instance of
    /// the publisher itself is created per outbox row.</summary>
    private sealed class PublishSpy
    {
        public List<(object Notification, string? TenantIdDuringPublish, CompanyScope CompanyScopeDuringPublish)> Records { get; } = [];
    }

    private sealed class RecordingPublisher(PublishSpy spy, ICurrentUserService? userContext = null) : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            spy.Records.Add((notification, userContext?.GetTenantId(), CompanyContext.CurrentScope));
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            spy.Records.Add((notification!, userContext?.GetTenantId(), CompanyContext.CurrentScope));
            return Task.CompletedTask;
        }
    }

    /// <summary>Singleton spy shared across DI scopes — mirrors <see cref="PublishSpy"/> — because
    /// the processor creates a new scope (and therefore a new scoped <see cref="RecordingBridge"/>)
    /// per outbox row.</summary>
    private sealed class BridgeSpy
    {
        public List<WorkflowEventMessage> Messages { get; } = [];
        public List<CompanyScope> CompanyScopesSeen { get; } = [];
    }

    private sealed class RecordingBridge(BridgeSpy spy) : IWorkflowTriggerBridge
    {
        public bool ShouldSucceed { get; set; } = true;

        public Task<bool> TryTriggerTemporalWorkflowAsync(
            WorkflowEventMessage message, CancellationToken cancellationToken = default)
        {
            spy.Messages.Add(message);
            spy.CompanyScopesSeen.Add(CompanyContext.CurrentScope);
            return Task.FromResult(ShouldSucceed);
        }
    }

    private sealed class CountingScopeFactory(IServiceScopeFactory inner) : IServiceScopeFactory
    {
        public int CreateScopeCallCount { get; private set; }

        public IServiceScope CreateScope()
        {
            CreateScopeCallCount++;
            return inner.CreateScope();
        }
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly ServiceProvider _root;
        public CountingScopeFactory ScopeFactory { get; }
        public string DbName { get; } = Guid.NewGuid().ToString();

        public Harness(bool withBridge = true, bool withPublisher = true, bool bridgeSucceeds = true)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ITenantScopeSetter>(sp => (ITenantScopeSetter)sp.GetRequiredService<ITenantContext>());
            services.AddScoped<ICompanyContext, CompanyContext>();
            services.AddScoped<ICompanyScopeSetter>(sp => (ICompanyScopeSetter)sp.GetRequiredService<ICompanyContext>());
            services.AddScoped<ICurrentUserService, UserContextService>();
            services.AddHttpContextAccessor();

            services.AddDbContext<TestApplicationDbContext>(options =>
                options.UseInMemoryDatabase(DbName));

            services.AddSingleton<BridgeSpy>();
            if (withBridge)
                services.AddScoped<IWorkflowTriggerBridge>(sp =>
                    new RecordingBridge(sp.GetRequiredService<BridgeSpy>()) { ShouldSucceed = bridgeSucceeds });

            services.AddSingleton<PublishSpy>();
            if (withPublisher)
                services.AddScoped<IPublisher>(sp =>
                    new RecordingPublisher(sp.GetRequiredService<PublishSpy>(), sp.GetService<ICurrentUserService>()));

            _root = services.BuildServiceProvider();
            ScopeFactory = new CountingScopeFactory(_root.GetRequiredService<IServiceScopeFactory>());
        }

        public PublishSpy Spy => _root.GetRequiredService<PublishSpy>();
        public List<WorkflowEventMessage> BridgeMessages => _root.GetRequiredService<BridgeSpy>().Messages;
        public List<CompanyScope> BridgeCompanyScopesSeen => _root.GetRequiredService<BridgeSpy>().CompanyScopesSeen;

        public async Task SeedAsync(string eventType, string payload, bool isProcessed = false)
        {
            using var scope = _root.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TestApplicationDbContext>();
            db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = eventType,
                Payload = payload,
                IsProcessed = isProcessed
            });
            await db.SaveChangesAsync();
        }

        public async Task<List<OutboxMessage>> GetAllAsync()
        {
            using var scope = _root.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TestApplicationDbContext>();
            return await db.OutboxMessages.ToListAsync();
        }

        public OutboxProcessorBackgroundService<TestApplicationDbContext> CreateProcessor(int batchSize = 50) =>
            new(ScopeFactory,
                NullLogger<OutboxProcessorBackgroundService<TestApplicationDbContext>>.Instance,
                Microsoft.Extensions.Options.Options.Create(
                    new OutboxProcessorOptions { BatchSize = batchSize, PollingIntervalSeconds = 30 }));

        public ValueTask DisposeAsync()
        {
            return _root.DisposeAsync();
        }
    }

    private static string WorkflowEventTypeName() => typeof(WorkflowEventMessage).AssemblyQualifiedName!;
    private static string DomainEventTypeName() => typeof(TestDomainEvent).AssemblyQualifiedName!;

    [Fact(DisplayName = "10: WorkflowEventMessage row is routed to IWorkflowTriggerBridge and marked processed on success")]
    public async Task WorkflowEventMessage_RoutesToBridge_MarksProcessed()
    {
        await using var harness = new Harness();
        var message = new WorkflowEventMessage { EntityType = "X", WorkflowCode = "WF-1", TenantId = TenantId };
        var payload = OutboxEnvelope.Wrap(message, TenantId);
        await harness.SeedAsync(WorkflowEventTypeName(), payload);

        var processor = harness.CreateProcessor();
        var processed = await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        processed.Should().Be(1);
        var row = (await harness.GetAllAsync()).Single();
        row.IsProcessed.Should().BeTrue();
    }

    [Fact(DisplayName = "11: IDomainEvent row is routed to IPublisher and marked processed on success")]
    public async Task DomainEvent_RoutesToPublisher_MarksProcessed()
    {
        await using var harness = new Harness();
        var evt = new TestDomainEvent("hello");
        var payload = OutboxEnvelope.Wrap(evt, TenantId);
        await harness.SeedAsync(DomainEventTypeName(), payload);

        var processor = harness.CreateProcessor();
        var processed = await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        processed.Should().Be(1);
        var row = (await harness.GetAllAsync()).Single();
        row.IsProcessed.Should().BeTrue();
    }

    [Fact(DisplayName = "12: Tenant from envelope is seeded on ICurrentUserService before dispatch and cleared after")]
    public async Task SeedsTenantContext_BeforeDispatch_ClearsAfter()
    {
        await using var harness = new Harness();
        var evt = new TestDomainEvent("hello");
        var payload = OutboxEnvelope.Wrap(evt, TenantId);
        await harness.SeedAsync(DomainEventTypeName(), payload);

        var processor = harness.CreateProcessor();
        await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        harness.Spy.Records.Should().ContainSingle();
        harness.Spy.Records[0].TenantIdDuringPublish.Should().Be(TenantId,
            "the envelope's TenantId must be set on ICurrentUserService before IPublisher.Publish is called");

        using var verifyScope = harness.ScopeFactory.CreateScope();
        verifyScope.ServiceProvider.GetRequiredService<ICurrentUserService>().GetTenantId()
            .Should().BeEmpty("tenant context must not leak outside the per-row scope that dispatched it");
    }

    [Fact(DisplayName = "13: Dispatch failure (no bridge registered) leaves the row unprocessed without throwing")]
    public async Task MissingBridge_LeavesRowUnprocessed_NoThrow()
    {
        await using var harness = new Harness(withBridge: false);
        var message = new WorkflowEventMessage { EntityType = "X", WorkflowCode = "WF-1", TenantId = TenantId };
        var payload = OutboxEnvelope.Wrap(message, TenantId);
        await harness.SeedAsync(WorkflowEventTypeName(), payload);

        var processor = harness.CreateProcessor();
        var act = async () => await processor.ProcessPendingMessagesAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();

        var row = (await harness.GetAllAsync()).Single();
        row.IsProcessed.Should().BeFalse();
    }

    [Fact(DisplayName = "14: Bridge returning false leaves the row unprocessed for redelivery")]
    public async Task BridgeReturnsFalse_LeavesRowUnprocessed()
    {
        await using var harness = new Harness(bridgeSucceeds: false);
        var message = new WorkflowEventMessage { EntityType = "X", WorkflowCode = "WF-1", TenantId = TenantId };
        var payload = OutboxEnvelope.Wrap(message, TenantId);
        await harness.SeedAsync(WorkflowEventTypeName(), payload);

        var processor = harness.CreateProcessor();
        var processed = await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        processed.Should().Be(0);
        var row = (await harness.GetAllAsync()).Single();
        row.IsProcessed.Should().BeFalse();
    }

    [Fact(DisplayName = "15: Already-processed rows are skipped")]
    public async Task AlreadyProcessedRows_AreSkipped()
    {
        await using var harness = new Harness();
        var message = new WorkflowEventMessage { EntityType = "X", WorkflowCode = "WF-1", TenantId = TenantId };
        var payload = OutboxEnvelope.Wrap(message, TenantId);
        await harness.SeedAsync(WorkflowEventTypeName(), payload, isProcessed: true);

        var processor = harness.CreateProcessor();
        var processed = await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        processed.Should().Be(0);
    }

    [Fact(DisplayName = "16: BatchSize caps the number of rows processed per sweep")]
    public async Task BatchSize_CapsRowsProcessedPerSweep()
    {
        await using var harness = new Harness();
        for (var i = 0; i < 5; i++)
        {
            var evt = new TestDomainEvent($"item-{i}");
            await harness.SeedAsync(DomainEventTypeName(), OutboxEnvelope.Wrap(evt, TenantId));
        }

        var processor = harness.CreateProcessor(batchSize: 2);
        var processed = await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        processed.Should().Be(2);
        (await harness.GetAllAsync()).Count(r => r.IsProcessed).Should().Be(2);
    }

    [Fact(DisplayName = "17: Each row is processed in its own DI scope (scope-per-row)")]
    public async Task ProcessesEachRow_InItsOwnScope()
    {
        await using var harness = new Harness();
        for (var i = 0; i < 3; i++)
        {
            var evt = new TestDomainEvent($"item-{i}");
            await harness.SeedAsync(DomainEventTypeName(), OutboxEnvelope.Wrap(evt, TenantId));
        }

        var processor = harness.CreateProcessor();
        var before = harness.ScopeFactory.CreateScopeCallCount;
        await processor.ProcessPendingMessagesAsync(CancellationToken.None);
        var scopesCreated = harness.ScopeFactory.CreateScopeCallCount - before;

        // 1 scope to fetch the pending-id batch + 1 scope per row dispatched.
        scopesCreated.Should().Be(1 + 3);
    }

    [Fact(DisplayName = "18: An unresolvable EventType is skipped without throwing")]
    public async Task UnresolvableEventType_IsSkipped_NoThrow()
    {
        await using var harness = new Harness();
        await harness.SeedAsync("Not.A.Real.Type, NotAnAssembly", OutboxEnvelope.Wrap(new TestDomainEvent("x"), TenantId));

        var processor = harness.CreateProcessor();
        var act = async () => await processor.ProcessPendingMessagesAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();

        var row = (await harness.GetAllAsync()).Single();
        row.IsProcessed.Should().BeFalse();
    }

    [Fact(DisplayName = "19: Envelope CompanyId seeds an ACTIVE company filter (not Inactive) before IWorkflowTriggerBridge dispatch, and clears after")]
    public async Task EnvelopeCompanyId_SeedsActiveCompanyScope_BeforeBridgeDispatch()
    {
        await using var harness = new Harness();
        var message = new WorkflowEventMessage { EntityType = "X", WorkflowCode = "WF-1", TenantId = TenantId };
        var payload = OutboxEnvelope.Wrap(message, TenantId, companyId: "company-A");
        await harness.SeedAsync(WorkflowEventTypeName(), payload);

        var processor = harness.CreateProcessor();
        await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        harness.BridgeCompanyScopesSeen.Should().ContainSingle();
        var scopeSeen = harness.BridgeCompanyScopesSeen[0];
        scopeSeen.Should().NotBe(CompanyScope.Inactive,
            "redelivery must apply the same company isolation the original save did, not the no-filter default");
        scopeSeen.FilterActive.Should().BeTrue();
        scopeSeen.ActiveCompanyId.Should().Be("company-A");

        CompanyContext.CurrentScope.Should().Be(CompanyScope.Inactive,
            "company scope must be cleared after dispatch, not leaked to the caller");
    }

    [Fact(DisplayName = "20: Two rows for different companies each dispatch with their OWN active, isolated company scope")]
    public async Task DifferentCompanyRows_EachDispatch_WithIsolatedActiveCompanyScope()
    {
        await using var harness = new Harness();
        var messageA = new WorkflowEventMessage { EntityType = "X", WorkflowCode = "WF-1", TenantId = TenantId };
        var messageB = new WorkflowEventMessage { EntityType = "Y", WorkflowCode = "WF-2", TenantId = TenantId };
        await harness.SeedAsync(WorkflowEventTypeName(), OutboxEnvelope.Wrap(messageA, TenantId, companyId: "company-A"));
        await harness.SeedAsync(WorkflowEventTypeName(), OutboxEnvelope.Wrap(messageB, TenantId, companyId: "company-B"));

        var processor = harness.CreateProcessor();
        var processed = await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        processed.Should().Be(2);
        harness.BridgeCompanyScopesSeen.Should().HaveCount(2);
        harness.BridgeCompanyScopesSeen.Should().NotContain(
            CompanyScope.Inactive, "each row must dispatch under an active company filter, never the no-filter default");
        harness.BridgeCompanyScopesSeen.Should().OnlyContain(s => s.FilterActive);
        harness.BridgeCompanyScopesSeen.Select(s => s.ActiveCompanyId)
            .Should().BeEquivalentTo(["company-A", "company-B"]);
        harness.BridgeCompanyScopesSeen[0].AllowedCompanyIds.Should().BeEquivalentTo(new[] { "company-A", "" });
        harness.BridgeCompanyScopesSeen[1].AllowedCompanyIds.Should().BeEquivalentTo(new[] { "company-B", "" });

        CompanyContext.CurrentScope.Should().Be(CompanyScope.Inactive,
            "company scope must be cleared after the sweep finishes, not leaked to the caller");
    }

    [Fact(DisplayName = "21: Envelope with no CompanyId (tenant-shared write) dispatches with the company filter Inactive, not a fabricated company")]
    public async Task EnvelopeWithoutCompanyId_DispatchesWithInactiveCompanyFilter()
    {
        await using var harness = new Harness();
        var message = new WorkflowEventMessage { EntityType = "X", WorkflowCode = "WF-1", TenantId = TenantId };
        await harness.SeedAsync(WorkflowEventTypeName(), OutboxEnvelope.Wrap(message, TenantId));

        var processor = harness.CreateProcessor();
        await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        harness.BridgeCompanyScopesSeen.Should().ContainSingle();
        harness.BridgeCompanyScopesSeen[0].FilterActive.Should().BeFalse(
            "a tenant-shared write (no CompanyId on the envelope) must not seed an active company filter on redelivery");
    }

    [Fact(DisplayName = "22: IDomainEvent dispatch (IPublisher path) also seeds the active company scope from the envelope")]
    public async Task DomainEventDispatch_AlsoSeedsActiveCompanyScope()
    {
        await using var harness = new Harness();
        var evt = new TestDomainEvent("hello");
        await harness.SeedAsync(DomainEventTypeName(), OutboxEnvelope.Wrap(evt, TenantId, companyId: "company-A"));

        var processor = harness.CreateProcessor();
        await processor.ProcessPendingMessagesAsync(CancellationToken.None);

        harness.Spy.Records.Should().ContainSingle();
        var scopeSeen = harness.Spy.Records[0].CompanyScopeDuringPublish;
        scopeSeen.FilterActive.Should().BeTrue();
        scopeSeen.ActiveCompanyId.Should().Be("company-A");
    }

    [Fact(DisplayName = "23: A disallowed resolved type is never passed to JsonSerializer.Deserialize")]
    public async Task DisallowedResolvedType_IsNeverDeserialized_RowStaysUnprocessed()
    {
        await using var harness = new Harness();
        // Resolvable via Type.GetType but neither WorkflowEventMessage nor IDomainEvent — the
        // allow-list must reject it before Deserialize ever runs.
        var disallowedType = typeof(NotAllowListedPayload).AssemblyQualifiedName!;
        await harness.SeedAsync(disallowedType, OutboxEnvelope.Wrap(new NotAllowListedPayload { Value = "x" }, TenantId));

        var processor = harness.CreateProcessor();
        var act = async () => await processor.ProcessPendingMessagesAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();

        var row = (await harness.GetAllAsync()).Single();
        row.IsProcessed.Should().BeFalse();
    }

    public sealed class NotAllowListedPayload
    {
        public string Value { get; set; } = string.Empty;
    }
}
