using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using QFace.Sdk.BlobStorage.Services;

namespace QimErp.Shared.Common.Features.Files;

public static class UploadFile
{
    public record Command(
        IFormFile File,
        string FileType,
        string? Context = null) : IRequest<Result<FileUploadResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        private static readonly string[] ValidFileTypes = 
        {
            "survey-image",
            "news-image",
            "profile-picture",
            "document",
            "evidence",
            "goal-evidence",
            "training-material"
        };

        public Validator()
        {
            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required");

            RuleFor(x => x.FileType)
                .NotEmpty().WithMessage("FileType is required")
                .Must(BeValidFileType)
                .WithMessage($"Invalid file type. Valid types: {string.Join(", ", ValidFileTypes)}");

            RuleFor(x => x.File)
                .Must(BeValidImageFile)
                .When(x => x.FileType.Contains("image") || x.FileType.Contains("picture"))
                .WithMessage("File must be a valid image (jpg, jpeg, png, gif, webp) under 5MB");

            RuleFor(x => x.File)
                .Must(BeValidDocumentFile)
                .When(x => x.FileType == "document" || x.FileType.Contains("material"))
                .WithMessage("File must be a valid document (pdf, doc, docx, xls, xlsx) under 10MB");
        }

        private static bool BeValidFileType(string fileType) => ValidFileTypes.Contains(fileType);

        private static bool BeValidImageFile(IFormFile? file)
        {
            if (file == null) return false;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var maxSizeBytes = 5 * 1024 * 1024; // 5MB
            return allowedExtensions.Contains(extension) && file.Length <= maxSizeBytes;
        }

        private static bool BeValidDocumentFile(IFormFile? file)
        {
            if (file == null) return false;
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var maxSizeBytes = 10 * 1024 * 1024; // 10MB
            return allowedExtensions.Contains(extension) && file.Length <= maxSizeBytes;
        }
    }

    internal sealed class Handler(
        IFileUploadService fileUploadService,
        ILogger<Handler> logger)
        : IRequestHandler<Command, Result<FileUploadResponse>>
    {
        public async Task<Result<FileUploadResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("📤 [File Upload] Uploading {FileType}: {FileName}", 
                    request.FileType, request.File.FileName);

                // Determine folder based on file type
                var folder = GetFolderPath(request.FileType, request.Context);
                var fileName = $"{request.FileType}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid():N}";

                // Upload to S3 or blob storage
                var uploadResult = await fileUploadService.UploadPublicFileAsync(
                    request.File, 
                    folder, 
                    fileName);

                var response = new FileUploadResponse
                {
                    Url = uploadResult.SaveUrl,
                    S3Key = uploadResult.SaveUrl,
                    FileName = request.File.FileName,
                    FileSize = request.File.Length,
                    ContentType = request.File.ContentType,
                    UploadedAt = DateTime.UtcNow
                };

                logger.LogInformation("✅ [File Upload Success] File uploaded: {FileName} -> {Url}", 
                    request.File.FileName, uploadResult.SaveUrl);

                return Result.WithSuccess(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ [File Upload Error] Failed to upload file: {FileName}", 
                    request.File.FileName);
                return Result.WithFailure<FileUploadResponse>(
                    new Error("UploadFile.Error", $"Failed to upload file: {ex.Message}"));
            }
        }

        private static string GetFolderPath(string fileType, string? context)
        {
            return fileType switch
            {
                "survey-image" => $"surveys/images/{context ?? "general"}",
                "news-image" => "news/images",
                "profile-picture" => "employees/profile-pictures",
                "document" => $"documents/{context ?? "general"}",
                "evidence" => "performance/evidence",
                "goal-evidence" => "performance/goals/evidence",
                "training-material" => "learning/materials",
                _ => throw new ArgumentException($"Invalid file type: {fileType}")
            };
        }
    }
}

public class UploadFileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/files/upload", [Authorize] async (
            HttpRequest request,
            ISender sender,
            ILogger<UploadFileEndpoint> logger) =>
        {
            try
            {
                if (!request.HasFormContentType)
                {
                    return Results.BadRequest(new 
                    { 
                        IsSuccess = false, 
                        Message = "Request must be multipart/form-data" 
                    });
                }

                var form = await request.ReadFormAsync();
                var file = form.Files["file"];
                var fileType = form["fileType"].ToString();
                var context = form["context"].ToString();

                if (file == null)
                {
                    return Results.BadRequest(new 
                    { 
                        IsSuccess = false, 
                        Message = "File is required" 
                    });
                }

                if (string.IsNullOrEmpty(fileType))
                {
                    return Results.BadRequest(new 
                    { 
                        IsSuccess = false, 
                        Message = "FileType is required" 
                    });
                }

                var command = new UploadFile.Command(
                    file, 
                    fileType, 
                    string.IsNullOrEmpty(context) ? null : context);
                
                var result = await sender.Send(command);
                return result.ToIResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error handling file upload request");
                return Results.StatusCode(500);
            }
        })
        .WithTags("Files")
        .WithName("UploadFile")
        .WithSummary("Upload a file to blob storage")
        .WithDescription("Uploads a file and returns the URL for use in other endpoints. Supports images, documents, and training materials.")
        .DisableAntiforgery()
        .RequireAuthorization();
    }
}

