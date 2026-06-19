using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QFace.Sdk.Temporal.Abstractions;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.Workflow.Temporal;

namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Default implementation of <see cref="INotificationWorkflowStarter"/>.
/// Starts a <see cref="INotificationWorkflow"/> on the <c>qimerp-notifications</c>
/// Temporal task queue using StartOrIgnore semantics (safe to call repeatedly).
/// </summary>
public class NotificationWorkflowStarter(
    IWorkflowStarter starter,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory)
    : INotificationWorkflowStarter
{
    // The notification worker listens on "qimerp-notifications" + Temporal:TaskQueueSuffix
    // (e.g. "-local" in Development). Honour the same suffix here so started workflows land
    // on the queue a worker is actually polling — otherwise notifications never deliver.
    private readonly string _taskQueue =
        "qimerp-notifications" + (configuration["Temporal:TaskQueueSuffix"] ?? string.Empty);

    public Task SendAsync(UnifiedMessageModel model)
    {
        model.MessageId ??= Guid.NewGuid().ToString();
        StampTenant(model);
        return starter.StartOrIgnoreAsync<INotificationWorkflow>(
            $"notification-{model.MessageId}",
            _taskQueue,
            wf => wf.RunAsync(model));
    }

    /// <summary>
    /// Stamp the ambient tenant onto the message so the notification worker's
    /// <c>TenantContextActivityInterceptor</c> can seed tenant context for the
    /// tenant-scoped <c>NotificationHistory</c> read/write. This is a singleton, so we
    /// resolve <see cref="ITenantContext"/> through a fresh scope — its value is
    /// AsyncLocal-backed and flows from the HTTP request (or the Temporal activity that
    /// triggered the send) into the scope. Best-effort: never fail a send on this.
    /// </summary>
    private void StampTenant(UnifiedMessageModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.TenantId)) return;
        try
        {
            using var scope = scopeFactory.CreateScope();
            var tenantId = scope.ServiceProvider.GetService<ITenantContext>()?.TenantId;
            if (!string.IsNullOrWhiteSpace(tenantId))
                model.TenantId = tenantId;
        }
        catch
        {
            // ignore — tenant stamping is best-effort and must not block notifications
        }
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
            Metadata     = metadata ?? request.Metadata ?? [],
            Attachments  = request.Attachments,
        });
}
