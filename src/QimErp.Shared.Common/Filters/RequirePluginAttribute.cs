namespace QimErp.Shared.Common.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
public sealed class RequirePluginAttribute(string pluginKey) : Attribute
{
    public string PluginKey { get; } = pluginKey;
}
