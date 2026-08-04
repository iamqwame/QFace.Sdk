using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;
using QFace.Sdk.BlobStorage.Services;
using QimErp.Shared.Common.Knowledge.Contracts;
using QimErp.Shared.Common.Knowledge.Entities;
using QimErp.Shared.Common.Knowledge.Repositories;
using UglyToad.PdfPig;

namespace QimErp.Shared.Common.Knowledge.Services;

public class KnowledgeIngestionService<TDbContext>(
    KnowledgeChunkRepository<TDbContext> chunkRepository,
    IFileUploadService fileUploadService,
    IEmbeddingService embeddingService,
    ILogger<KnowledgeIngestionService<TDbContext>> logger) : IKnowledgeIngestionService
    where TDbContext : DbContext
{
    private const int ChunkSize = 1200;

    public async Task<IndexKnowledgeDocumentResponse> IndexDocumentAsync(
        string tenantId,
        string collectionKey,
        IndexKnowledgeDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        await chunkRepository.DeleteByDocumentAsync(tenantId, collectionKey, request.DocumentId, cancellationToken);

        var text = await ResolveTextAsync(request, cancellationToken);
        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogInformation(
                "Skipped indexing document {DocumentId} — no extractable text",
                request.DocumentId);
            return new IndexKnowledgeDocumentResponse(request.DocumentId, 0, "Skipped");
        }

        var chunks = SplitIntoChunks(text, ChunkSize);
        for (var i = 0; i < chunks.Count; i++)
        {
            var chunkText = chunks[i];
            var embeddingResponse = await embeddingService.GenerateEmbeddingAsync(
                new EmbeddingRequest { Text = chunkText },
                cancellationToken);

            var chunk = KnowledgeChunk.Create(
                tenantId,
                collectionKey,
                request.DocumentId,
                request.DocumentTitle,
                request.Category,
                i,
                chunkText,
                new Vector(embeddingResponse.Embedding));

            await chunkRepository.AddAsync(chunk, cancellationToken);
        }

        await chunkRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Indexed document {DocumentId} into collection {CollectionKey} with {ChunkCount} chunks",
            request.DocumentId,
            collectionKey,
            chunks.Count);

        return new IndexKnowledgeDocumentResponse(request.DocumentId, chunks.Count, "Ready");
    }

    public Task DeleteDocumentAsync(
        string tenantId,
        string collectionKey,
        Guid documentId,
        CancellationToken cancellationToken = default) =>
        chunkRepository.DeleteByDocumentAsync(tenantId, collectionKey, documentId, cancellationToken);

    private async Task<string?> ResolveTextAsync(
        IndexKnowledgeDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.TextContent))
        {
            return request.TextContent;
        }

        if (string.IsNullOrWhiteSpace(request.StorageKey))
        {
            return null;
        }

        var bytes = await fileUploadService.GetObjectBytesAsync(request.StorageKey, cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        return ExtractText(bytes, request.ContentType, request.FileName);
    }

    private static string? ExtractText(byte[] fileBytes, string? contentType, string? fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        var normalizedContentType = contentType?.ToLowerInvariant() ?? string.Empty;

        if (extension == ".pdf" || normalizedContentType.Contains("pdf"))
        {
            using var stream = new MemoryStream(fileBytes);
            using var document = PdfDocument.Open(stream);
            return string.Join("\n", document.GetPages().Select(p => p.Text)).Trim();
        }

        if (extension is ".docx" or ".doc"
            || normalizedContentType.Contains("wordprocessingml")
            || normalizedContentType.Contains("msword"))
        {
            using var stream = new MemoryStream(fileBytes);
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body is null)
            {
                return null;
            }

            return string.Join("\n", body.Descendants<Text>().Select(t => t.Text)).Trim();
        }

        if (extension == ".txt" || normalizedContentType.StartsWith("text/plain"))
        {
            return Encoding.UTF8.GetString(fileBytes);
        }

        return null;
    }

    private static List<string> SplitIntoChunks(string text, int chunkSize)
    {
        var chunks = new List<string>();
        for (var i = 0; i < text.Length; i += chunkSize)
        {
            var length = Math.Min(chunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }
}
