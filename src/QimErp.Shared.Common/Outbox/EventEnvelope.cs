using System.Text.Json;

namespace QimErp.Shared.Common.Outbox;

/// <summary>Transport-facing view of a claimed outbox row.</summary>
public sealed class EventEnvelope
{
    public Guid OutboxId { get; init; }
    public Guid EventId { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string Transport { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public long EntityVersion { get; init; }
    public string Payload { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public string CorrelationId { get; init; } = string.Empty;

    public static EventEnvelope From(OutboxMessage message) => new()
    {
        OutboxId = message.Id,
        // Outbox row Id is UUIDv7 and doubles as EventId until payloads carry their own.
        EventId = message.Id,
        TenantId = message.TenantId,
        EventType = message.EventType,
        Transport = message.Transport,
        EntityId = message.EntityId,
        EntityVersion = message.EntityVersion,
        Payload = message.Payload,
        OccurredAt = message.OccurredAt,
        CorrelationId = message.Id.ToString("N"),
    };

    public T Deserialize<T>(JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<T>(Payload, options ?? JsonSerializerOptions.Web)
        ?? throw new InvalidOperationException($"Failed to deserialize outbox payload as {typeof(T).Name}.");
}
