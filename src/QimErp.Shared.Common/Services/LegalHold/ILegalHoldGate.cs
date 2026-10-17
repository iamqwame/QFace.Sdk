namespace QimErp.Shared.Common.Services.LegalHold;

public interface ILegalHoldGate
{
    /// <summary>True = active hold OR lookup uncertainty — block erasure on true.</summary>
    Task<bool> HasActivePersonLegalHoldAsync(
        string tenantId,
        Guid personId,
        DateTimeOffset? asOfUtc = null,
        CancellationToken cancellationToken = default);
}
