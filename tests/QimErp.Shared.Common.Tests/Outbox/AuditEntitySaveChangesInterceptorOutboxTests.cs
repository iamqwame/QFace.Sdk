using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Interceptors;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Tests.Workflow;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

/// <summary>
/// Covers DATA-09: domain events raised on save are written as <see cref="OutboxMessage"/> rows
/// in the same SaveChanges transaction via <see cref="IOutbox"/>. Delivery is the Akka relay's job.
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

    /// <summary>Binds <see cref="IOutbox"/> to the harness DbContext after it is constructed.</summary>
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
            _inner ?? throw new InvalidOperationException("Outbox not bound to a DbContext.");
    }

    private sealed class Harness : IAsyncDisposable
    {
        private readonly ServiceProvider _sp;
        public TestApplicationDbContext Db { get; }

        public Harness()
        {
            var bound = new BoundOutbox();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IOutbox>(bound);
            services.Configure<OutboxOptions>(_ => { });
            _sp = services.BuildServiceProvider();

            var tenantContext = new TenantContext();
            var interceptor = new AuditEntitySaveChangesInterceptor(
                new FakeCurrentUserService(TenantId),
                NullLogger<AuditEntitySaveChangesInterceptor>.Instance,
                _sp,
                configuration: null);

            var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            Db = new TestApplicationDbContext(options, tenantContext);
            var routing = new TransportRouting(_sp.GetRequiredService<IOptions<OutboxOptions>>());
            bound.Bind(Db, routing, TimeProvider.System);
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
        rows[0].ProcessedAt.Should().BeNull("the Akka relay marks ProcessedAt after delivery");
        rows[0].Transport.Should().Be(OutboxTransports.InProcess);
    }

    [Fact(DisplayName = "2: Outbox row carries TenantId + serialized event payload")]
    public async Task OutboxPayload_ContainsTenantAndEventData()
    {
        await using var harness = new Harness();
        var entity = NewEntityWithDomainEvent("hello");
        harness.Db.Add(entity);
        await harness.Db.SaveChangesAsync();

        var row = await harness.Db.OutboxMessages.SingleAsync();
        row.TenantId.Should().Be(TenantId);
        row.EventType.Should().Be(nameof(TestDomainEvent));
        row.Payload.Should().Contain("hello");
    }

    [Fact(DisplayName = "3: Immediately after SaveChanges the row is unprocessed (relay has not run)")]
    public async Task OutboxRow_StaysUnprocessed_UntilRelayDelivers()
    {
        await using var harness = new Harness();
        var entity = NewEntityWithDomainEvent();
        harness.Db.Add(entity);

        await harness.Db.SaveChangesAsync();

        var row = await harness.Db.OutboxMessages.SingleAsync();
        row.ProcessedAt.Should().BeNull();
#pragma warning disable CS0618
        row.IsProcessed.Should().BeFalse();
#pragma warning restore CS0618
    }

    [Fact(DisplayName = "4: Outbox row is queryable the instant SaveChangesAsync returns")]
    public async Task OutboxRow_IsDurable_BeforeRelay()
    {
        await using var harness = new Harness();
        var entity = NewEntityWithDomainEvent();
        harness.Db.Add(entity);

        await harness.Db.SaveChangesAsync();

        var row = await harness.Db.OutboxMessages.AsNoTracking().SingleAsync();
        row.Id.Should().NotBeEmpty();
        row.ProcessedAt.Should().BeNull();
    }

    [Fact(DisplayName = "5: Saving an entity with no domain events writes no OutboxMessage row")]
    public async Task NoDomainEvents_NoOutboxRow()
    {
        await using var harness = new Harness();
        var entity = EntityCodeConfig.Create(string.Empty, $"Plain-{Guid.NewGuid():N}");
        harness.Db.Add(entity);

        await harness.Db.SaveChangesAsync();

        (await harness.Db.OutboxMessages.ToListAsync()).Should().BeEmpty();
    }

    [Fact(DisplayName = "6: Outbox TenantId comes from the domain event")]
    public async Task OutboxTenantId_ComesFromDomainEvent()
    {
        await using var harness = new Harness();
        var entity = NewEntityWithDomainEvent("tenant-check");
        harness.Db.Add(entity);
        await harness.Db.SaveChangesAsync();

        var row = await harness.Db.OutboxMessages.SingleAsync();
        row.TenantId.Should().Be(TenantId);
        row.Payload.Should().Contain("tenant-check");
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
