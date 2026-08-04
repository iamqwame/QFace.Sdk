using Pgvector;
using QimErp.Shared.Common.Entities;

namespace QimErp.Shared.Common.Knowledge.Entities;

public sealed class KnowledgeChunk : GuidAuditableEntity
{
    public string CollectionKey { get; private set; } = string.Empty;
    public Guid DocumentId { get; private set; }
    public string DocumentTitle { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public int ChunkIndex { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Vector? Embedding { get; private set; }

    private KnowledgeChunk() { }

    public static KnowledgeChunk Create(
        string tenantId,
        string collectionKey,
        Guid documentId,
        string documentTitle,
        string category,
        int chunkIndex,
        string content,
        Vector? embedding = null)
    {
        var chunk = new KnowledgeChunk
        {
            Id = CreateId(),
            CollectionKey = collectionKey,
            DocumentId = documentId,
            DocumentTitle = documentTitle,
            Category = category,
            ChunkIndex = chunkIndex,
            Content = content,
            Embedding = embedding
        };
        chunk.WithTenantId(tenantId);
        chunk.AsActive();
        return chunk;
    }

    public void SetEmbedding(Vector embedding) => Embedding = embedding;
}
