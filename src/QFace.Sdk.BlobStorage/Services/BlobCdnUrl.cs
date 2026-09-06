using QFace.Sdk.BlobStorage.Models;

namespace QFace.Sdk.BlobStorage.Services;

public static class BlobCdnUrl
{
    public static string FromKey(BlobStorageOptions options, string s3Key)
    {
        if (string.IsNullOrEmpty(s3Key))
            throw new ArgumentException("S3 key cannot be null or empty", nameof(s3Key));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        var bucket = options.Bucket?.Name
                     ?? throw new ArgumentException("Bucket Name configuration is missing");
        var region = string.IsNullOrWhiteSpace(options.Region) ? "nyc3" : options.Region;
        var configured = options.Bucket?.CdnBaseDomain;

        if (configured is null)
            configured = "cdn.digitaloceanspaces.com";

        if (!string.IsNullOrWhiteSpace(configured))
        {
            var host = NormalizePublicHost(configured);
            if (UsesPathStylePublicCdn(options.Provider, host))
                return $"https://{host}/{s3Key}";

            return $"https://{bucket}.{region}.{host}/{s3Key}";
        }

        if (!string.IsNullOrWhiteSpace(options.ServiceURL))
            return $"{options.ServiceURL.TrimEnd('/')}/{bucket}/{s3Key}";

        throw new InvalidOperationException(
            "Blob storage ServiceURL or Bucket:CdnBaseDomain must be configured to build a public URL.");
    }

    private static bool UsesPathStylePublicCdn(string provider, string host)
    {
        if ((provider ?? "").Equals("CloudflareR2", StringComparison.OrdinalIgnoreCase))
            return true;

        return host.Contains("r2.dev", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePublicHost(string configured)
    {
        var host = configured.Trim();
        if (host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            host = host["https://".Length..];
        else if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            host = host["http://".Length..];

        return host.TrimEnd('/');
    }
}
