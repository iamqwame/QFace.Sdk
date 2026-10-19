namespace QimErp.Shared.DataIntegrity;

public sealed record IntegrityRunInfo(string RunId, DateTimeOffset RunAt, string ModuleKey, string ContextKey);
