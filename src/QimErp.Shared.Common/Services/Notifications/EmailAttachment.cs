namespace QimErp.Shared.Common.Services.Notifications;

/// <summary>
/// A binary file attachment to include on an outgoing email.
/// Serialisable so it can be carried in the <see cref="UnifiedMessageModel"/>
/// through Temporal workflow history and deserialized by the activity.
/// Keep attachments small (&lt;5 MB recommended) to stay within Graph API limits.
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>Filename shown to the recipient, including extension (e.g. "PS-2026-00525.pdf").</summary>
    public required string FileName { get; init; }

    /// <summary>MIME content-type (e.g. "application/pdf", "image/png").</summary>
    public required string ContentType { get; init; }

    /// <summary>Raw file bytes. Serialised as a Base64 string by System.Text.Json.</summary>
    public required byte[] Content { get; init; }
}
