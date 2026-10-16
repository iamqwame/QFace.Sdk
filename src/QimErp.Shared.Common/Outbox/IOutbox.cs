namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// Writes durable outbox rows into the current <see cref="DbContext"/> so they commit
/// in the same transaction as the business change (DATA-09).
/// </summary>
public interface IOutbox
{
    /// <summary>
    /// Enqueues <paramref name="domainEvent"/> for every transport configured for its type.
    /// Call before <c>SaveChanges</c>; do not Tell / start Temporal after save.
    /// </summary>
    void Add(IDomainEvent domainEvent, Guid entityId, long entityVersion = 0);

    /// <summary>
    /// Enqueues an arbitrary serializable payload under <paramref name="eventType"/>.
    /// </summary>
    void Add(string eventType, string tenantId, Guid entityId, long entityVersion, object payload);
}

/// <summary>Resolves which transports receive a given event type.</summary>
public interface ITransportRouting
{
    IReadOnlyList<string> TransportsFor(string eventType);
}
