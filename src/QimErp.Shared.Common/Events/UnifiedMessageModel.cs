using System.Text.Json.Serialization;

namespace QimErp.Shared.Common.Events;

public class UnifiedMessageModel
{
    // Required for all message types
    [JsonRequired]
    public string MessageType { get; set; } = string.Empty;

    // Email properties
    public string ToEmail { get; set; } = string.Empty;
    public List<string> ToEmails { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    // Template properties
    public string Template { get; set; } = string.Empty;
    /// <summary>Template name (no extension). Notifications module resolves + renders it.
    /// Ignored when <see cref="Template"/> already contains pre-rendered HTML.</summary>
    public string? TemplateCode { get; set; }
    public Dictionary<string, string> Replacements { get; set; } = new();

    // SMS properties
    public string PhoneNumber { get; set; } = string.Empty;
    public List<string> PhoneNumbers { get; set; } = [];
    public string Message { get; set; } = string.Empty;

    // Combined properties
    public string Email { get; set; } = string.Empty;

    // Metadata properties that might be useful
    public string MessageId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Owning tenant. Read by <c>TenantContextActivityInterceptor</c> (it reflects a
    /// <c>TenantId</c> property off the activity input) to seed the ambient tenant context
    /// before the notification <c>Send</c> activity runs — without it the tenant-scoped
    /// <c>NotificationHistory</c> read/write executes with an unseeded EF filter and the
    /// activity fails ("No TenantId on input for activity Send"). Stamped by
    /// <c>NotificationWorkflowStarter</c> from the ambient tenant when not already set.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Optional file attachments (e.g. payslip PDF).  Serialised through Temporal
    /// history — keep each attachment &lt;5 MB to stay within Graph API and history limits.
    /// </summary>
    public List<QimErp.Shared.Common.Services.Notifications.EmailAttachment>? Attachments { get; set; }
    
    // Timestamp for tracking
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}