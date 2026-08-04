namespace QimErp.Shared.Common.Services.Knowledge;

public interface ITenantKnowledgeAccessService
{
    Task<bool> IsCollectionEnabledAsync(string? tenantId, string collectionKey, CancellationToken cancellationToken = default);
}
