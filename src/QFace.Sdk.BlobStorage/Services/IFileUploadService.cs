using Microsoft.AspNetCore.Http;

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
    Task<FileUploadResponse> UploadFileAsync(IFormFile file, string folder, string fileName = null, bool isPublic = false);
        
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
    
    /// <summary>
    /// Uploads a Base64 encoded image to blob storage
    /// </summary>
    /// <param name="base64Image">The Base64 encoded image string (with or without data URI prefix)</param>
    /// <param name="folder">Optional folder path within the storage</param>
    /// <param name="fileName">Optional file name (if not provided, a unique name will be generated)</param>
    /// <param name="contentType">The content type of the image (e.g., "image/jpeg", "image/png")</param>
    /// <returns>URL of the uploaded file</returns>
    Task<FileUploadResponse> UploadBase64ImageAsync(string base64Image, string folder, string fileName = null, string contentType = null, bool isPublic = false);
    
    /// <summary>
    /// Uploads a file to blob storage as a public file (accessible directly via CDN URL)
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Optional folder path within the storage</param>
    /// <param name="fileName">Optional file name (if not provided, a unique name will be generated)</param>
    /// <returns>CDN URL of the uploaded file (publicly accessible)</returns>
    Task<FileUploadResponse> UploadPublicFileAsync(IFormFile file, string folder, string fileName = null);
    
    /// <summary>
    /// Uploads a Base64 encoded image to blob storage as a public file (accessible directly via CDN URL)
    /// </summary>
    /// <param name="base64Image">The Base64 encoded image string (with or without data URI prefix)</param>
    /// <param name="folder">Optional folder path within the storage</param>
    /// <param name="fileName">Optional file name (if not provided, a unique name will be generated)</param>
    /// <param name="contentType">The content type of the image (e.g., "image/jpeg", "image/png")</param>
    /// <returns>CDN URL of the uploaded file (publicly accessible)</returns>
    Task<FileUploadResponse> UploadPublicBase64ImageAsync(string base64Image, string folder, string fileName = null, string contentType = null);
    
    /// <summary>
    /// Uploads a file to blob storage as a private file (requires pre-signed URL for access)
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Optional folder path within the storage</param>
    /// <param name="fileName">Optional file name (if not provided, a unique name will be generated)</param>
    /// <returns>CDN URL of the uploaded file (requires pre-signed URL for access)</returns>
    Task<FileUploadResponse> UploadPrivateFileAsync(IFormFile file, string folder, string fileName = null);
    
    /// <summary>
    /// Uploads a Base64 encoded image to blob storage as a private file (requires pre-signed URL for access)
    /// </summary>
    /// <param name="base64Image">The Base64 encoded image string (with or without data URI prefix)</param>
    /// <param name="folder">Optional folder path within the storage</param>
    /// <param name="fileName">Optional file name (if not provided, a unique name will be generated)</param>
    /// <param name="contentType">The content type of the image (e.g., "image/jpeg", "image/png")</param>
    /// <returns>CDN URL of the uploaded file (requires pre-signed URL for access)</returns>
    Task<FileUploadResponse> UploadPrivateBase64ImageAsync(string base64Image, string folder, string fileName = null, string contentType = null);

    /// <summary>
    /// Gets the text content of an object from blob storage.
    /// </summary>
    /// <param name="s3Key">The S3 object key (e.g. "templates/emails/WorkflowApproval.html")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The object content as string, or null if not found</returns>
    Task<string?> GetObjectContentAsync(string s3Key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads text content to blob storage.
    /// </summary>
    /// <param name="content">The content to upload</param>
    /// <param name="s3Key">The S3 object key</param>
    /// <param name="contentType">Content type (default: text/html)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadContentAsync(string content, string s3Key, string contentType = "text/html", CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists object keys under the given prefix.
    /// </summary>
    /// <param name="prefix">The S3 prefix (e.g. "templates/emails")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of full S3 keys under the prefix</returns>
    Task<IReadOnlyList<string>> ListObjectKeysAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads the source <paramref name="file"/> as five public S3 objects:
    /// original + four WebP variants (thumb 48, sm 96, md 256, lg 512).
    /// Returns CDN URLs for all variants.
    /// </summary>
    Task<ProfileImageVariants> UploadProfileImageVariantsAsync(
        IFormFile file,
        string folder,
        string baseFileName,
        CancellationToken cancellationToken = default);
}