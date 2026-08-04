using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QimErp.Shared.Common.Knowledge.Entities;
using QimErp.Shared.Common.Repositories;

namespace QimErp.Shared.Common.Knowledge.Repositories;

public class KnowledgeChunkRepository<TDbContext>(
    TDbContext context,
    ILogger<KnowledgeChunkRepository<TDbContext>> logger)
    : Repository<KnowledgeChunk, Guid, TDbContext>(context, logger)
    where TDbContext : DbContext
{
    protected override DbSet<KnowledgeChunk> DbSet => Context.Set<KnowledgeChunk>();
    protected override Expression<Func<KnowledgeChunk, Guid>> KeySelector => e => e.Id;

    public async Task DeleteByDocumentAsync(
        string tenantId,
        string collectionKey,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        await DbSet
            .Where(c => c.TenantId == tenantId
                && c.CollectionKey == collectionKey
                && c.DocumentId == documentId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<KnowledgeChunkMatch>> SearchByCosineAsync(
        string tenantId,
        string collectionKey,
        float[] queryEmbedding,
        int topK,
        Guid? documentId = null,
        CancellationToken cancellationToken = default)
    {
        var vectorLiteral = ToVectorLiteral(queryEmbedding);

        var sql = documentId.HasValue
            ? "SELECT c.\"DocumentId\", c.\"DocumentTitle\", c.\"Category\", c.\"ChunkIndex\", c.\"Content\", " +
              "1 - (c.\"Embedding\" <=> " + vectorLiteral + "::vector) AS \"Score\" " +
              "FROM \"KnowledgeChunks\" c " +
              "WHERE c.\"TenantId\" = {0} AND c.\"CollectionKey\" = {1} AND c.\"DataStatus\" = 'Active' AND c.\"DocumentId\" = {3} " +
              "ORDER BY c.\"Embedding\" <=> " + vectorLiteral + "::vector LIMIT {2}"
            : "SELECT c.\"DocumentId\", c.\"DocumentTitle\", c.\"Category\", c.\"ChunkIndex\", c.\"Content\", " +
              "1 - (c.\"Embedding\" <=> " + vectorLiteral + "::vector) AS \"Score\" " +
              "FROM \"KnowledgeChunks\" c " +
              "WHERE c.\"TenantId\" = {0} AND c.\"CollectionKey\" = {1} AND c.\"DataStatus\" = 'Active' " +
              "ORDER BY c.\"Embedding\" <=> " + vectorLiteral + "::vector LIMIT {2}";

        return documentId.HasValue
            ? await Context.Database
                .SqlQueryRaw<KnowledgeChunkMatch>(
                    sql,
                    tenantId,
                    collectionKey,
                    topK,
                    documentId.Value)
                .ToListAsync(cancellationToken)
            : await Context.Database
                .SqlQueryRaw<KnowledgeChunkMatch>(
                    sql,
                    tenantId,
                    collectionKey,
                    topK)
                .ToListAsync(cancellationToken);
    }

    private static string ToVectorLiteral(float[] values)
    {
        var parts = string.Join(",", values.Select(v => v.ToString(CultureInfo.InvariantCulture)));
        return $"'[{parts}]'";
    }
}

public sealed class KnowledgeChunkMatch
{
    public Guid DocumentId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
}
