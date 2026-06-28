namespace QimErp.Shared.Common.Activities.TenantActivity;

public sealed record RecordTenantActivityRequest
{
    public required string TenantId { get; init; }
    public required string Module { get; init; }
    public required string ActivityType { get; init; }
    public required string Summary { get; init; }
    public required Guid ActorUserId { get; init; }
    public required string ActorUserName { get; init; }
    public string? SubjectType { get; init; }
    public Guid? SubjectId { get; init; }
    public string? SubjectLabel { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public string? MetadataJson { get; init; }
}
