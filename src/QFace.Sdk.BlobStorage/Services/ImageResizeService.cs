using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace QFace.Sdk.BlobStorage.Services;

public sealed class ImageResizeService : IImageResizeService
{
    private readonly ILogger<ImageResizeService> _logger;

    public ImageResizeService(ILogger<ImageResizeService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<byte[]> ResizeToWebPAsync(
        Stream imageStream,
        int width,
        int height,
        int quality = 85,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var sourceBitmap = SKBitmap.Decode(imageStream);
                if (sourceBitmap is null)
                {
                    _logger.LogError("SKBitmap.Decode returned null — the stream may not be a supported image format");
                    throw new InvalidOperationException("Failed to decode image: unsupported format or corrupt data.");
                }

                // Compute scale while maintaining aspect ratio (fit-within box)
                var scaleX = (float)width / sourceBitmap.Width;
                var scaleY = (float)height / sourceBitmap.Height;
                var scale = Math.Min(scaleX, scaleY);

                var targetWidth = (int)Math.Round(sourceBitmap.Width * scale);
                var targetHeight = (int)Math.Round(sourceBitmap.Height * scale);

                using var resizedBitmap = sourceBitmap.Resize(
                    new SKImageInfo(targetWidth, targetHeight),
                    SKFilterQuality.High);

                if (resizedBitmap is null)
                {
                    _logger.LogError("SKBitmap.Resize returned null for target size {W}x{H}", targetWidth, targetHeight);
                    throw new InvalidOperationException("Failed to resize image.");
                }

                using var image = SKImage.FromBitmap(resizedBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Webp, quality);

                return data.ToArray();
            }
            catch (Exception ex) when (ex is not InvalidOperationException && ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unexpected error resizing image to {W}x{H}", width, height);
                throw;
            }
        }, cancellationToken);
    }
}
