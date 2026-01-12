namespace QimErp.Shared.Common.Events;

/// <summary>
/// Marker interface for domain events.
/// Inherits from INotification to integrate with MediatR pipeline.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the domain event occurred
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// The tenant context where this event occurred
    /// </summary>
    string TenantId { get; }
    
    /// <summary>
    /// User who triggered the domain event
    /// </summary>
    string? TriggeredBy { get; }
}
public abstract record BaseDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public string? TriggeredBy { get; init; }

    protected BaseDomainEvent() { }
    protected BaseDomainEvent(string tenantId, string? triggeredBy)
        => (TenantId, TriggeredBy) = (tenantId ?? string.Empty, triggeredBy);
}