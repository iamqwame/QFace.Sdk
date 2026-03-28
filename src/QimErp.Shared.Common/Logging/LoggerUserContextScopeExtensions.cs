using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Services.Auth;

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
}
