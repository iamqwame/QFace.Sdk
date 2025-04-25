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

        // Upload the file
        var fileUrl = await fileUploadService.UploadFileAsync(
            file, 
            folder ?? "uploads", 
            fileName);

        return Results.Ok(new { FileUrl = fileUrl });
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

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }