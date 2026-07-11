namespace QimErp.Shared.Common.TenantSetup;

public static class PluginAccessGuard
{
    public static bool IsInstalled(IReadOnlyList<string>? installedPlugins, string pluginKey) =>
        installedPlugins?.Contains(pluginKey, StringComparer.OrdinalIgnoreCase) ?? false;
}
