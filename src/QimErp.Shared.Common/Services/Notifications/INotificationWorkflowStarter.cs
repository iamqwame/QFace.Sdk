using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Fires a <c>NotificationWorkflow</c> via Temporal from any module — no HTTP API call
/// or exchange name required.
///
/// Registered automatically by <see cref="QimErp.Shared.Common.Extensions.TemporalServiceCollectionExtensions.AddTemporalWorkflow"/>.
/// </summary>
public interface INotificationWorkflowStarter
{
    /// <summary>Full control — caller populates the <see cref="UnifiedMessageModel"/> directly.</summary>
    Task SendAsync(UnifiedMessageModel model);

    /// <summary>Sends a plain-text email notification.</summary>
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Sends a template-based email. The Notifications module resolves
    /// <paramref name="templateCode"/> and merges <paramref name="replacements"/>.
    /// </summary>
    Task SendTemplatedEmailAsync(
        string toEmail,
        string subject,
        string templateCode,
        Dictionary<string, string> replacements,
        Dictionary<string, string>? metadata = null);
}
