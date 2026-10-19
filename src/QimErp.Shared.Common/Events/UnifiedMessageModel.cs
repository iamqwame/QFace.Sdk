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
    // Persisted in plaintext in Temporal history; never put tokens, passwords or reset links here.
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
    /// Owning tenant. Required for the notification worker's
    /// <c>TenantContextActivityInterceptor</c>. Stamped by
    /// <c>NotificationWorkflowStarter</c> from ambient context when unset; send fails closed
    /// if still empty after stamp.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Optional file attachments (e.g. payslip PDF).  Serialised through Temporal
    /// history — keep each attachment &lt;5 MB to stay within Graph API and history limits.
    /// </summary>
    public List<QimErp.Shared.Common.Services.Notifications.EmailAttachment>? Attachments { get; set; }
    
    // Timestamp for tracking
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}