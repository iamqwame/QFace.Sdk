namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for setting audit context in consumers
/// Use with ConsumerUserContextService to enable AuditEntitySaveChangesInterceptor
/// </summary>
public static class ConsumerAuditExtensions
{
    /// <summary>
    /// Executes an action with audit context automatically set and cleared
    /// </summary>
    public static async Task WithAuditContextAsync(
        this ConsumerUserContextService contextService,
        string tenantId,
        string userEmail,
        Func<Task> action,
        string? userName = null,
        string? triggeredBy = null)
    {
        try
        {
            contextService.SetContext(tenantId, userEmail, userName, triggeredBy);
            await action();
        }
        finally
        {
            contextService.ClearContext();
        }
    }

    /// <summary>
    /// Executes an action with audit context from a domain event
    /// </summary>
    public static Task WithAuditContextAsync(
        this ConsumerUserContextService contextService,
        DomainEvent @event,
        Func<Task> action)
    {
        return contextService.WithAuditContextAsync(@event.TenantId, @event.UserEmail, action, @event.UserName, @event.TriggeredBy);
    }

    /// <summary>
    /// Executes a function with audit context and returns a result
    /// </summary>
    public static async Task<T> WithAuditContextAsync<T>(
        this ConsumerUserContextService contextService,
        string tenantId,
        string userEmail,
        Func<Task<T>> func,
        string? userName = null,
        string? triggeredBy = null)
    {
        try
        {
            contextService.SetContext(tenantId, userEmail, userName, triggeredBy);
            return await func();
        }
        finally
        {
            contextService.ClearContext();
        }
    }

    /// <summary>
    /// Executes a function with audit context from a domain event and returns a result
    /// </summary>
    // public static Task<T> WithAuditContextAsync<T>(
    //     this ConsumerUserContextService contextService,
    //     DomainEvent @event,
    //     Func<Task<T>> func)
    // {
    //     return contextService.WithAuditContextAsync(@event.TenantId, @event.UserEmail, func, @event.UserName, @event.TriggeredBy);
    // }
}
