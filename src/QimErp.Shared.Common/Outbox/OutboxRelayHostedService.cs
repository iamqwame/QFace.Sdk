using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Outbox.Actors;

namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// Boots a dedicated Akka system: poller → consistent-hash pool of dispatch workers (Appendix 1).
/// </summary>
internal sealed class OutboxRelayHostedService(
    IServiceProvider rootServices,
    IOptions<OutboxOptions> options,
    ILogger<OutboxRelayHostedService> logger) : IHostedService
{
    private ActorSystem? _system;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.RelayEnabled)
        {
            logger.LogInformation("Outbox relay disabled (Outbox:RelayEnabled=false).");
            return Task.CompletedTask;
        }

        var setup = BootstrapSetup.Create()
            .And(DependencyResolverSetup.Create(rootServices));

        _system = ActorSystem.Create("QFaceOutboxRelay", setup);

        var scopes = rootServices.GetRequiredService<IServiceScopeFactory>();
        var transports = rootServices.GetRequiredService<IEnumerable<IEventTransport>>()
            .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);

        var workerCount = Math.Max(1, options.Value.DispatchWorkerCount);
        var workerProps = Props.Create(() =>
                new OutboxDispatchWorkerActor(scopes, transports, options))
            .WithRouter(new ConsistentHashingPool(workerCount));

        var router = _system.ActorOf(workerProps, "outbox-dispatch");
        _system.ActorOf(
            Props.Create(() => new OutboxPollerActor(
                scopes,
                router,
                options,
                rootServices.GetRequiredService<ILoggerFactory>())),
            "outbox-poller");

        logger.LogInformation(
            "Outbox relay started. Workers={Workers}, PollInterval={PollInterval}",
            workerCount, options.Value.PollInterval);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_system is null) return;

        logger.LogInformation("Stopping outbox relay actor system.");
        await _system.Terminate();
        await _system.WhenTerminated;
        _system = null;
    }
}
