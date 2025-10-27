using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.BlobStorage.Extensions;
using QFace.Sdk.BlobStorage.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Blob Storage API", 
        Version = "v1" 
    });
});

// Configure Blob Storage Services
builder.Services.AddBlobStorageServices(builder.Configuration);

// Add logging
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blob Storage API v1"));
}

app.UseHttpsRedirection();

// File Upload Endpoint
app.MapPost("/api/upload", async (
    IFormFile file, 
    IFileUploadService fileUploadService, 
    ILogger<Program> logger,
    string? folder = null, 
    string? fileName = null) =>
{
    try
    {
        // Validate file (basic validation, you can extend this)
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { Message = "No file uploaded" });

        // You can add more specific validation if needed
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var maxFileSize = 5 * 1024 * 1024; // 5MB

        if (!fileUploadService.IsValidFile(file, allowedExtensions, maxFileSize))
            return Results.BadRequest(new { Message = "Invalid file type or size" });

        // Upload as private file using dedicated method
        var result = await fileUploadService.UploadPrivateFileAsync(
            file, 
            folder ?? "uploads", 
            fileName);

        return Results.Ok(new { 
            Message = "File uploaded as private",
            CdnUrl = result.SaveUrl,  // CDN URL (requires pre-signed URL for access)
            PreSignedUrl = result.Url  // Pre-signed URL for temporary access
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "File upload failed");
        return Results.StatusCode(500);
    }
})
.DisableAntiforgery()
.WithName("UploadFile")
.WithOpenApi();

// Public File Upload Endpoint (e.g., for profile pictures)
app.MapPost("/api/upload-public", async (
    IFormFile file, 
    IFileUploadService fileUploadService, 
    ILogger<Program> logger,
    string? folder = null, 
    string? fileName = null) =>
{
    try
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { Message = "No file uploaded" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var maxFileSize = 5 * 1024 * 1024; // 5MB

        if (!fileUploadService.IsValidFile(file, allowedExtensions, maxFileSize))
            return Results.BadRequest(new { Message = "Invalid file type or size" });

        // Upload as public file
        var result = await fileUploadService.UploadPublicFileAsync(
            file, 
            folder ?? "public/uploads", 
            fileName);

        return Results.Ok(new { 
            Message = "File uploaded as public",
            CdnUrl = result.SaveUrl  // This is now publicly accessible - no pre-signed URL needed!
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Public file upload failed");
        return Results.StatusCode(500);
    }
})
.DisableAntiforgery()
.WithName("UploadPublicFile")
.WithOpenApi();

// Get Pre-Signed URL Endpoint
app.MapGet("/api/presigned-url", async (
    string fileKey, 
    IFileUploadService fileUploadService, 
    ILogger<Program> logger,
    int expirationMinutes = 15) =>
{
    try
    {
        var preSignedUrl = await fileUploadService.GetPreSignedUrlAsync(
            fileKey, 
            expirationMinutes);

        return Results.Ok(new { PreSignedUrl = preSignedUrl });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to generate pre-signed URL");
        return Results.StatusCode(500);
    }
})
.WithName("GetPreSignedUrl")
.WithOpenApi();

// Delete File Endpoint
app.MapDelete("/api/delete", async (
    string fileUrl, 
    IFileUploadService fileUploadService, 
    ILogger<Program> logger) =>
{
    try
    {
        await fileUploadService.DeleteFileAsync(fileUrl);
        return Results.Ok(new { Message = "File deleted successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "File deletion failed");
        return Results.StatusCode(500);
    }
})
.WithName("DeleteFile")
.WithOpenApi();


// Base64 Image Upload Endpoint
app.MapPost("/api/upload-base64", async (
        [FromBody] Base64UploadRequest request,
        IFileUploadService fileUploadService,
        ILogger<Program> logger) =>
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Base64Image))
                return Results.BadRequest(new { Message = "No base64 image data provided" });

            // Upload the base64 image as private
            var result = await fileUploadService.UploadPrivateBase64ImageAsync(
                request.Base64Image,
                request.Folder ?? "uploads",
                request.FileName,
                request.ContentType);

            return Results.Ok(new { 
                Message = "Base64 image uploaded as private",
                CdnUrl = result.SaveUrl,  // CDN URL (requires pre-signed URL for access)
                PreSignedUrl = result.Url  // Pre-signed URL for temporary access
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid base64 image data");
            return Results.BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Base64 image upload failed");
            return Results.StatusCode(500);
        }
    })
    .DisableAntiforgery()
    .WithName("UploadBase64Image")
    .WithOpenApi(operation => {
        operation.Description = "Uploads a Base64 encoded image to blob storage";
        return operation;
    });

// Public Base64 Image Upload Endpoint
app.MapPost("/api/upload-public-base64", async (
        [FromBody] Base64UploadRequest request,
        IFileUploadService fileUploadService,
        ILogger<Program> logger) =>
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Base64Image))
                return Results.BadRequest(new { Message = "No base64 image data provided" });

            // Upload as public base64 image
            var result = await fileUploadService.UploadPublicBase64ImageAsync(
                request.Base64Image,
                request.Folder ?? "public/images",
                request.FileName,
                request.ContentType);

            return Results.Ok(new { 
                Message = "Base64 image uploaded as public",
                CdnUrl = result.SaveUrl  // Directly accessible CDN URL
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid base64 image data");
            return Results.BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Public base64 image upload failed");
            return Results.StatusCode(500);
        }
    })
    .DisableAntiforgery()
