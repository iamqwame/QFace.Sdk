namespace QFace.Sdk.BlobStorage.Services;

public interface IImageResizeService
{
    /// <summary>
    /// Decodes <paramref name="imageStream"/>, scales to fit within
    /// <paramref name="width"/>×<paramref name="height"/> (maintains aspect ratio),
    /// and returns the result encoded as WebP at the given quality.
    /// </summary>
    Task<byte[]> ResizeToWebPAsync(
        Stream imageStream,
        int width,
        int height,
        int quality = 85,
        CancellationToken cancellationToken = default);
}
