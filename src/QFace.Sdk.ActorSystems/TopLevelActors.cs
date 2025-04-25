using System.Collections.Concurrent;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Routing;

namespace QFace.Sdk.ActorSystems;

/// <summary>
/// Registry for top-level actors in the system
/// </summary>
public static class TopLevelActors
{
    private static readonly ConcurrentDictionary<string, IActorRef> _actorRegistry = new();
    private static SupervisorStrategy _defaultSupervisorStrategy = null!;

    /// <summary>
    /// Gets a registered actor of the specified type
    /// </summary>
    /// <typeparam name="T">The actor type</typeparam>
    /// <param name="name">Optional name identifier</param>
    /// <returns>The actor reference</returns>
    public static IActorRef GetActor<T>(string name = "") where T : BaseActor
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));

        var actorFullName = GetActorFullName<T>(name);

        if (_actorRegistry.TryGetValue(actorFullName, out var actorInstance))
        {
            return actorInstance;
        }

        throw new ArgumentOutOfRangeException(nameof(actorFullName),
            $"\"{actorFullName}\" not created or registered");
    }

    /// <summary>
    /// Registers a top-level actor in the system
    /// </summary>
    /// <typeparam name="T">The actor type</typeparam>
    /// <param name="actorSystem">The actor system</param>
    /// <param name="name">Optional name identifier</param>
    /// <returns>True if registration succeeded, false otherwise</returns>
    public static bool RegisterActor<T>(Akka.Actor.ActorSystem actorSystem, string name = "") where T : BaseActor
    {
        var actorFullName = GetActorFullName<T>(name);

        var actor = CreateNewActor<T>(actorSystem, actorFullName);

        return _actorRegistry.TryAdd(actorFullName, actor);
    }

    /// <summary>
    /// Registers a top-level actor with a router for load balancing
    /// </summary>
    /// <typeparam name="T">The actor type</typeparam>
    /// <param name="actorSystem">The actor system</param>
    /// <param name="numberOfInstance">Initial number of instances</param>
    /// <param name="upperBound">Maximum number of instances</param>
    /// <param name="name">Optional name identifier</param>
    /// <returns>True if registration succeeded, false otherwise</returns>
    public static bool RegisterActorWithRouter<T>(Akka.Actor.ActorSystem actorSystem, int numberOfInstance, int upperBound,
        string name = "") where T : BaseActor
    {
        if (numberOfInstance >= upperBound)
            throw new ArgumentOutOfRangeException(nameof(numberOfInstance),
                "numberOfInstance should be < upperBound");

        var actorFullName = GetActorFullName<T>(name);

        var actor = CreateNewActorWithRouter<T>(actorSystem, numberOfInstance, upperBound, actorFullName);

        return _actorRegistry.TryAdd(actorFullName, actor);
    }

    /// <summary>
    /// Creates a new actor of the specified type
    /// </summary>
    private static IActorRef CreateNewActor<T>(Akka.Actor.ActorSystem actorSystem, string name) where T : BaseActor
    {
        return actorSystem.ActorOf(
            DependencyResolver
                .For(actorSystem)
                .Props<T>()
                .WithSupervisorStrategy(actorSystem.DefaultSupervisorStrategy()), name);
    }

    /// <summary>
    /// Creates a new actor with a router for load balancing
    /// </summary>
    private static IActorRef CreateNewActorWithRouter<T>(Akka.Actor.ActorSystem actorSystem, int numberOfInstance, int upperBound,
        string name) where T : BaseActor
    {
        return actorSystem.ActorOf(
            DependencyResolver
                .For(actorSystem)
                .Props<T>()
                .WithSupervisorStrategy(actorSystem.DefaultSupervisorStrategy())
                .WithRouter(new RoundRobinPool(numberOfInstance, new DefaultResizer(numberOfInstance, upperBound))),
            name);
    }

    /// <summary>
    /// Gets the default supervisor strategy for the actor system
    /// </summary>
    public static SupervisorStrategy DefaultSupervisorStrategy(this Akka.Actor.ActorSystem actorSystem)
    {
        return _defaultSupervisorStrategy ??= new OneForOneStrategy(
            3, TimeSpan.FromSeconds(3), ex =>
            {
                if (ex is not ActorInitializationException)
                    return Directive.Resume;

                actorSystem?.Terminate().Wait(1000);

                return Directive.Stop;
            });
    }

    /// <summary>
    /// Gets the full name for an actor
    /// </summary>
    private static string GetActorFullName<T>(string name) where T : BaseActor
    {
        return $"{name.Trim()}_{typeof(T).Name}";
    }
}