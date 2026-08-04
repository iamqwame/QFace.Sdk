using QimErp.Shared.Common.Services.TenantSetup;
using QimErp.Shared.Common.TenantSetup;

namespace QimErp.Shared.Common.Services.Knowledge;

/// <summary>
/// Gates tenant knowledge features on the QimAI module install, reusing the same
/// TenantModuleSnapshot cache IAM already maintains — no separate knowledge-config table.
/// </summary>
public sealed class TenantKnowledgeAccessService(ITenantModuleAccessService moduleAccess) : ITenantKnowledgeAccessService
{
    public Task<bool> IsCollectionEnabledAsync(string? tenantId, string collectionKey, CancellationToken cancellationToken = default) =>
        moduleAccess.IsModuleEnabledAsync(tenantId, ModuleKeys.QimAI, cancellationToken);
}
