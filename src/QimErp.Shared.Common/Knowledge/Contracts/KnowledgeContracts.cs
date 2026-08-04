namespace QimErp.Shared.Common.Knowledge.Contracts;

public record IndexKnowledgeDocumentRequest(
    Guid DocumentId,
    string DocumentTitle,
    string Category,
    string? TextContent = null,
    string? StorageKey = null,
    string? ContentType = null,
    string? FileName = null);

public record IndexKnowledgeDocumentResponse(
    Guid DocumentId,
    int ChunkCount,
    string Status);

public record AskKnowledgeRequest(
    string Question,
    Guid? DocumentId = null,
    int TopK = 5);

public record AskKnowledgeResponse(
    string Answer,
    IReadOnlyList<AskKnowledgeSourceResponse> Sources);

public record AskKnowledgeSourceResponse(
    Guid DocumentId,
    string DocumentTitle,
    string Category,
    int ChunkIndex,
    string Excerpt,
    double Score);
