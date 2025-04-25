using Akka.Actor;

namespace QFace.Sdk.ActorSystems.QFace.ActorSystem.Sdk;

/// <summary>
/// Default implementation of the actor service
/// </summary>
public class ActorService : IActorService
{
    /// <summary>
    /// Sends a message to an actor of the specified type
    /// </summary>
    /// <typeparam name="T">The actor type</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="name">Optional name identifier for the actor</param>
    public void Tell<T>(object message, string name = "") where T : BaseActor
    {
        TopLevelActors.GetActor<T>(name)
            .Tell(message);
    }
}