using Microsoft.EntityFrameworkCore;

namespace QimErp.Shared.DataIntegrity;

public sealed class DataIntegrityBuilder
{
    private readonly List<DataIntegrityContextRegistration> _contexts = [];
    private readonly List<Type> _checks = [];

    internal DataIntegrityBuilder()
    {
    }

    internal IReadOnlyList<DataIntegrityContextRegistration> Contexts => _contexts;
    internal IReadOnlyList<Type> Checks => _checks;

    public DataIntegrityBuilder AddContext<TContext>(Action<DataIntegrityLinkOptions>? links = null) where TContext : DbContext
    {
        if (_contexts.Any(c => c.ContextType == typeof(TContext)))
            throw new InvalidOperationException($"{typeof(TContext).Name} is already registered for data integrity checks.");

        var options = new DataIntegrityLinkOptions();
        links?.Invoke(options);
        _contexts.Add(new DataIntegrityContextRegistration(typeof(TContext), options));
        return this;
    }

    public DataIntegrityBuilder AddCheck<TCheck>() where TCheck : class, IDataIntegrityCheck
    {
        if (_checks.Contains(typeof(TCheck)))
            throw new InvalidOperationException($"{typeof(TCheck).Name} is already registered as a data integrity check.");

        _checks.Add(typeof(TCheck));
        return this;
    }
}

internal sealed record DataIntegrityContextRegistration(Type ContextType, DataIntegrityLinkOptions Links);
