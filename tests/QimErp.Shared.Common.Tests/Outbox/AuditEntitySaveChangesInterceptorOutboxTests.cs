using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Services;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Tests.Workflow;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

/// <summary>
/// Covers the SDK-only outbox backstop added to <see cref="AuditEntitySaveChangesInterceptor"/>
/// for plain domain events (the MediatR / <see cref="IPublisher"/> post-commit path). The
/// Temporal/<see cref="QimErp.Shared.Common.Services.Workflow.IWorkflowTriggerBridge"/> pre-commit
/// path is covered separately in <see cref="OutboxWorkflowBridgeTests"/>.
/// </summary>
public sealed class AuditEntitySaveChangesInterceptorOutboxTests
{
    private const string TenantId = "outbox-tenant-1";

    private sealed record TestDomainEvent(string Payload) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string TenantId { get; init; } = AuditEntitySaveChangesInterceptorOutboxTests.TenantId;
        public string? TriggeredBy { get; } = "tester";
    }

    private sealed class RecordingPublisher : IPublisher
    {
        public List<object> Published { get; } = [];
        public bool ThrowOnPublish { get; set; }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            if (ThrowOnPublish) throw new InvalidOperationException("boom");
            Published.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            if (ThrowOnPublish) throw new InvalidOperationException("boom");
            Published.Add(notification!);
            return Task.CompletedTask;
        }
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly ServiceProvider _sp;
        public TestApplicationDbContext Db { get; }
        public RecordingPublisher Publisher { get; } = new();

        public Harness(bool withPublisher = true)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            _sp = services.BuildServiceProvider();

            var tenantContext = new TenantContext();
            var interceptor = new AuditEntitySaveChangesInterceptor(
                new FakeCurrentUserService(TenantId),
                NullLogger<AuditEntitySaveChangesInterceptor>.Instance,
                _sp,
                configuration: null,
                publisher: withPublisher ? Publisher : null);

            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            Db = new TestApplicationDbContext(options, tenantContext);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _sp.DisposeAsync();
        }
    }

    private static EntityCodeConfig NewEntityWithDomainEvent(string payload = "p1") =>
        EntityCodeConfig.Create(string.Empty, $"OutboxTest-{Guid.NewGuid():N}")
            .WithDomainEvent(new TestDomainEvent(payload));

    [Fact(DisplayName = "1: A domain event raised on save gets an OutboxMessage row in the SAME SaveChanges call")]
    public async Task EnqueuesOutboxMessage_InSameSaveChangesCall()
    {
        await using var harness = new Harness();
        var entity = NewEntityWithDomainEvent();
        harness.Db.Add(entity);

        await harness.Db.SaveChangesAsync();

        var rows = await harness.Db.OutboxMessages.ToListAsync();
        rows.Should().ContainSingle();
    }

    [Fact(DisplayName = "2: Outbox payload wraps TenantId + serialized event data")]
    public async Task OutboxPayload_ContainsEnvelopeWithTenantAndData()
    {
        await using var harness = new Harness();
        var entity = NewEntityWithDomainEvent("hello");
        harness.Db.Add(entity);
        await harness.Db.SaveChangesAsync();

        var row = await harness.Db.OutboxMessages.SingleAsync();
        var envelope = OutboxEnvelope.Unwrap(row.Payload);

        envelope.Should().NotBeNull();
        envelope!.TenantId.Should().Be(TenantId);
        envelope.Data.Should().Contain("hello");
        row.EventType.Should().NotBeNullOrWhiteSpace();
        Type.GetType(row.EventType!).Should().NotBeNull("EventType must be resolvable via Type.GetType for the processor to deserialize it");
    }

    [Fact(DisplayName = "3: After successful post-commit IPublisher.Publish, OutboxMessage.IsProcessed becomes true")]
    public async Task SuccessfulPublish_MarksOutboxMessageProcessed()
    {
        await using var harness = new Harness();
        var entity = NewEntityWithDomainEvent();
        harness.Db.Add(entity);

        await harness.Db.SaveChangesAsync();

        harness.Publisher.Published.Should().ContainSingle();

        var row = await harness.Db.OutboxMessages.SingleAsync();
        row.IsProcessed.Should().BeTrue();
    }

    [Fact(DisplayName = "4: Immediately after SaveChangesAsync returns the row is durably committed, even before IsProcessed is marked")]
    public async Task OutboxRow_IsDurable_BeforeMarkingProcessed()
    {
        // Regression guard for "enqueue in SavingChanges (same txn)": even if nothing published
        // the row must already be queryable the instant the outer SaveChangesAsync call returns.
        await using var harness = new Harness(withPublisher: false);
        var entity = NewEntityWithDomainEvent();
        harness.Db.Add(entity);

        await harness.Db.SaveChangesAsync();

        var row = await harness.Db.OutboxMessages.SingleAsync();
        row.IsProcessed.Should().BeFalse("no publisher was registered, so nothing could have dispatched it yet");
    }

    [Fact(DisplayName = "5: When IPublisher throws for a non-critical event, StoreFailedEventsAsync is log-only — OutboxMessage stays unprocessed and SaveChanges does not throw")]
    public async Task FailedPublish_IsLogOnly_OutboxStaysUnprocessed()
    {
        await using var harness = new Harness();
        harness.Publisher.ThrowOnPublish = true;
        var entity = NewEntityWithDomainEvent();
        harness.Db.Add(entity);

        var act = async () => await harness.Db.SaveChangesAsync();
        await act.Should().NotThrowAsync("a non-critical publish failure is logged, not thrown");

        var row = await harness.Db.OutboxMessages.SingleAsync();
        row.IsProcessed.Should().BeFalse("the backstop processor must redeliver this event later");
    }

    [Fact(DisplayName = "6: Saving an entity with no domain events writes no OutboxMessage row")]
    public async Task NoDomainEvents_NoOutboxRow()
    {
        await using var harness = new Harness();
        var entity = EntityCodeConfig.Create(string.Empty, $"Plain-{Guid.NewGuid():N}");
        harness.Db.Add(entity);

        await harness.Db.SaveChangesAsync();

        (await harness.Db.OutboxMessages.ToListAsync()).Should().BeEmpty();
    }

    [Fact(DisplayName = "7: Outbox envelope carries CompanyId from the ambient CompanyContext scope at enqueue time")]
    public async Task OutboxEnvelope_CarriesCompanyId_FromAmbientCompanyScope()
    {
        await using var harness = new Harness();
        var companyContext = new CompanyContext();
        companyContext.SetScope(CompanyScope.ForCompanies(["company-A"], "company-A"));
        try
        {
            var entity = NewEntityWithDomainEvent();
            harness.Db.Add(entity);
            await harness.Db.SaveChangesAsync();
        }
        finally
        {
            companyContext.Clear();
        }

        var row = await harness.Db.OutboxMessages.SingleAsync();
        var envelope = OutboxEnvelope.Unwrap(row.Payload);
        envelope!.CompanyId.Should().Be("company-A",
            "OutboxProcessorBackgroundService must redeliver this message under the same company scope");
    }

    [Fact(DisplayName = "8: Outbox envelope CompanyId falls back to the entity's stamped CompanyId when no single company is ambiently active")]
    public async Task OutboxEnvelope_CompanyId_FallsBackToEntityStamp()
    {
        await using var harness = new Harness();
        var companyContext = new CompanyContext();
        // Multi-company on, no active selection: SetCompanyIdOnEntities falls through to the
        // CompanyStampScope override below rather than the ambient scope's EffectiveCompanyId.
        companyContext.SetScope(CompanyScope.AllCompanies(active: null));
        try
        {
            using (CompanyStampScope.Enter("company-B"))
            {
                var entity = NewEntityWithDomainEvent();
                harness.Db.Add(entity);
                await harness.Db.SaveChangesAsync();
            }
        }
        finally
        {
            companyContext.Clear();
        }

        var row = await harness.Db.OutboxMessages.SingleAsync();
        var envelope = OutboxEnvelope.Unwrap(row.Payload);
        envelope!.CompanyId.Should().Be("company-B");
    }

    [Fact(DisplayName = "9: Outbox envelope CompanyId is empty for a tenant-shared write (multi-company disabled)")]
    public async Task OutboxEnvelope_CompanyId_EmptyForTenantSharedWrite()
    {
        await using var harness = new Harness();
        // Default CompanyContext scope (CompanyScope.Inactive) ⇒ MultiCompanyEnabled=false ⇒
        // SetCompanyIdOnEntities stamps every entity as tenant-shared ("").
        var entity = NewEntityWithDomainEvent();
        harness.Db.Add(entity);
        await harness.Db.SaveChangesAsync();

        var row = await harness.Db.OutboxMessages.SingleAsync();
        var envelope = OutboxEnvelope.Unwrap(row.Payload);
        envelope!.CompanyId.Should().BeEmpty(
            "a tenant-shared write must never fabricate a company on redelivery");
    }
}

file static class EntityCodeConfigTestExtensions
{
    public static EntityCodeConfig WithDomainEvent(this EntityCodeConfig entity, IDomainEvent domainEvent)
    {
        entity.AddDomainEvent(domainEvent);
        return entity;
    }
}
