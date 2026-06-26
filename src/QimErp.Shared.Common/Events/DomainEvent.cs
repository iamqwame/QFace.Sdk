namespace QimErp.Shared.Common.Events;

/// <summary>
/// Base implementation for domain events with common properties
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? UserName { get; set; }

    /// <summary>Optional application correlation id (e.g. from <c>X-Correlation-Id</c>) for consumers and tracing.</summary>
    public string? CorrelationId { get; set; }

    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }

    protected DomainEvent(string tenantId, string? userEmail, string? triggeredBy = null, string? userName = null)
    {
        OccurredOn = DateTime.UtcNow;
        TenantId = tenantId;
        TriggeredBy = triggeredBy;
        UserEmail = userEmail ?? string.Empty;
        UserName = userName;
    }
    protected DomainEvent(Guid tenantId, string? userEmail, string? triggeredBy = null, string? userName = null)
    {
        OccurredOn = DateTime.UtcNow;
        TenantId = tenantId.ToString();
        TriggeredBy = triggeredBy;
        UserEmail = userEmail ?? string.Empty;
        UserName = userName;
    }
}
