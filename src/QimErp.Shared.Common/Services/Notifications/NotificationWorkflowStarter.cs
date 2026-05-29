using QFace.Sdk.Temporal.Abstractions;
using QimErp.Shared.Common.Services.Workflow.Temporal;

namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Default implementation of <see cref="INotificationWorkflowStarter"/>.
/// Starts a <see cref="INotificationWorkflow"/> on the <c>qimerp-notifications</c>
/// Temporal task queue using StartOrIgnore semantics (safe to call repeatedly).
/// </summary>
public class NotificationWorkflowStarter(IWorkflowStarter starter)
    : INotificationWorkflowStarter
{
    private const string TaskQueue = "qimerp-notifications";

    public Task SendAsync(UnifiedMessageModel model)
    {
        model.MessageId ??= Guid.NewGuid().ToString();
        return starter.StartOrIgnoreAsync<INotificationWorkflow>(
            $"notification-{model.MessageId}",
            TaskQueue,
            wf => wf.RunAsync(model));
    }

    public Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        Dictionary<string, string>? metadata = null) =>
        SendAsync(new UnifiedMessageModel
        {
            MessageType = "simple_email",
            ToEmail     = toEmail,
            Subject     = subject,
            Body        = body,
            Metadata    = metadata ?? []
        });

    public Task SendTemplatedEmailAsync(
        string toEmail,
        string subject,
        string templateCode,
        Dictionary<string, string> replacements,
        Dictionary<string, string>? metadata = null) =>
        SendAsync(new UnifiedMessageModel
        {
            MessageType  = "templated_email",
            ToEmail      = toEmail,
            Subject      = subject,
            TemplateCode = templateCode,
            Replacements = replacements,
            Metadata     = metadata ?? []
        });

    public Task SendTemplatedEmailAsync(
        TemplatedEmailRequest request,
        Dictionary<string, string>? metadata = null) =>
        SendAsync(new UnifiedMessageModel
        {
            MessageType  = "templated_email",
            ToEmail      = request.ToEmail,
            Subject      = request.Subject,
            TemplateCode = request.TemplateCode,
            Replacements = new Dictionary<string, string>(request.Tokens),
            Metadata     = metadata ?? request.Metadata ?? []
        });
}
