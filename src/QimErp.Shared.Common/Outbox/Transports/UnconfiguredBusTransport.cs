using Microsoft.Extensions.Logging;

namespace QimErp.Shared.Common.Outbox.Transports;

/// <summary>
/// Placeholder for RabbitMQ / Kafka until a decision-log-approved bus is wired.
/// Rows routed here fail and retry so misconfiguration is visible rather than silent.
/// </summary>
internal abstract class UnconfiguredBusTransport(string name, ILogger logger) : IEventTransport
{
    public string Name { get; } = name;

    public Task SendAsync(EventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        logger.LogError(
            "Outbox transport '{Transport}' is not configured. EventType={EventType}, OutboxId={OutboxId}. " +
            "Record a decision log before enabling RabbitMQ/Kafka (DATA-09).",
            Name, envelope.EventType, envelope.OutboxId);

        return Task.FromException(new InvalidOperationException(
            $"Outbox transport '{Name}' is not configured in this SDK build."));
    }
}

internal sealed class UnconfiguredRabbitMqTransport(ILoggerFactory loggerFactory)
    : UnconfiguredBusTransport(
        OutboxTransports.RabbitMq,
        loggerFactory.CreateLogger("Outbox.RabbitMq"));

internal sealed class UnconfiguredKafkaTransport(ILoggerFactory loggerFactory)
    : UnconfiguredBusTransport(
        OutboxTransports.Kafka,
        loggerFactory.CreateLogger("Outbox.Kafka"));
