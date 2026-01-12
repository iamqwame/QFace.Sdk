namespace QimErp.Shared.Common.Events;

/// <summary>
/// Base implementation for domain events with common properties
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; set; }
    public string TenantId { get; set; }
    public string? TriggeredBy { get; set; }
    public string UserEmail { get; set; }
    public string? UserName { get; set; }

    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }

    protected DomainEvent(string tenantId, string userEmail, string? triggeredBy = null, string? userName = null)
    {
        OccurredOn = DateTime.UtcNow;
        TenantId = tenantId;
        TriggeredBy = triggeredBy;
        UserEmail = userEmail;
        UserName = userName;
    }
}
