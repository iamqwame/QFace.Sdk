using QimErp.Shared.Common.Logging;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for setting audit context in consumers
/// Use with ConsumerUserContextService to enable AuditEntitySaveChangesInterceptor
/// </summary>
public static class ConsumerAuditExtensions
{
    /// <summary>
    /// Executes an action with audit context automatically set and cleared.
    /// Pass <paramref name="logger"/> to attach TenantId/UserEmail/UserName to log scope for the action.
    /// </summary>
    public static async Task WithAuditContextAsync(
        this ConsumerUserContextService contextService,
        string tenantId,
        string userEmail,
        Func<Task> action,
        string? userName = null,
        string? triggeredBy = null,
        ILogger? logger = null)
    {
        try
        {
            contextService.SetContext(tenantId, userEmail, userName, triggeredBy);
            if (logger != null)
            {
                var scopeItems = UserContextScopeProperties.From(tenantId, userEmail, userName, triggeredBy);
                if (scopeItems.Count > 0)
                {
                    using (logger.BeginScope(scopeItems))
                        await action();
                    return;
                }
            }

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
        Func<Task> action,
        ILogger? logger = null)
    {
        return contextService.WithAuditContextAsync(
            @event.TenantId,
            @event.UserEmail,
            action,
            @event.UserName,
            @event.TriggeredBy,
            logger);
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
        string? triggeredBy = null,
        ILogger? logger = null)
    {
        try
        {
            contextService.SetContext(tenantId, userEmail, userName, triggeredBy);
            if (logger != null)
            {
                var scopeItems = UserContextScopeProperties.From(tenantId, userEmail, userName, triggeredBy);
                if (scopeItems.Count > 0)
                {
                    using (logger.BeginScope(scopeItems))
                        return await func();
                }
            }

            return await func();
        }
        finally
        {
            contextService.ClearContext();
        }
    }

}
