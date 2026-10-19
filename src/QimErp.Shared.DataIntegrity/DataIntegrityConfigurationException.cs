namespace QimErp.Shared.DataIntegrity;

public sealed class DataIntegrityConfigurationException : Exception
{
    public DataIntegrityConfigurationException(IReadOnlyList<string> problems)
        : base("Data integrity configuration is invalid:" + Environment.NewLine + string.Join(Environment.NewLine, problems.Select(p => "- " + p)))
    {
        Problems = problems;
    }

    public IReadOnlyList<string> Problems { get; }
}
