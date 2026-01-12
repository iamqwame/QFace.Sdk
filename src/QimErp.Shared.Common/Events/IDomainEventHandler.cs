namespace QimErp.Shared.Common.Events;

/// <summary>
/// Marker interface for domain event handlers.
/// Provides strongly-typed base for domain event handling.
/// </summary>
/// <typeparam name="TDomainEvent">The domain event type to handle</typeparam>
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    // Inherits Handle method from INotificationHandler
    // Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for domain event handlers with common functionality
/// </summary>
/// <typeparam name="TDomainEvent">The domain event type to handle</typeparam>
public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    protected readonly ILogger Logger;

    protected DomainEventHandler(ILogger logger)
    {
        Logger = logger;
    }

    public async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogDebug("Handling domain event {EventType} at {OccurredOn}", 
                typeof(TDomainEvent).Name, domainEvent.OccurredOn);

            await HandleDomainEvent(domainEvent, cancellationToken);

            Logger.LogDebug("Successfully handled domain event {EventType} at {OccurredOn}", 
                typeof(TDomainEvent).Name, domainEvent.OccurredOn);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling domain event {EventType} at {OccurredOn}", 
                typeof(TDomainEvent).Name, domainEvent.OccurredOn);
            throw;
        }
    }

    /// <summary>
    /// Handle the specific domain event. Override this method in concrete handlers.
    /// </summary>
    protected abstract Task HandleDomainEvent(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
