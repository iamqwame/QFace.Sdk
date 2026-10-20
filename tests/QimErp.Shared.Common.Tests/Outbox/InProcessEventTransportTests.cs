using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Outbox;
using QimErp.Shared.Common.Outbox.Transports;
using QimErp.Shared.Common.Services.Auth;
using QimErp.Shared.Common.Services.MultiTenancy;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

public class InProcessEventTransportTests
{
    public sealed record Notification : INotification;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dispatch_restores_tenant_and_clears_it_even_when_consumer_fails(bool fail)
    {
        var tenant = new TenantContext();
        var identity = new Mock<ICurrentUserService>();
        var publisher = new Mock<IPublisher>();
        publisher.Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns<INotification, CancellationToken>((_, _) =>
            {
                tenant.TenantId.Should().Be("tenant-a");
                identity.Verify(i => i.SetContext("tenant-a", "outbox-relay@system", "Outbox Relay", "system"), Times.Once);
                return fail ? Task.FromException(new InvalidOperationException("Consumer failed")) : Task.CompletedTask;
            });
        using var provider = new ServiceCollection()
            .AddSingleton<ITenantScopeSetter>(tenant)
            .AddSingleton(identity.Object)
            .AddSingleton(publisher.Object)
            .AddSingleton<IOutboxEventTypeResolver>(new OutboxEventTypeResolver([new("test", typeof(Notification))]))
            .BuildServiceProvider();
        var transport = new InProcessEventTransport(provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<InProcessEventTransport>.Instance);

        var dispatch = transport.SendAsync(new EventEnvelope { TenantId = "tenant-a", EventType = "test", Payload = "{}" });
        if (fail)
            await Assert.ThrowsAsync<InvalidOperationException>(() => dispatch);
        else
            await dispatch;

        identity.Verify(i => i.ClearContext(), Times.Once);
        tenant.TenantId.Should().BeNull();
        publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
