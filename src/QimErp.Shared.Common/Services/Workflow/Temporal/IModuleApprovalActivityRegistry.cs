namespace QimErp.Shared.Common.Services.Workflow.Temporal;

/// <summary>
/// Tracks which IModuleApprovalActivity implementation handles which entity types.
/// Each module's Consumer registers itself on startup via AddModuleApprovalActivity&lt;T&gt;().
/// The Platform Worker resolves the correct implementation at runtime.
/// </summary>
public interface IModuleApprovalActivityRegistry
{
    void Register(string entityType, Type activityType);
    Type? Resolve(string entityType);
    IReadOnlyDictionary<string, Type> All();
}

public sealed class ModuleApprovalActivityRegistry : IModuleApprovalActivityRegistry
{
    private readonly Dictionary<string, Type> _map =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(string entityType, Type activityType)
        => _map[entityType] = activityType;

    public Type? Resolve(string entityType)
        => _map.TryGetValue(entityType, out var t) ? t : null;

    public IReadOnlyDictionary<string, Type> All()
        => _map;
}

/// <summary>
/// Marker used to defer registration from module Consumer Program.cs
/// into the singleton registry at startup.
/// </summary>
public sealed class ModuleApprovalActivityRegistration
{
    public IReadOnlyList<string> EntityTypes { get; }
    public Type ActivityType { get; }

    public ModuleApprovalActivityRegistration(
        IEnumerable<string> entityTypes,
        Type activityType)
    {
        EntityTypes   = entityTypes.ToList().AsReadOnly();
        ActivityType  = activityType;
    }
}
