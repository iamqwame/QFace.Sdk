using System.Reflection;
using Akka.Actor;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QimErp.Shared.Common.Outbox;
using Xunit;

namespace QimErp.Shared.Common.Tests.Outbox;

public sealed class OutboxRelayHostedServiceTests
{
    [Fact]
    public async Task StopAsync_When_System_Is_Null_Does_Not_Throw()
    {
        var sut = CreateSut();

        var act = async () => await sut.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_When_System_Already_Terminated_Does_Not_Throw()
    {
        var sut = CreateSut();
        var system = ActorSystem.Create("OutboxRelayStopTest");
        await system.Terminate();
        await system.WhenTerminated;

        SetSystem(sut, system);

        var act = async () => await sut.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_Terminates_Live_System_Without_Throwing()
    {
        var sut = CreateSut();
        var system = ActorSystem.Create("OutboxRelayLiveStopTest");
        SetSystem(sut, system);

        var act = async () => await sut.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
        system.WhenTerminated.IsCompleted.Should().BeTrue();
    }

    private static OutboxRelayHostedService CreateSut() =>
        new(
            Mock.Of<IServiceProvider>(),
            Microsoft.Extensions.Options.Options.Create(new OutboxOptions { RelayEnabled = false }),
            NullLogger<OutboxRelayHostedService>.Instance);

    private static void SetSystem(OutboxRelayHostedService sut, ActorSystem? system)
    {
        var field = typeof(OutboxRelayHostedService).GetField(
            "_system", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        field!.SetValue(sut, system);
    }
}
