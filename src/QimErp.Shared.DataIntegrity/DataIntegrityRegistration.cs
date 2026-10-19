namespace QimErp.Shared.DataIntegrity;

public sealed record DataIntegrityRegistration(string ModuleKey, IReadOnlyList<Type> Contexts);

internal sealed record DataIntegrityContextSet(
    string ModuleKey,
    IReadOnlyList<DataIntegrityContextRegistration> Contexts,
    IReadOnlyList<Type> Checks);
