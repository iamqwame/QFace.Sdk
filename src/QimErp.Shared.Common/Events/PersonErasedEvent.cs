namespace QimErp.Shared.Common.Events;

/// <summary>
/// DATA-23 compliant person erasure contract: envelope + Ids only.
/// No names, emails, phones, national Ids, staff codes, or other personal/display data.
/// Consumers react via the erasure workflow; the event itself carries no fetchable PII.
/// </summary>
public sealed class PersonErasedEvent
{
    public const string EventTypeName = "PersonErased";

    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public string EventType => EventTypeName;
    public string TenantId { get; init; } = string.Empty;
    /// <summary>Canonical subject id — in People equals <c>Employee.Id</c>.</summary>
    public Guid PersonId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    public static PersonErasedEvent Create(
        Guid personId,
        string tenantId,
        string? correlationId = null)
    {
        return new PersonErasedEvent
        {
            EventId = Guid.CreateVersion7(),
            PersonId = personId,
            TenantId = tenantId ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }
}
