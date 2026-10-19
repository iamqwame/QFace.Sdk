using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace QimErp.Shared.DataIntegrity.Discovery;

internal sealed class IntegrityLinkCatalog(DataIntegrityContextSet contextSet)
{
    private readonly ConcurrentDictionary<Type, IntegrityDiscoveryResult> _results = new();

    public IntegrityDiscoveryResult Get(DataIntegrityContextRegistration registration, DbContext context)
    {
        if (!contextSet.Contexts.Contains(registration))
            throw new DataIntegrityConfigurationException([$"{registration.ContextType.Name} is not registered for data integrity checks."]);

        if (!context.Database.IsRelational())
            throw new DataIntegrityConfigurationException([$"{registration.ContextType.Name} uses provider {context.Database.ProviderName}, which is not relational."]);

        return _results.GetOrAdd(
            registration.ContextType,
            _ => IntegrityLinkDiscovery.Discover(context.Model, registration, CheckPrefix(registration)));
    }

    private string CheckPrefix(DataIntegrityContextRegistration registration)
        => contextSet.Contexts.Count > 1
            ? $"{contextSet.ModuleKey}.{IntegrityNaming.ContextKey(registration.ContextType)}"
            : contextSet.ModuleKey;
}
