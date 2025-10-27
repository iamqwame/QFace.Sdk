using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QFace.Sdk.BlobStorage.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _cdnBaseDomain;
    private readonly string _region;
    private readonly ILogger<FileUploadService> _logger;
    private readonly BlobStorageOptions _options;

    public FileUploadService(
        IAmazonS3 s3Client,
        IOptions<BlobStorageOptions> options,
        ILogger<FileUploadService> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _bucketName = _options.Bucket?.Name ?? 
                      throw new ArgumentException("Bucket Name configuration is missing");
                
        // Get region from options
        _region = _options.Region ?? "nyc3";
            
        // Base domain for CDN without region prefix
        _cdnBaseDomain = _options.Bucket?.CdnBaseDomain ?? 
                         "cdn.digitaloceanspaces.com";
        _logger = logger;
            
        // Log the service URL, bucket and CDN setup for debugging
        _logger.LogInformation("Initialized S3FileUploadService with service URL: {ServiceUrl}, bucket: {BucketName}, region: {Region}, CDN base domain: {CdnBaseDomain}",
            _options.ServiceURL, _bucketName, _region, _cdnBaseDomain);
    }

    public async Task<FileUploadResponse> UploadFileAsync(IFormFile file, string folder, string fileName = null, bool isPublic = false)
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
                CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
            };

            await transferUtility.UploadAsync(uploadRequest);
            _logger.LogInformation("Successfully uploaded file to S3 with key: {S3Key}", s3Key);


            var s3KeyUrl = GetCdnUrl(s3Key);
            var singUrl = await GetPreSignedUrlAsync(
                s3KeyUrl,60
                );
            // Return the CDN URL instead of just the S3 key
            return new FileUploadResponse
            {
                SaveUrl =s3KeyUrl,
                Url = singUrl,
            };
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
    
     /// <summary>
        /// Uploads a Base64 encoded image to blob storage
        /// </summary>
        /// <param name="base64Image">The Base64 encoded image string (with or without data URI prefix)</param>
        /// <param name="folder">Optional folder path within the storage</param>
        /// <param name="fileName">Optional file name (if not provided, a unique name will be generated)</param>
        /// <param name="contentType">The content type of the image (e.g., "image/jpeg", "image/png")</param>
        /// <returns>URL of the uploaded file</returns>
        public async Task<FileUploadResponse> UploadBase64ImageAsync(string base64Image, string folder, string fileName = null, string contentType = null, bool isPublic = false)
        {
            if (string.IsNullOrEmpty(base64Image))
            {
                throw new ArgumentException("Base64 image string is empty or null", nameof(base64Image));
            }

            try
            {
                // Remove data URI prefix if present (e.g., "data:image/jpeg;base64,")
                string base64Data = base64Image;
                string extractedContentType = null;
                
                if (base64Image.Contains(","))
                {
                    var parts = base64Image.Split(',');
                    if (parts.Length > 1)
                    {
                        base64Data = parts[1];
                        
                        // Try to extract content type from the data URI
                        if (parts[0].Contains("data:") && parts[0].Contains(";base64"))
                        {
                            extractedContentType = parts[0].Substring(5, parts[0].IndexOf(";") - 5);
                            _logger.LogInformation("Extracted content type from data URI: {ContentType}", extractedContentType);
                        }
                    }
                }
                
                // Use extracted content type if explicit one is not provided
                if (string.IsNullOrEmpty(contentType) && !string.IsNullOrEmpty(extractedContentType))
                {
                    contentType = extractedContentType;
                }
                
                // Default to image/jpeg if content type is still not determined
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = "image/jpeg";
                    _logger.LogInformation("No content type provided, defaulting to: {ContentType}", contentType);
                }
                
                // Get file extension from content type
                string extension = DetermineFileExtension(contentType);
                
                // Generate unique file name if not provided
                fileName = string.IsNullOrEmpty(fileName) 
                    ? $"{Guid.NewGuid()}{extension}" 
                    : $"{fileName}{extension}";

                // Combine folder and filename for the full S3 key
                var s3Key = string.IsNullOrEmpty(folder) ? fileName : $"{folder.TrimEnd('/')}/{fileName}";

                _logger.LogInformation("Attempting to upload Base64 image to S3 with key: {S3Key}", s3Key);

                // Convert Base64 to byte array
                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(base64Data);
                    _logger.LogInformation("Successfully converted Base64 string to byte array, size: {Size} bytes", imageBytes.Length);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Invalid Base64 string format");
                    throw new ArgumentException("Invalid Base64 string format", nameof(base64Image), ex);
                }

                // Use TransferUtility which manages large files better
                using var transferUtility = new TransferUtility(_s3Client);
                using var memoryStream = new MemoryStream(imageBytes);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = memoryStream,
                    BucketName = _bucketName,
                    Key = s3Key,
                    ContentType = contentType,
                    CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
                };

                await transferUtility.UploadAsync(uploadRequest);
                _logger.LogInformation("Successfully uploaded Base64 image to S3 with key: {S3Key}", s3Key);

                var s3KeyUrl = GetCdnUrl(s3Key);
                var signedUrl = await GetPreSignedUrlAsync(s3KeyUrl, 60);
                
                // Return the CDN URL and signed URL
                return new FileUploadResponse
                {
                    SaveUrl = s3KeyUrl,
                    Url = signedUrl,
                };
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "AWS S3 Error uploading Base64 image to S3. StatusCode: {StatusCode}, Message: {Message}, ErrorCode: {ErrorCode}", 
                    ex.StatusCode, ex.Message, ex.ErrorCode);
                throw new Exception($"Error uploading Base64 image to S3: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading Base64 image to S3: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Determines the file extension based on content type
        /// </summary>
        private string DetermineFileExtension(string contentType)
        {
            string extension = ".jpg"; // Default
            switch (contentType.ToLower())
            {
                case "image/png":
                    extension = ".png";
                    break;
                case "image/gif":
                    extension = ".gif";
                    break;
                case "image/bmp":
                    extension = ".bmp";
                    break;
                case "image/webp":
                    extension = ".webp";
                    break;
                case "image/svg+xml":
                    extension = ".svg";
                    break;
                case "image/jpeg":
                case "image/jpg":
                    extension = ".jpg";
                    break;
            }
            _logger.LogInformation("Determined file extension for content type {ContentType}: {Extension}", contentType, extension);
            return extension;
        }

    /// <summary>
    /// Uploads a file to blob storage as a public file (accessible directly via CDN URL)
    /// </summary>
    public async Task<FileUploadResponse> UploadPublicFileAsync(IFormFile file, string folder, string fileName = null)
    {
        return await UploadFileAsync(file, folder, fileName, isPublic: true);
    }

    /// <summary>
    /// Uploads a Base64 encoded image to blob storage as a public file (accessible directly via CDN URL)
    /// </summary>
    public async Task<FileUploadResponse> UploadPublicBase64ImageAsync(string base64Image, string folder, string fileName = null, string contentType = null)
    {
        return await UploadBase64ImageAsync(base64Image, folder, fileName, contentType, isPublic: true);
    }

    /// <summary>
    /// Uploads a file to blob storage as a private file (requires pre-signed URL for access)
    /// </summary>
    public async Task<FileUploadResponse> UploadPrivateFileAsync(IFormFile file, string folder, string fileName = null)
    {
        return await UploadFileAsync(file, folder, fileName, isPublic: false);
    }

    /// <summary>
    /// Uploads a Base64 encoded image to blob storage as a private file (requires pre-signed URL for access)
    /// </summary>
    public async Task<FileUploadResponse> UploadPrivateBase64ImageAsync(string base64Image, string folder, string fileName = null, string contentType = null)
    {
        return await UploadBase64ImageAsync(base64Image, folder, fileName, contentType, isPublic: false);
    }
}