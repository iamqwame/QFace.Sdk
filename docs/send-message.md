# QFace.Sdk.BlobStorage

This package provides a convenient way to interact with S3-compatible blob storage services (like AWS S3, DigitalOcean Spaces, etc.) in .NET applications.

## Features

- Upload files to S3-compatible storage
- Delete files from storage
- Generate pre-signed URLs for temporary access
- Validate files based on size and extension
- Convert between S3 keys and CDN URLs

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
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "Bucket": {
      "Name": "your-bucket-name",
      "CdnBaseDomain": "cdn.digitaloceanspaces.com"
    }
  }
}
```

## Usage

### Registration

```csharp
// In Program.cs or Startup.cs
builder.Services.AddBlobStorage(builder.Configuration);
```

### Basic Usage

```csharp
// Inject IFileUploadService into your controllers or services
public class FileController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public FileController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (!_fileUploadService.IsValidFile(file, new[] { ".jpg", ".png", ".pdf" }, 5 * 1024 * 1024))
        {
            return BadRequest("Invalid file type or size");
        }

        var fileUrl = await _fileUploadService.UploadFileAsync(file, "uploads");
        return Ok(new { url = fileUrl });
    }

    [HttpGet("download/{fileKey}")]
    public async Task<IActionResult> GetDownloadUrl(string fileKey)
    {
        var presignedUrl = await _fileUploadService.GetPreSignedUrlAsync(fileKey, 30);
        return Ok(new { downloadUrl = presignedUrl });
    }

    [HttpDelete("{fileKey}")]
    public async Task<IActionResult> DeleteFile(string fileKey)
    {
        await _fileUploadService.DeleteFileAsync(fileKey);
        return Ok();
    }
}
```

## Advanced Use Cases

### Custom File Naming

```csharp
// Generate a custom file name based on a user ID
var userId = "user123";
var customFileName = $"{userId}-profile-picture";
var fileUrl = await _fileUploadService.UploadFileAsync(file, "profiles", customFileName);
```

### Working with CDN URLs

```csharp
// Convert an S3 key to a CDN URL
var cdnUrl = _fileUploadService.GetCdnUrl("uploads/image.jpg");

// Extract S3 key from URL (handled internally by the service)
await _fileUploadService.DeleteFileAsync(cdnUrl);
```
