namespace QimErp.Shared.Common.Outbox;

/// <summary>Delivers a claimed outbox envelope through one transport (DATA-09).</summary>
public interface IEventTransport
{
    string Name { get; }

    Task SendAsync(EventEnvelope envelope, CancellationToken cancellationToken = default);
}
