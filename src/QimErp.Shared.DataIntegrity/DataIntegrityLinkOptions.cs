namespace QimErp.Shared.DataIntegrity;

public sealed class DataIntegrityLinkOptions
{
    private readonly List<KeyValuePair<string, string>> _maps = [];
    private readonly List<KeyValuePair<string, string>> _excludes = [];
    private readonly List<KeyValuePair<string, string>> _aliases = [];
    private readonly List<string> _noSamples = [];

    internal IReadOnlyList<KeyValuePair<string, string>> Maps => _maps;
    internal IReadOnlyList<KeyValuePair<string, string>> Excludes => _excludes;
    internal IReadOnlyList<KeyValuePair<string, string>> Aliases => _aliases;
    internal IReadOnlyList<string> NoSampleTables => _noSamples;

    /// <summary>tableColumn is "Table.Column" or "schema.Table.Column"; targetEntity is the CLR short name, or the full name when the short name is ambiguous.</summary>
    public DataIntegrityLinkOptions Map(string tableColumn, string targetEntity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableColumn);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetEntity);
        _maps.Add(new(tableColumn.Trim(), targetEntity.Trim()));
        return this;
    }

    /// <summary>Reason convention: "cross-service: &lt;Module&gt;.&lt;Entity&gt;" | "polymorphic" | "external" | "business-key".</summary>
    public DataIntegrityLinkOptions Exclude(string tableColumn, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableColumn);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        _excludes.Add(new(tableColumn.Trim(), reason.Trim()));
        return this;
    }

    /// <summary>Applies to every stem lookup in this context, e.g. Alias("User", "UserIdentity") resolves *UserId columns.</summary>
    public DataIntegrityLinkOptions Alias(string idStem, string targetEntity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idStem);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetEntity);
        _aliases.Add(new(idStem.Trim(), targetEntity.Trim()));
        return this;
    }

    /// <summary>table is "Table" or "schema.Table"; findings rooted at it carry counts but no sample ids. Use when the primary key acts as a secret.</summary>
    public DataIntegrityLinkOptions NoSamples(string table)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);
        _noSamples.Add(table.Trim());
        return this;
    }
}
