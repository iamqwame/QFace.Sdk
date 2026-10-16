using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace QimErp.Shared.Common.Outbox.Actors;

/// <summary>
/// Delivers one claimed outbox row through its transport, then marks processed or schedules retry.
/// </summary>
internal sealed class OutboxDispatchWorkerActor : ReceiveActor
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IReadOnlyDictionary<string, IEventTransport> _transports;
    private readonly OutboxOptions _options;

    public OutboxDispatchWorkerActor(
        IServiceScopeFactory scopes,
        IReadOnlyDictionary<string, IEventTransport> transports,
        IOptions<OutboxOptions> options)
    {
        _scopes = scopes;
        _transports = transports;
        _options = options.Value;

        ReceiveAsync<OutboxDispatch>(DispatchAsync);
    }

    private async Task DispatchAsync(OutboxDispatch dispatch)
    {
        var msg = dispatch.Message;
        using var scope = _scopes.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<OutboxStore>();
        var log = Context.GetLogger();

        try
        {
            if (!_transports.TryGetValue(msg.Transport, out var transport))
            {
                throw new InvalidOperationException(
                    $"No IEventTransport registered for '{msg.Transport}'.");
            }

            await transport.SendAsync(EventEnvelope.From(msg), CancellationToken.None);
            await store.MarkProcessedAsync(msg.Id, CancellationToken.None);
        }
        catch (Exception ex)
        {
            var attempts = msg.Attempts + 1;
            var delaySeconds = Math.Min(300, Math.Pow(2, attempts));
            var dead = attempts >= Math.Max(1, _options.MaxAttempts);
            await store.ScheduleRetryAsync(
                msg.Id,
                attempts,
                ex.Message,
                DateTimeOffset.UtcNow.AddSeconds(delaySeconds),
                dead,
                CancellationToken.None);

            if (dead)
            {
                log.Error(
                    "outbox.dead Id={0} Type={1} Transport={2} Error={3}",
                    msg.Id, msg.EventType, msg.Transport, ex.Message);
            }
            else
            {
                log.Warning(
                    "outbox.retry Id={0} Type={1} Attempts={2} Error={3}",
                    msg.Id, msg.EventType, attempts, ex.Message);
            }
        }
    }
}
