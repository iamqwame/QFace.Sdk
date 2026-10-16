using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Events;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

public class TransactionalOutboxTests
{
    private sealed class SampleDomainEvent : DomainEvent
    {
        public Guid EntityId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task IOutbox_Add_WritesRowInSameDbContext_BeforeSaveChanges()
    {
        await using var provider = BuildProvider(out var tenant);
        tenant.SetTenant("tenant-a");

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TestApplicationDbContext>();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();

        var entityId = Guid.CreateVersion7();
        outbox.Add(new SampleDomainEvent
        {
            TenantId = "tenant-a",
            EntityId = entityId,
            Name = "renamed",
        }, entityId, entityVersion: 3);

        db.ChangeTracker.Entries<OutboxMessage>().Should().HaveCount(1);
        var pending = db.ChangeTracker.Entries<OutboxMessage>().Single().Entity;
        pending.EventType.Should().Be(nameof(SampleDomainEvent));
        pending.Transport.Should().Be(OutboxTransports.InProcess);
        pending.EntityId.Should().Be(entityId);
        pending.EntityVersion.Should().Be(3);
        pending.ProcessedAt.Should().BeNull();
        pending.Payload.Should().Contain("renamed");

        await db.SaveChangesAsync();

        var stored = await db.OutboxMessages.AsNoTracking().SingleAsync();
        stored.Id.Should().Be(pending.Id);
        stored.TenantId.Should().Be("tenant-a");
    }

    [Fact]
    public async Task OutboxStore_ClaimAndMarkProcessed_WorksOnInMemory()
    {
        await using var provider = BuildProvider(out var tenant);
        tenant.SetTenant("tenant-b");

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TestApplicationDbContext>();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();
        var store = scope.ServiceProvider.GetRequiredService<OutboxStore>();

        var entityId = Guid.CreateVersion7();
        outbox.Add("SampleDomainEvent", "tenant-b", entityId, 1, new SampleDomainEvent
        {
            TenantId = "tenant-b",
            EntityId = entityId,
            Name = "x",
        });
        await db.SaveChangesAsync();

        var claimed = await store.ClaimAsync("test-node", batchSize: 10, lease: TimeSpan.FromSeconds(30), CancellationToken.None);
        claimed.Should().HaveCount(1);
        claimed[0].LockedBy.Should().Be("test-node");

        await store.MarkProcessedAsync(claimed[0].Id, CancellationToken.None);

        var row = await db.OutboxMessages.AsNoTracking().SingleAsync();
        row.ProcessedAt.Should().NotBeNull();
        row.LockedBy.Should().BeNull();
    }

    [Fact]
    public void TransportRouting_WorkflowApprovalRequired_DefaultsToTemporal()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransactionalOutbox<TestApplicationDbContext>();
        using var provider = services.BuildServiceProvider();

        var routing = provider.GetRequiredService<ITransportRouting>();
        routing.TransportsFor("WorkflowApprovalRequired").Should().Equal(OutboxTransports.Temporal);
        routing.TransportsFor("EmployeeChangedEvent").Should().Equal(OutboxTransports.InProcess);
    }

    [Fact]
    public void AddTransactionalOutbox_RegistersIOutboxAndStore()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITenantContext, TenantContext>();
        services.AddDbContext<TestApplicationDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddTransactionalOutbox<TestApplicationDbContext>();

        using var scope = services.BuildServiceProvider().CreateScope();
        scope.ServiceProvider.GetService<IOutbox>().Should().NotBeNull();
        scope.ServiceProvider.GetService<OutboxStore>().Should().NotBeNull();
        scope.ServiceProvider.GetServices<IEventTransport>().Select(t => t.Name)
            .Should().Contain([OutboxTransports.InProcess, OutboxTransports.Temporal]);
    }

    private static ServiceProvider BuildProvider(out TenantContext tenant)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        tenant = new TenantContext();
        services.AddSingleton<ITenantContext>(tenant);
        services.AddDbContext<TestApplicationDbContext>(o =>
            o.UseInMemoryDatabase("outbox-" + Guid.NewGuid().ToString("N")));
        services.AddTransactionalOutbox<TestApplicationDbContext>();
        return services.BuildServiceProvider();
    }
}
