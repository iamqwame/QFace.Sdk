using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Outbox.Transports;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using Xunit;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Tests.Outbox;

public class TemporalEventTransportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Specialized_handler_is_resolved_and_dispatched_with_tenant_and_cleared_afterward(bool fail)
    {
        var tenant = new TenantContext();
        var identity = new Mock<ICurrentUserService>();
        var handler = new Mock<IOutboxTemporalHandler>();
        handler.Setup(h => h.CanHandle("EmployeeChangedV2")).Returns(true);
        handler.Setup(h => h.SendAsync(It.IsAny<EventEnvelope>(), It.IsAny<CancellationToken>()))
            .Returns<EventEnvelope, CancellationToken>((_, _) =>
            {
                tenant.TenantId.Should().Be("tenant-a");
                identity.Verify(i => i.SetContext("tenant-a", "outbox-relay@system", "Outbox Relay", "system"), Times.Once);
                return fail ? Task.FromException(new InvalidOperationException("Handler failed")) : Task.CompletedTask;
            });
        using var provider = new ServiceCollection()
            .AddSingleton<ITenantScopeSetter>(tenant)
            .AddSingleton(identity.Object)
            .AddScoped<IOutboxTemporalHandler>(_ =>
            {
                tenant.TenantId.Should().Be("tenant-a");
                return handler.Object;
            }).BuildServiceProvider();
        var transport = CreateTransport(provider);

        var dispatch = transport.SendAsync(Envelope("tenant-a", Guid.CreateVersion7(), Guid.CreateVersion7()));
        if (fail)
            await Assert.ThrowsAsync<InvalidOperationException>(() => dispatch);
        else
            await dispatch;

        identity.Verify(i => i.ClearContext(), Times.Once);
        tenant.TenantId.Should().BeNull();
        handler.Verify(h => h.SendAsync(It.IsAny<EventEnvelope>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Tenant_identity_is_distinct_and_redelivery_reuses_the_same_workflow()
    {
        var starts = new List<WorkflowOptions>();
        var client = new Mock<ITemporalClient>();
        client.Setup(c => c.StartWorkflowAsync(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<object>>(), It.IsAny<WorkflowOptions>()))
            .Callback<string, IReadOnlyCollection<object>, WorkflowOptions>((_, _, options) => starts.Add(options))
            .ReturnsAsync((WorkflowHandle)null!);
        using var provider = new ServiceCollection().AddSingleton(client.Object).BuildServiceProvider();
        var transport = CreateTransport(provider);
        var entityId = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();

        await transport.SendAsync(Envelope("tenant-a", entityId, eventId));
        await transport.SendAsync(Envelope("tenant-b", entityId, eventId));
        await transport.SendAsync(Envelope("tenant-a", entityId, eventId));
        await transport.SendAsync(Envelope("TENANT-A", entityId, eventId));
        await transport.SendAsync(Envelope("tenant-a", entityId, Guid.CreateVersion7()));
        await transport.SendAsync(new EventEnvelope
        {
            TenantId = "tenant-a", EventType = "EmployeeChangedV2", EntityId = entityId,
            EntityVersion = 0, OutboxId = eventId, Payload = "{}"
        });

        starts[0].Id.Should().NotBe(starts[1].Id);
        starts[0].Id.Should().Be(starts[2].Id);
        starts[0].Id.Should().NotBe(starts[3].Id);
        starts[0].Id.Should().NotBe(starts[4].Id);
        starts[0].Id.Should().Be(starts[5].Id);
        starts.Should().OnlyContain(o => o.IdReusePolicy == WorkflowIdReusePolicy.RejectDuplicate
            && o.IdConflictPolicy == WorkflowIdConflictPolicy.UseExisting);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Missing_tenant_fails_before_transport_dispatch(string tenantId)
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var transport = CreateTransport(provider);
        await Assert.ThrowsAsync<ArgumentException>(() => transport.SendAsync(Envelope(tenantId, Guid.CreateVersion7())));
    }

    private static TemporalEventTransport CreateTransport(ServiceProvider provider) => new(
        provider.GetRequiredService<IServiceScopeFactory>(),
        Microsoft.Extensions.Options.Options.Create(new OutboxOptions
        {
            Temporal = new() { ["EmployeeChangedV2"] = new() { WorkflowType = "EmployeeSync", TaskQueue = "people" } }
        }),
        NullLogger<TemporalEventTransport>.Instance);

    private static EventEnvelope Envelope(string tenantId, Guid entityId, Guid eventId = default) => new()
    {
        TenantId = tenantId, EventType = "EmployeeChangedV2", EntityId = entityId, EntityVersion = 0, EventId = eventId, Payload = "{}"
    };
}
