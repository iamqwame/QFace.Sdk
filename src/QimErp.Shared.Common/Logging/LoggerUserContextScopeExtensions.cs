namespace QimErp.Shared.Common.Logging;

/// <summary>
/// Pushes TenantId, UserEmail, UserName (and UserId when meaningful) into the logging scope
/// so Serilog / structured sinks receive them on all child log entries.
/// </summary>
public static class LoggerUserContextScopeExtensions
{
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }

    /// <summary>
    /// Begins a log scope from HTTP/current user context. No-op when there is nothing to add.
    /// </summary>
    public static IDisposable BeginUserContextScope(this ILogger logger, ICurrentUserService? user)
    {
        var items = UserContextScopeProperties.From(user);
        return items.Count == 0 ? NullScope.Instance : logger.BeginScope(items)!;
    }

    /// <summary>
    /// Begins a log scope for background workers, Temporal activities, etc.
    /// Omits empty values; skips UserId when it is the literal "system".
    /// </summary>
    public static IDisposable BeginUserContextScope(
        this ILogger logger,
        string? tenantId,
        string? userEmail,
        string? userName,
        string? userId = null)
    {
        var items = UserContextScopeProperties.From(tenantId, userEmail, userName, userId);
        return items.Count == 0 ? NullScope.Instance : logger.BeginScope(items)!;
    }

    /// <summary>
    /// MediatR pipeline scope: <c>CorrelationId</c>, <c>TenantId</c> (domain name when present), <c>UserId</c> (acting user email per observability standard), <c>RequestType</c>.
    /// </summary>
    public static IDisposable BeginMediatrObservabilityScope(
        this ILogger logger,
        ICurrentUserService currentUser,
        string requestTypeName)
    {
        var items = new List<KeyValuePair<string, object>>(5);
        var correlationId = currentUser.GetCorrelationId();
        if (!string.IsNullOrWhiteSpace(correlationId))
            items.Add(new KeyValuePair<string, object>("CorrelationId", correlationId));

        var tenantKey = currentUser.GetDomainName() ?? currentUser.GetTenantId();
        if (!string.IsNullOrWhiteSpace(tenantKey))
            items.Add(new KeyValuePair<string, object>("TenantId", tenantKey));

        var actorEmail = currentUser.GetUserEmail();
        items.Add(new KeyValuePair<string, object>("UserId", string.IsNullOrWhiteSpace(actorEmail) ? "unknown" : actorEmail));

        items.Add(new KeyValuePair<string, object>("RequestType", requestTypeName));

        return items.Count == 0 ? NullScope.Instance : logger.BeginScope(items)!;
    }
}
