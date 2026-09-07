namespace QFace.Sdk.BlobStorage.Models;

public class PresignedPutUrlResult
{
    public string PutUrl { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
