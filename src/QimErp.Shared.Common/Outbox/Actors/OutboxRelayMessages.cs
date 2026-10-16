using Akka.Routing;

namespace QimErp.Shared.Common.Outbox.Actors;

internal sealed class OutboxPollTick
{
    public static readonly OutboxPollTick Instance = new();
}

internal sealed record OutboxDispatch(OutboxMessage Message) : IConsistentHashable
{
    public object ConsistentHashKey => Message.EntityId;
}
