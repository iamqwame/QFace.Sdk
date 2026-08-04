using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QimErp.Shared.Common.Database.Configurations;
using QimErp.Shared.Common.Knowledge.Entities;

namespace QimErp.Shared.Common.Knowledge.Database;

public class KnowledgeChunkConfiguration : AuditableEntityConfiguration<KnowledgeChunk>
{
    public override void Configure(EntityTypeBuilder<KnowledgeChunk> builder)
    {
        base.Configure(builder);

        builder.ToTable("KnowledgeChunks");

        builder.Property(x => x.CollectionKey).IsRequired();
        builder.Property(x => x.DocumentId).IsRequired();
        builder.Property(x => x.DocumentTitle).IsRequired();
        builder.Property(x => x.Category).IsRequired();
        builder.Property(x => x.ChunkIndex).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Embedding)
            .HasColumnType("vector(768)");

        builder.HasIndex(x => new { x.TenantId, x.CollectionKey, x.DocumentId });
        builder.HasIndex(x => new { x.TenantId, x.CollectionKey, x.DocumentId, x.ChunkIndex })
            .IsUnique()
            .HasDatabaseName("IX_KnowledgeChunks_TenantId_CollectionKey_DocumentId_ChunkIndex");

        builder.HasIndex(x => x.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
