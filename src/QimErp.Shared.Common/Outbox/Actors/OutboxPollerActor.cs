using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QimErp.Shared.Common.Outbox.Actors;

/// <summary>
/// Periodically claims a batch of outbox rows and fans them to the consistent-hash router.
/// </summary>
internal sealed class OutboxPollerActor : ReceiveActor, IWithTimers
{
    public ITimerScheduler Timers { get; set; } = null!;

    private readonly IServiceScopeFactory _scopes;
    private readonly IActorRef _router;
    private readonly OutboxOptions _options;
    private readonly ILogger _logger;
    private readonly string _node = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    public OutboxPollerActor(
        IServiceScopeFactory scopes,
        IActorRef router,
        IOptions<OutboxOptions> options,
        ILoggerFactory loggerFactory)
    {
        _scopes = scopes;
        _router = router;
        _options = options.Value;
        _logger = loggerFactory.CreateLogger<OutboxPollerActor>();

        ReceiveAsync<OutboxPollTick>(OnTickAsync);
    }

    protected override void PreStart()
    {
        var interval = _options.PollInterval <= TimeSpan.Zero
            ? TimeSpan.FromSeconds(1)
            : _options.PollInterval;
        Timers.StartPeriodicTimer("outbox-poll", OutboxPollTick.Instance, interval, interval);
    }

    private async Task OnTickAsync(OutboxPollTick _)
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<OutboxStore>();
            var batch = await store.ClaimAsync(
                _node,
                _options.BatchSize,
                _options.ClaimLease,
                CancellationToken.None);

            OutboxMetrics.RecordSweepBatch(batch.Count);

            foreach (var msg in batch)
                _router.Tell(new OutboxDispatch(msg));

            if (batch.Count > 0)
            {
                _logger.LogDebug("Outbox claimed {Count} row(s) on node {Node}", batch.Count, _node);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Outbox poll tick failed on node {Node}", _node);
        }
    }
}
