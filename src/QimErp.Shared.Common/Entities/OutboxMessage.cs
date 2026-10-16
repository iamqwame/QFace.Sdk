namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Durable outbox row (DATA-09 / Appendix 1). Written in the same EF transaction as the
/// business change; the Akka relay claims, delivers via the configured transport, then marks
/// processed only after the transport confirms.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string TenantId { get; set; } = string.Empty;

    /// <summary>Event type name used for transport routing (e.g. <c>EmployeeChangedEvent</c>).</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Transport key: <c>temporal</c> | <c>inprocess</c> | <c>rabbitmq</c> | <c>kafka</c>.</summary>
    public string Transport { get; set; } = OutboxTransports.InProcess;

    public Guid EntityId { get; set; }

    public long EntityVersion { get; set; }

    /// <summary>JSON payload of the integration / domain event.</summary>
    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset NextAttemptAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LockedUntil { get; set; }

    public string? LockedBy { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public DateTimeOffset? DeadAt { get; set; }

    /// <summary>Compatibility shim for the pre-DATA-09 shape. Prefer <see cref="ProcessedAt"/>.</summary>
    [Obsolete("Use ProcessedAt. Kept for source compatibility with early OutboxMessage drafts.")]
    public bool IsProcessed
    {
        get => ProcessedAt.HasValue;
        set
        {
            if (value && ProcessedAt is null)
                ProcessedAt = DateTimeOffset.UtcNow;
            else if (!value)
                ProcessedAt = null;
        }
    }

    /// <summary>
    /// Legacy factory — creates an in-process row with no entity binding.
    /// Prefer <see cref="Outbox.IOutbox"/> so transport routing and entity keys are set correctly.
    /// </summary>
    [Obsolete("Use IOutbox.Add so rows get TenantId, Transport, EntityId, and EntityVersion.")]
    public static OutboxMessage Create(string fullName, string payload)
    {
        var now = DateTimeOffset.UtcNow;
        return new OutboxMessage
        {
            EventType = fullName,
            Payload = payload,
            Transport = OutboxTransports.InProcess,
            OccurredAt = now,
            NextAttemptAt = now,
        };
    }
}

/// <summary>Well-known outbox transport names (DATA-09).</summary>
public static class OutboxTransports
{
    public const string InProcess = "inprocess";
    public const string Temporal = "temporal";
    public const string RabbitMq = "rabbitmq";
    public const string Kafka = "kafka";
}
