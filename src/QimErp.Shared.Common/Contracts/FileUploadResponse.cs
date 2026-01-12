namespace QimErp.Shared.Common.Contracts;

/// <summary>
/// Response model for file upload operations
/// </summary>
public record FileUploadResponse
{
    public string Url { get; init; } = string.Empty;
    public string S3Key { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}

