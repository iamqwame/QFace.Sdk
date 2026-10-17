using QimErp.Shared.Common.Events;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Ids-only payload for <see cref="IPersonErasureWorkflow"/>.
/// No AuthorizationHeader, email, or staff code.
/// </summary>
public sealed class PersonErasureRequest
{
    public string TenantId { get; init; } = string.Empty;
    public Guid PersonId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;

    public static PersonErasureRequest Create(
        Guid personId,
        string tenantId,
        string? correlationId = null)
    {
        return new PersonErasureRequest
        {
            PersonId = personId,
            TenantId = tenantId ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty
        };
    }

    public static PersonErasureRequest From(PersonErasedEvent erased)
    {
        ArgumentNullException.ThrowIfNull(erased);
        return Create(erased.PersonId, erased.TenantId, erased.CorrelationId);
    }
}
