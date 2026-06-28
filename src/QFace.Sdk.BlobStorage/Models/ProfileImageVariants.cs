namespace QFace.Sdk.BlobStorage.Models;

public sealed record ProfileImageVariants
{
    public string? Thumb { get; init; }    // 48×48 WebP — avatar chips, list rows
    public string? Sm { get; init; }       // 96×96 WebP — compact cards, supervisor cache
    public string? Md { get; init; }       // 256×256 WebP — profile page hero
    public string? Lg { get; init; }       // 512×512 WebP — full-size / print
    public string? Original { get; init; } // original file, native format — kept for re-processing
}
