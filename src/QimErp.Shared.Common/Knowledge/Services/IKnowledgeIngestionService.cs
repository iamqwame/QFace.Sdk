using QimErp.Shared.Common.Knowledge.Contracts;

namespace QimErp.Shared.Common.Knowledge.Services;

public interface IKnowledgeIngestionService
{
    Task<IndexKnowledgeDocumentResponse> IndexDocumentAsync(
        string tenantId,
        string collectionKey,
        IndexKnowledgeDocumentRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(
        string tenantId,
        string collectionKey,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
