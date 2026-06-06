namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// Strict templated-email payload — domain callers supply template code and complete content tokens;
/// the notification service merges infrastructure defaults only.
/// </summary>
public sealed record TemplatedEmailRequest
{
    public required string ToEmail { get; init; }
    public required string Subject { get; init; }
    public required string TemplateCode { get; init; }
    public required IReadOnlyDictionary<string, string> Tokens { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
    /// <summary>Optional file attachments (e.g. payslip PDF). Forwarded to <see cref="UnifiedMessageModel.Attachments"/>.</summary>
    public List<EmailAttachment>? Attachments { get; init; }
}
