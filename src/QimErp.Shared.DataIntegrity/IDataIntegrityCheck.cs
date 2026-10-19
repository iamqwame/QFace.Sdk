namespace QimErp.Shared.DataIntegrity;

public interface IDataIntegrityCheck
{
    string Name { get; }

    Task<IReadOnlyList<IntegrityFinding>> RunAsync(CancellationToken cancellationToken);
}
