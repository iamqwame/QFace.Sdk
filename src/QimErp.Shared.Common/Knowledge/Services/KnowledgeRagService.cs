using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QFace.Sdk.AI.Models;
using QFace.Sdk.AI.Services;
using QimErp.Shared.Common.Knowledge.Contracts;
using QimErp.Shared.Common.Knowledge.Repositories;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Knowledge.Services;

public class KnowledgeRagService<TDbContext>(
    KnowledgeChunkRepository<TDbContext> chunkRepository,
    IEmbeddingService embeddingService,
    ILLMService llmService,
    IDistributedCacheService cacheService,
    ILogger<KnowledgeRagService<TDbContext>> logger) : IKnowledgeRagService
    where TDbContext : DbContext
{
    public async Task<AskKnowledgeResponse> AskAsync(
        string tenantId,
        string userId,
        string collectionKey,
        AskKnowledgeRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnforceRateLimitAsync(tenantId, userId, cancellationToken);

        var embeddingResponse = await embeddingService.GenerateEmbeddingAsync(
            new EmbeddingRequest { Text = request.Question },
            cancellationToken);

        var matches = await chunkRepository.SearchByCosineAsync(
            tenantId,
            collectionKey,
            embeddingResponse.Embedding,
            request.TopK,
            request.DocumentId,
            cancellationToken);

        if (matches.Count == 0)
        {
            return new AskKnowledgeResponse(
                "I couldn't find relevant information in the indexed knowledge to answer that question.",
                []);
        }

        var contextBuilder = new StringBuilder();
        var sources = new List<AskKnowledgeSourceResponse>();

        foreach (var match in matches)
        {
            contextBuilder.AppendLine($"[Document: {match.DocumentTitle}]");
            contextBuilder.AppendLine(match.Content);
            contextBuilder.AppendLine();

            sources.Add(new AskKnowledgeSourceResponse(
                match.DocumentId,
                match.DocumentTitle,
                match.Category,
                match.ChunkIndex,
                Truncate(match.Content, 240),
                match.Score));
        }

        var llmResponse = await llmService.GenerateChatCompletionAsync(
            new LLMRequest
            {
                Messages =
                [
                    new LLMMessage
                    {
                        Role = "system",
                        Content =
                            "You are a helpful assistant. Answer questions using only the provided document excerpts. " +
                            "If the excerpts do not contain enough information, say so clearly. Cite document titles when helpful."
                    },
                    new LLMMessage
                    {
                        Role = "user",
                        Content = $"Question: {request.Question}\n\nRelevant excerpts:\n{contextBuilder}"
                    }
                ],
                Temperature = 0.2,
                MaxTokens = 1500
            },
            cancellationToken);

        logger.LogInformation(
            "Generated RAG answer for tenant {TenantId} collection {CollectionKey} using {MatchCount} chunks",
            tenantId,
            collectionKey,
            matches.Count);

        return new AskKnowledgeResponse(llmResponse.Content, sources);
    }

    private async Task EnforceRateLimitAsync(
        string tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        var cacheKey = KnowledgeCache.AskRateLimit(tenantId, userId);
        var count = await cacheService.GetAsync<int?>(cacheKey) ?? 0;
        if (count >= KnowledgeCache.AskRateLimitMaxPerWindow)
        {
            throw new InvalidOperationException("Ask rate limit exceeded. Please try again shortly.");
        }

        await cacheService.SetAsync(
            cacheKey,
            count + 1,
            TimeSpan.FromSeconds(KnowledgeCache.AskRateLimitWindowSeconds));
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }
}
