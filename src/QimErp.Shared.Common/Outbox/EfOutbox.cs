using System.Text.Json;
using Microsoft.Extensions.Options;

namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// EF-backed <see cref="IOutbox"/>. Adds one <see cref="OutboxMessage"/> per configured
/// transport to the ambient <see cref="DbContext"/> (same unit of work / transaction).
/// </summary>
internal sealed class EfOutbox(
    DbContext db,
    ITransportRouting routing,
    TimeProvider clock,
    IOptions<JsonSerializerOptions>? jsonOptions = null) : IOutbox
{
    private static readonly JsonSerializerOptions DefaultJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly JsonSerializerOptions _json = jsonOptions?.Value ?? DefaultJson;

    public void Add(IDomainEvent domainEvent, Guid entityId, long entityVersion = 0)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var eventType = domainEvent.GetType().Name;
        Add(eventType, domainEvent.TenantId ?? string.Empty, entityId, entityVersion, domainEvent);
    }

    public void Add(string eventType, string tenantId, Guid entityId, long entityVersion, object payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentNullException.ThrowIfNull(payload);

        var now = clock.GetUtcNow();
        var payloadJson = JsonSerializer.Serialize(payload, payload.GetType(), _json);

        var transports = routing.TransportsFor(eventType).ToList();
        foreach (var transport in transports)
        {
            db.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantId ?? string.Empty,
                EventType = eventType,
                Transport = transport.ToLowerInvariant(),
                EntityId = entityId,
                EntityVersion = entityVersion,
                Payload = payloadJson,
                OccurredAt = now,
                NextAttemptAt = now,
            });
        }

        OutboxMetrics.RecordEnqueued(transports.Count);
    }
}
