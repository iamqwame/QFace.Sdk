namespace QFace.Sdk.ActorSystems;

/// <summary>
/// Base actor class for all actors in the system
/// </summary>
public abstract class BaseActor : ReceiveActor
{
    /// <summary>
    /// Publishes an event to the event stream
    /// </summary>
    protected void Publish(object @event)
    {
        Context.Dispatcher.EventStream.Publish(@event);
    }

    /// <summary>
    /// Sets up receiving for a message type and forwards it to a new child actor of the specified type
    /// </summary>
    protected void ReceiveAsync<TMessage, TActor>(bool poisonPill = false, string? name = null)
        where TActor : BaseActor
        => ReceiveAsync<TMessage>(async message => await CreateActorAsync<TActor, TMessage>(message, poisonPill, name));

    /// <summary>
    /// Creates a child actor and forwards a message to it
    /// </summary>
    private async Task CreateActorAsync<TActor, TMessage>(TMessage message, bool poisonPill = false,
        string? name = null)
        where TActor : BaseActor
    {
        var actorRef = Context.ActorOf(
            DependencyResolver
                .For(Context.System)
                .Props<TActor>()
                .WithSupervisorStrategy(GetDefaultSupervisorStrategy),
            GetActorName(name));

        actorRef.Forward(message);

        if (poisonPill)
        {
            actorRef.Tell(PoisonPill.Instance);
        }

        await Task.CompletedTask;

        static string GetActorName(string? name)
        {
            return
                $"actor-{typeof(TActor).Name.ToLower()}{(string.IsNullOrEmpty(name) ? "" : "-" + name.ToLower())}-{Guid.NewGuid():N}";
        }
    }

    /// <summary>
    /// Default supervisor strategy for actors
    /// </summary>
    private static SupervisorStrategy GetDefaultSupervisorStrategy => new OneForOneStrategy(
        3, TimeSpan.FromSeconds(3), ex =>
        {
            if (!(ex is ActorInitializationException))
                return Directive.Resume;

            Context.System.Terminate().Wait(1000);

            return Directive.Stop;
        });
}