.WithName("UploadPublicBase64Image")
.WithOpenApi(operation => {
    operation.Description = "Uploads a Base64 encoded image as a public file (accessible directly via CDN URL)";
    return operation;
});

// Legacy Private File Upload Endpoint (for backward compatibility demonstration)
app.MapPost("/api/upload-legacy", async (
    IFormFile file, 
    IFileUploadService fileUploadService, 
    ILogger<Program> logger,
    string? folder = null, 
    string? fileName = null) =>
{
    try
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { Message = "No file uploaded" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
        var maxFileSize = 10 * 1024 * 1024; // 10MB

        if (!fileUploadService.IsValidFile(file, allowedExtensions, maxFileSize))
            return Results.BadRequest(new { Message = "Invalid file type or size" });

        // Upload using legacy method (private by default)
        var result = await fileUploadService.UploadFileAsync(
            file, 
            folder ?? "legacy/uploads", 
            fileName);

        return Results.Ok(new { 
            Message = "File uploaded using legacy method (private by default)",
            Method = "UploadFileAsync() - Legacy",
            CdnUrl = result.SaveUrl,  // CDN URL (requires pre-signed URL for access)
            PreSignedUrl = result.Url  // Pre-signed URL for temporary access
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Legacy file upload failed");
        return Results.StatusCode(500);
    }
})
.DisableAntiforgery()
.WithName("UploadLegacyFile")
.WithOpenApi(operation => {
    operation.Description = "Demonstrates legacy UploadFileAsync() method (private by default)";
    return operation;
});

// Legacy Private Base64 Upload Endpoint (for backward compatibility demonstration)
app.MapPost("/api/upload-legacy-base64", async (
        [FromBody] Base64UploadRequest request,
        IFileUploadService fileUploadService,
        ILogger<Program> logger) =>
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.Base64Image))
                return Results.BadRequest(new { Message = "No base64 image data provided" });

            // Upload using legacy method (private by default)
            var result = await fileUploadService.UploadBase64ImageAsync(
                request.Base64Image,
                request.Folder ?? "legacy/images",
                request.FileName,
                request.ContentType);

            return Results.Ok(new { 
                Message = "Base64 image uploaded using legacy method (private by default)",
                Method = "UploadBase64ImageAsync() - Legacy",
                CdnUrl = result.SaveUrl,  // CDN URL (requires pre-signed URL for access)
                PreSignedUrl = result.Url  // Pre-signed URL for temporary access
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid base64 image data");
            return Results.BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Legacy base64 image upload failed");
            return Results.StatusCode(500);
        }
    })
    .DisableAntiforgery()
    .WithName("UploadLegacyBase64Image")
    .WithOpenApi(operation => {
        operation.Description = "Demonstrates legacy UploadBase64ImageAsync() method (private by default)";
        return operation;
    });

// API Methods Summary Endpoint
app.MapGet("/api/methods", () =>
{
    return Results.Ok(new
    {
        Title = "QFace.Sdk.BlobStorage - Complete API Methods",
        Description = "This demo showcases all available upload methods",
        Methods = new[]
        {
            new { 
                Method = "UploadFileAsync()", 
                Endpoint = "POST /api/upload-legacy", 
                Type = "Legacy", 
                Access = "Private (default)", 
                Description = "Original method - private by default" 
            },
            new { 
                Method = "UploadBase64ImageAsync()", 
                Endpoint = "POST /api/upload-legacy-base64", 
                Type = "Legacy", 
                Access = "Private (default)", 
                Description = "Original Base64 method - private by default" 
            },
            new { 
                Method = "UploadPrivateFileAsync()", 
                Endpoint = "POST /api/upload", 
                Type = "Explicit", 
                Access = "Private", 
                Description = "Explicit private file upload" 
            },
            new { 
                Method = "UploadPrivateBase64ImageAsync()", 
                Endpoint = "POST /api/upload-base64", 
                Type = "Explicit", 
                Access = "Private", 
                Description = "Explicit private Base64 upload" 
            },
            new { 
                Method = "UploadPublicFileAsync()", 
                Endpoint = "POST /api/upload-public", 
                Type = "Explicit", 
                Access = "Public", 
                Description = "Explicit public file upload (direct CDN access)" 
            },
            new { 
                Method = "UploadPublicBase64ImageAsync()", 
                Endpoint = "POST /api/upload-public-base64", 
                Type = "Explicit", 
                Access = "Public", 
                Description = "Explicit public Base64 upload (direct CDN access)" 
            }
        },
        Usage = new
        {
            Recommendation = "Use explicit methods (UploadPrivate* and UploadPublic*) for clarity",
            Legacy = "Legacy methods still work for backward compatibility",
            PublicFiles = "Profile pictures, logos, public assets - use UploadPublic* methods",
            PrivateFiles = "Documents, personal data, sensitive content - use UploadPrivate* methods"
        }
    });
})
.WithName("GetApiMethods")
.WithOpenApi(operation => {
    operation.Description = "Returns a summary of all available upload methods";
    return operation;
});

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }


public class Base64UploadRequest
{
    public string Base64Image { get; set; }
    public string Folder { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
}