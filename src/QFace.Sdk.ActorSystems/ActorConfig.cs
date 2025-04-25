namespace QFace.Sdk.ActorSystems;

/// <summary>
/// Configuration for an actor system
/// </summary>
public class ActorConfig
{
    /// <summary>
    /// Gets or sets the name of the actor system
    /// </summary>
    public string SystemName { get; set; } = "DefaultActorSystem";

    /// <summary>
    /// Gets the configuration for actor types
    /// </summary>
    public Dictionary<string, ActorTypeConfig> ActorTypes { get; } = new();

    /// <summary>
    /// Adds or updates configuration for an actor type
    /// </summary>
    /// <typeparam name="T">The actor type</typeparam>
    /// <param name="numberOfInstances">Number of instances for the actor type</param>
    /// <param name="upperBound">Upper bound for the number of instances</param>
    /// <param name="useRouter">Whether to use a router for this actor type</param>
    /// <returns>The actor configuration instance</returns>
    public ActorConfig AddActorType<T>(int numberOfInstances = 1, int upperBound = 10, bool useRouter = false) where T : BaseActor
    {
        var typeName = typeof(T).FullName ?? typeof(T).Name;
        
        ActorTypes[typeName] = new ActorTypeConfig
        {
            TypeName = typeName,
            NumberOfInstances = numberOfInstances,
            UpperBound = upperBound,
            UseRouter = useRouter
        };
        
        return this;
    }
}


/// <summary>
/// Configuration for an actor type
/// </summary>
public class ActorTypeConfig
{
    /// <summary>
    /// Gets or sets the type name of the actor
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the number of instances for this actor type
    /// </summary>
    public int NumberOfInstances { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the upper bound for the number of instances
    /// </summary>
    public int UpperBound { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets whether to use a router for this actor type
    /// </summary>
    public bool UseRouter { get; set; } = false;
}