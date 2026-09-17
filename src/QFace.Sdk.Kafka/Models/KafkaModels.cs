namespace QFace.Sdk.Kafka.Models;

/// <summary>
/// Provides access to top-level actors from the ActorSystems SDK
/// </summary>
public interface ITopLevelActors
{
    IActorRef GetActor<T>(string name = "") where T : BaseActor;
}

/// <summary>
/// Implementation of ITopLevelActors that wraps the static TopLevelActors class
/// </summary>
public class TopLevelActorsWrapper : ITopLevelActors
{
    public IActorRef GetActor<T>(string name = "") where T : BaseActor
    {
        return TopLevelActors.GetActor<T>(name);
    }
}
