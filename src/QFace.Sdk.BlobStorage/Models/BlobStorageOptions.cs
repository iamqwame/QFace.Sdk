namespace QFace.Sdk.BlobStorage.Models;

public class BlobStorageOptions
{
    /// <summary>
    /// The blob storage provider (e.g., AWS, DigitalOcean, Backblaze)
    /// </summary>
    public string Provider { get; set; } = "DigitalOcean";

    /// <summary>
    /// The service URL endpoint
    /// </summary>
    public string ServiceURL { get; set; }

    /// <summary>
    /// The region where the bucket is located
    /// </summary>
    public string Region { get; set; } = "nyc3";

    /// <summary>
    /// Whether to use path-style addressing (false for DigitalOcean)
    /// </summary>
    public bool ForcePathStyle { get; set; } = false;

    /// <summary>
    /// Credentials for accessing the blob storage
    /// </summary>
    public BlobStorageCredentials Credentials { get; set; } = new BlobStorageCredentials();

    /// <summary>
    /// Bucket configuration
    /// </summary>
    public BlobStorageBucketOptions Bucket { get; set; } = new BlobStorageBucketOptions();
}

public class BlobStorageCredentials
{
    /// <summary>
    /// Access key for the blob storage service
    /// </summary>
    public string AccessKey { get; set; }

    /// <summary>
    /// Secret key for the blob storage service
    /// </summary>
    public string SecretKey { get; set; }
}

public class BlobStorageBucketOptions
{
    /// <summary>
    /// Name of the bucket/container
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Base domain for CDN without region prefix (e.g., cdn.digitaloceanspaces.com)
    /// </summary>
    public string CdnBaseDomain { get; set; } = "cdn.digitaloceanspaces.com";
}

public enum S3Provider
{
    /// <summary>
    /// Amazon Web Services S3
    /// </summary>
    AWS,
    
    /// <summary>
    /// Digital Ocean Spaces
    /// </summary>
    DigitalOcean,
    
    /// <summary>
    /// Backblaze B2 Cloud Storage
    /// </summary>
    Backblaze,
    
    /// <summary>
    /// Wasabi Cloud Storage
    /// </summary>
    Wasabi,
    
    /// <summary>
    /// MinIO Object Storage
    /// </summary>
    MinIO,
    
    /// <summary>
    /// Generic S3-compatible storage
    /// </summary>
    Generic
}