using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QFace.Sdk.BlobStorage.Services;

/// <summary>
/// Interface for file upload service operations
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Uploads a file to blob storage
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Optional folder path within the storage</param>
    /// <param name="fileName">Optional file name (if not provided, a unique name will be generated)</param>
    /// <returns>URL of the uploaded file</returns>
    Task<string> UploadFileAsync(IFormFile file, string folder, string fileName = null);
        
    /// <summary>
    /// Deletes a file from blob storage
    /// </summary>
    /// <param name="fileUrl">URL or path of the file to delete</param>
    Task DeleteFileAsync(string fileUrl);
        
    /// <summary>
    /// Validates if a file meets the specified criteria
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <param name="allowedExtensions">Array of allowed file extensions</param>
    /// <param name="maxSizeInBytes">Maximum allowed file size in bytes</param>
    /// <returns>True if the file is valid, otherwise false</returns>
    bool IsValidFile(IFormFile file, string[] allowedExtensions, long maxSizeInBytes);
        
    /// <summary>
    /// Gets a pre-signed URL for temporary access to a file
    /// </summary>
    /// <param name="fileKey">Key or path of the file</param>
    /// <param name="expirationMinutes">Duration in minutes for which the URL remains valid</param>
    /// <returns>Pre-signed URL for the file</returns>
    Task<string> GetPreSignedUrlAsync(string fileKey, int expirationMinutes = 15);
        
    /// <summary>
    /// Gets the CDN URL for a file
    /// </summary>
    /// <param name="s3Key">The storage key of the file</param>
    /// <returns>Full CDN URL for the file</returns>
    string GetCdnUrl(string s3Key);
}


public class S3FileUploadService : IFileUploadService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _cdnBaseDomain;
        private readonly string _region;
        private readonly ILogger<S3FileUploadService> _logger;

        public S3FileUploadService(
            IAmazonS3 s3Client,
            IOptions<BlobStorageOptions> options,
            ILogger<S3FileUploadService> logger)
        {
            _s3Client = s3Client;
            var options1 = options.Value;
            _bucketName = options1.Bucket?.Name ?? 
                throw new ArgumentException("Bucket Name configuration is missing");
                
            // Get region from options
            _region = options1.Region ?? "nyc3";
            
            // Base domain for CDN without region prefix
            _cdnBaseDomain = options1.Bucket?.CdnBaseDomain ?? 
                "cdn.digitaloceanspaces.com";
            _logger = logger;
            
            // Log the service URL, bucket and CDN setup for debugging
            _logger.LogInformation("Initialized S3FileUploadService with service URL: {ServiceUrl}, bucket: {BucketName}, region: {Region}, CDN base domain: {CdnBaseDomain}",
                options1.ServiceURL, _bucketName, _region, _cdnBaseDomain);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder, string fileName = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            try
            {
                // Generate unique file name if not provided
                fileName = string.IsNullOrEmpty(fileName) ? $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}" :
                    // Ensure the extension is maintained
                    $"{fileName}{Path.GetExtension(file.FileName)}";

                // Combine folder and filename for the full S3 key
                var s3Key = string.IsNullOrEmpty(folder) ? fileName : $"{folder.TrimEnd('/')}/{fileName}";

                _logger.LogInformation("Attempting to upload file to S3 with key: {S3Key}", s3Key);

                // Use TransferUtility which manages large files better
                using var transferUtility = new TransferUtility(_s3Client);
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = memoryStream,
                    BucketName = _bucketName,
                    Key = s3Key,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.Private // Or PublicRead if files should be publicly accessible
                };

                await transferUtility.UploadAsync(uploadRequest);
                _logger.LogInformation("Successfully uploaded file to S3 with key: {S3Key}", s3Key);

                // Return the CDN URL instead of just the S3 key
                return GetCdnUrl(s3Key);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 Error uploading file to S3. StatusCode: {StatusCode}, Message: {Message}, ErrorCode: {ErrorCode}", 
                    ex.StatusCode, ex.Message, ex.ErrorCode);
                throw new Exception($"Error uploading file to S3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading file to S3: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentException("File URL cannot be null or empty", nameof(fileUrl));
            }

            try
            {
                // Extract the S3 key from the CDN URL if needed
                string s3Key = ExtractS3KeyFromUrl(fileUrl);
                
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key
                };

                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                _logger.LogInformation("Successfully deleted file from S3 with key: {FileKey}", s3Key);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 Error deleting file from S3: {Message}", ex.Message);
                throw new Exception($"Error deleting file from S3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting file from S3: {Message}", ex.Message);
                throw;
            }
        }

        public bool IsValidFile(IFormFile file, string[] allowedExtensions, long maxSizeInBytes)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            if (file.Length > maxSizeInBytes)
            {
                return false;
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            foreach (var extension in allowedExtensions)
            {
                if (fileExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<string> GetPreSignedUrlAsync(string fileKey, int expirationMinutes = 15)
        {
            try
            {
                // Extract the S3 key from the CDN URL if needed
                string s3Key = ExtractS3KeyFromUrl(fileKey);
                
                // Log the original file key
                _logger.LogInformation("Original file key: {FileKey}", s3Key);
        
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    Protocol = Protocol.HTTPS,
                    Verb = HttpVerb.GET
                };
        
                var url = _s3Client.GetPreSignedURL(request);
                _logger.LogInformation("Generated pre-signed URL before fix: {Url}", url);
        
                // Fix the doubled bucket name in the URL
                var fixedUrl = url;
                if (url.Contains($"{_bucketName}.{_bucketName}."))
                {
                    fixedUrl = url.Replace($"{_bucketName}.{_bucketName}.", $"{_bucketName}.");
                    _logger.LogInformation("Fixed URL with doubled bucket name: {Url}", fixedUrl);
                }
        
                return fixedUrl;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 Error getting pre-signed URL: {Message}", ex.Message);
                throw new Exception($"Error getting pre-signed URL: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting pre-signed URL: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Converts an S3 key to a full CDN URL
        /// </summary>
        /// <param name="s3Key">The S3 object key</param>
        /// <returns>Full CDN URL for the object</returns>
        public string GetCdnUrl(string s3Key)
        {
            if (string.IsNullOrEmpty(s3Key))
            {
                throw new ArgumentException("S3 key cannot be null or empty", nameof(s3Key));
            }

            // For DigitalOcean Spaces CDN, the format is:
            // https://bucket-name.region.cdn.digitaloceanspaces.com/path/to/file.jpg
            return $"https://{_bucketName}.{_region}.{_cdnBaseDomain}/{s3Key}";
        }

        /// <summary>
        /// Extracts the S3 key from a CDN URL
        /// </summary>
        /// <param name="url">The CDN URL or S3 key</param>
        /// <returns>The S3 object key</returns>
        private string ExtractS3KeyFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            // If it's already just an S3 key, return it as is
            if (!url.StartsWith("http"))
            {
                return url;
            }

            try
            {
                // Parse the URL
                var uri = new Uri(url);
                
                // Get the path part without the leading slash
                var path = uri.AbsolutePath.TrimStart('/');
                
                // For Digital Ocean CDN, we just need the path
                // No need to check for bucket name in the path for DO CDN
                return path;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting S3 key from URL: {Url}", url);
                throw new ArgumentException($"Invalid URL format: {url}", nameof(url), ex);
            }
        }
    }