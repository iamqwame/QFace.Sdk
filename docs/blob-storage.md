# QFace.Sdk.BlobStorage

This package provides a convenient way to upload, manage, and serve files using AWS S3-compatible storage (including DigitalOcean Spaces) with CDN support.

## Features

- Upload files from IFormFile or Base64 encoded images
- Support for both public and private file access
- CDN URL generation for optimal performance
- Pre-signed URL generation for private files
- File validation and size limits
- Automatic file deletion capabilities

## Installation

```shell
dotnet add package QFace.Sdk.BlobStorage
```

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "BlobStorage": {
    "ServiceURL": "https://nyc3.digitaloceanspaces.com",
    "Region": "nyc3",
    "Bucket": {
      "Name": "your-bucket-name",
      "CdnBaseDomain": "cdn.digitaloceanspaces.com"
    },
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key"
  }
}
```

## Usage

### Registration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddBlobStorageServices(builder.Configuration);
```

### Basic Usage

#### Private File Upload (Default)

```csharp
public class FileController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public FileController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, string folder = "documents")
    {
        // Upload as private file (default behavior)
        var result = await _fileUploadService.UploadFileAsync(file, folder);
        
        return Ok(new { 
            CdnUrl = result.SaveUrl,      // CDN URL (requires pre-signed URL for access)
            PreSignedUrl = result.Url     // Pre-signed URL for temporary access
        });
    }
}
```

#### Public File Upload

```csharp
[HttpPost("upload-profile-picture")]
public async Task<IActionResult> UploadProfilePicture(IFormFile file, string userId)
{
    // Upload as public file - accessible directly via CDN URL
    var result = await _fileUploadService.UploadFileAsync(
        file, 
        $"profiles/{userId}", 
        "avatar.jpg", 
        isPublic: true);
    
    return Ok(new { 
        ProfilePictureUrl = result.SaveUrl  // Directly accessible CDN URL
    });
}
```

### Base64 Image Upload

```csharp
[HttpPost("upload-base64")]
public async Task<IActionResult> UploadBase64Image([FromBody] Base64UploadRequest request)
{
    // Private upload (default)
    var privateResult = await _fileUploadService.UploadBase64ImageAsync(
        request.Base64Image, 
        "images", 
        request.FileName);
    
    // Public upload
    var publicResult = await _fileUploadService.UploadBase64ImageAsync(
        request.Base64Image, 
        "public/images", 
        request.FileName, 
        contentType: "image/jpeg",
        isPublic: true);
    
    return Ok(new { 
        PrivateUrl = privateResult.SaveUrl,
        PublicUrl = publicResult.SaveUrl 
    });
}
```

## Public vs Private Files

### When to Use Public Files (`isPublic: true`)

- **Profile pictures** - Need to be accessible without authentication
- **Public assets** - Logos, banners, shared media
- **Public documents** - Terms of service, privacy policy
- **Product images** - E-commerce product photos

### When to Use Private Files (`isPublic: false` - Default)

- **Personal documents** - User-uploaded sensitive files
- **Private media** - Personal photos, confidential documents
- **Temporary files** - Processing files, backups
- **Admin-only content** - Internal documents, reports

## File Access Patterns

### Public Files
```csharp
// Upload as public
var result = await _fileUploadService.UploadFileAsync(file, "public", isPublic: true);

// Access directly via CDN URL - no authentication needed
var imageUrl = result.SaveUrl; // https://bucket.region.cdn.digitaloceanspaces.com/public/image.jpg
```

### Private Files
```csharp
// Upload as private (default)
var result = await _fileUploadService.UploadFileAsync(file, "private");

// Access via pre-signed URL (temporary, expires)
var temporaryUrl = result.Url; // Pre-signed URL valid for 15 minutes by default

// Or generate new pre-signed URL
var newUrl = await _fileUploadService.GetPreSignedUrlAsync(result.SaveUrl, expirationMinutes: 60);
```

## File Management

### Delete Files

```csharp
[HttpDelete("delete")]
public async Task<IActionResult> DeleteFile(string fileUrl)
{
    await _fileUploadService.DeleteFileAsync(fileUrl);
    return Ok();
}
```

### File Validation

```csharp
[HttpPost("upload-validated")]
public async Task<IActionResult> UploadValidatedFile(IFormFile file)
{
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
    var maxSize = 10 * 1024 * 1024; // 10MB
    
    if (!_fileUploadService.IsValidFile(file, allowedExtensions, maxSize))
    {
        return BadRequest("Invalid file type or size");
    }
    
    var result = await _fileUploadService.UploadFileAsync(file, "validated");
    return Ok(result);
}
```

## Advanced Configuration

### Custom CDN Settings

```json
{
  "BlobStorage": {
    "ServiceURL": "https://nyc3.digitaloceanspaces.com",
    "Region": "nyc3",
    "Bucket": {
      "Name": "my-app-storage",
      "CdnBaseDomain": "cdn.digitaloceanspaces.com"
    }
  }
}
```

### Error Handling

```csharp
try
{
    var result = await _fileUploadService.UploadFileAsync(file, "uploads", isPublic: true);
    return Ok(result);
}
catch (ArgumentException ex)
{
    return BadRequest($"Invalid file: {ex.Message}");
}
catch (Exception ex)
{
    _logger.LogError(ex, "File upload failed");
    return StatusCode(500, "Upload failed");
}
```

## Best Practices

1. **Use appropriate ACL settings**: Public for assets that need direct access, private for sensitive content
2. **Validate file types and sizes** before upload
3. **Use descriptive folder structures** for organization
4. **Handle errors gracefully** with proper logging
5. **Consider CDN caching** for public assets
6. **Clean up unused files** to manage storage costs

## Migration from Private to Public

If you need to change a file's access level after upload, you would need to:

1. Re-upload the file with the correct `isPublic` setting
2. Update your database records with the new URL
3. Delete the old file if no longer needed

Note: AWS S3 doesn't support changing ACL after upload, so re-uploading is required.