using QimErp.Shared.Common.Knowledge.Contracts;

namespace QimErp.Shared.Common.Knowledge.Services;

public interface IKnowledgeRagService
{
    Task<AskKnowledgeResponse> AskAsync(
        string tenantId,
        string userId,
        string collectionKey,
        AskKnowledgeRequest request,
        CancellationToken cancellationToken = default);
}
