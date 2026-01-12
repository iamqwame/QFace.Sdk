namespace QimErp.Shared.Common.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string? EventType { get; set; }
    public string? Payload { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public bool IsProcessed { get; set; } = false;

    public static OutboxMessage Create(string fullName, string payload)
    {
        return new OutboxMessage
        {
            EventType = fullName,
            Payload = payload,
            OccurredAt = DateTime.UtcNow,
            IsProcessed = false
        };
    }
}