namespace QimErp.Shared.Common.TenantSetup;

/// <summary>Null or empty selections resolve to base model only via <see cref="BaseModelResolver"/>.</summary>
public static class ModuleGuard
{
    public static bool IsSelected(IReadOnlyList<string>? selectedModules, string moduleKey)
    {
        var resolved = BaseModelResolver.Resolve(selectedModules ?? []);
        return resolved.Contains(moduleKey, StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsSelectedFromCsv(string? selectedModulesCsv, string moduleKey)
    {
        var resolved = BaseModelResolver.ResolveFromCsv(selectedModulesCsv);
        return resolved.Contains(moduleKey, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Entitlement check: a registry prerequisite is not a purchased module and never grants access.</summary>
    public static bool IsExplicitlySelected(IReadOnlyList<string>? selectedModules, string moduleKey)
    {
        var resolved = BaseModelResolver.ResolveExplicit(selectedModules ?? []);
        return resolved.Contains(moduleKey, StringComparer.OrdinalIgnoreCase);
    }
}
