namespace QimErp.Shared.Common.Entities;

/// <summary>
/// Short-lived hold on a human-readable code so concurrent create UIs do not collide.
/// Shared by EntityCode (formatted codes) and ChartOfAccount (numeric codes via entityType).
/// </summary>
public sealed class EntityCodeReservation : GuidAuditableEntity
{
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan MaxTtl = TimeSpan.FromSeconds(600);
    public const int MaxActiveReservationsPerScope = 50;

    public string EntityType { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string ReservationToken { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public string? Metadata { get; private set; }

    private EntityCodeReservation() { }

    public static EntityCodeReservation Create(
        string tenantId,
        string entityType,
        string code,
        string reservationToken,
        TimeSpan ttl,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be empty.", nameof(entityType));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(reservationToken))
            throw new ArgumentException("ReservationToken cannot be empty.", nameof(reservationToken));
        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), "TTL must be positive.");

        var reservation = new EntityCodeReservation
        {
            Id = CreateId(),
            TenantId = tenantId,
            EntityType = entityType.Trim(),
            Code = code.Trim(),
            ReservationToken = reservationToken.Trim(),
            ExpiresAt = DateTimeOffset.UtcNow.Add(ttl),
            Metadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim(),
        };
        reservation.AsActive();
        return reservation;
    }

    public bool IsActive => DataStatus == DataState.Active && ExpiresAt > DateTimeOffset.UtcNow;

    public void RefreshTtl(TimeSpan ttl, string? metadata = null)
    {
        if (ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), "TTL must be positive.");
        ExpiresAt = DateTimeOffset.UtcNow.Add(ttl);
        if (metadata is not null)
            Metadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim();
    }
}
