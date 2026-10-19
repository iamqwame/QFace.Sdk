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
        if (string.IsNullOrWhiteSpace(model.MessageId))
            model.MessageId = Guid.CreateVersion7().ToString();
        StampTenant(model);
        return starter.StartOrIgnoreAsync<INotificationWorkflow>(
            $"notification-{model.MessageId}",
            _taskQueue,
            wf => wf.RunAsync(model));
    }

    /// <summary>
    /// Stamp the ambient tenant onto the message so the notification worker's
    /// <c>TenantContextActivityInterceptor</c> can seed tenant context. Resolves
    /// <see cref="ITenantContext"/> through a fresh scope (this type is a singleton;
    /// tenant is AsyncLocal-backed). Fails closed if TenantId is still empty after stamp.
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
            // Resolution failure still fails closed below when TenantId remains empty.
        }

        if (string.IsNullOrWhiteSpace(model.TenantId))
            throw new InvalidOperationException(
                "Cannot start notification workflow without a TenantId. Set UnifiedMessageModel.TenantId or ensure ITenantContext is seeded.");
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
