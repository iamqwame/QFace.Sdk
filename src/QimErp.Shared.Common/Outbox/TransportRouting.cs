using Microsoft.Extensions.Options;

namespace QimErp.Shared.Common.Outbox;

internal sealed class TransportRouting(IOptions<OutboxOptions> options) : ITransportRouting
{
    public IReadOnlyList<string> TransportsFor(string eventType)
    {
        var opts = options.Value;
        if (!string.IsNullOrWhiteSpace(eventType)
            && opts.Routing.TryGetValue(eventType, out var configured)
            && configured is { Length: > 0 })
        {
            return configured;
        }

        // Workflow approval hand-off is Temporal by default (DATA-09 level S).
        if (string.Equals(eventType, "WorkflowApprovalRequired", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "WorkflowEventMessage", StringComparison.OrdinalIgnoreCase))
        {
            return [OutboxTransports.Temporal];
        }

        return opts.DefaultTransports is { Length: > 0 }
            ? opts.DefaultTransports
            : [OutboxTransports.InProcess];
    }
}
