using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QimErp.Shared.Common.Knowledge.Repositories;
using QimErp.Shared.Common.Knowledge.Services;

namespace QimErp.Shared.Common.Knowledge.Extensions;

public static class KnowledgeServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Knowledge/RAG stack (repository, ingestion, RAG service) against the
    /// host's own DbContext — TDbContext must already have a KnowledgeChunks DbSet and
    /// apply KnowledgeChunkConfiguration in OnModelCreating.
    /// </summary>
    public static IServiceCollection AddKnowledgeServices<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<KnowledgeChunkRepository<TDbContext>>();
        services.AddScoped<IKnowledgeIngestionService, KnowledgeIngestionService<TDbContext>>();
        services.AddScoped<IKnowledgeRagService, KnowledgeRagService<TDbContext>>();
        return services;
    }
}
