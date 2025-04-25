namespace QFace.Sdk.ActorSystems;

/// <summary>
/// Service for interacting with the actor system
/// </summary>
public interface IActorService
{
    /// <summary>
    /// Sends a message to an actor of the specified type
    /// </summary>
    /// <typeparam name="T">The actor type</typeparam>
    /// <param name="message">The message to send</param>
    /// <param name="name">Optional name identifier for the actor</param>
    void Tell<T>(object message, string name = "") where T : BaseActor;
}