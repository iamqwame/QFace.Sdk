namespace QimErp.Shared.Common.Extensions;

public static class UserContextAuditExtensions
{
    public static async Task WithAuditContextAsync(
        this UserContextService contextService,
        string tenantId,
        string userEmail,
        Func<Task> action,
        string? userName = null,
        string? userId = null)
    {
        try
        {
            contextService.SetContext(tenantId, userEmail, userName, userId);
            await action();
        }
        finally
        {
            contextService.ClearContext();
        }
    }

    public static async Task<T> WithAuditContextAsync<T>(
        this UserContextService contextService,
        string tenantId,
        string userEmail,
        Func<Task<T>> func,
        string? userName = null,
        string? userId = null)
    {
        try
        {
            contextService.SetContext(tenantId, userEmail, userName, userId);
            return await func();
        }
        finally
        {
            contextService.ClearContext();
        }
    }

    public static async Task WithAuditContextAsync(
        this ICurrentUserService contextService,
        string tenantId,
        string userEmail,
        Func<Task> action,
        string? userName = null,
        string? userId = null)
    {
        if (contextService is UserContextService userContextService)
        {
            await userContextService.WithAuditContextAsync(tenantId, userEmail, action, userName, userId);
        }
        else
        {
            await action();
        }
    }

    public static async Task<T> WithAuditContextAsync<T>(
        this ICurrentUserService contextService,
        string tenantId,
        string userEmail,
        Func<Task<T>> func,
        string? userName = null,
        string? userId = null)
    {
        if (contextService is UserContextService userContextService)
        {
            return await userContextService.WithAuditContextAsync(tenantId, userEmail, func, userName, userId);
        }
        else
        {
            return await func();
        }
    }
}

