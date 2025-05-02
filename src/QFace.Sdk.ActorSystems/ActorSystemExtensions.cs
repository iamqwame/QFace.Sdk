namespace QFace.Sdk.ActorSystems;

/// <summary>
/// Extension methods for setting up the actor system
/// </summary>
public static class ActorSystemExtensions
{
    /// <summary>
    /// Adds an actor system to the service collection with required assemblies for actor discovery
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies to scan for actors (required)</param>
    /// <param name="configure">Optional configuration action</param>
    /// <param name="addLifecycle">Whether to add the actor system hosted service for lifecycle management</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddActorSystem(
        this IServiceCollection services,
        Assembly[] assemblies,
        Action<ActorConfig> configure = null,
        bool addLifecycle = false)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided for actor discovery", nameof(assemblies));
        }
        
        // Register actor service
        services.AddScoped<IActorService, ActorService>();
        
        // Configure actor system
        var actorConfig = new ActorConfig();
        configure?.Invoke(actorConfig);
        
        // Auto-discover actors from the specified assemblies
        DiscoverActors(assemblies, actorConfig);
        
        // Get system name from assembly or use configured name
        var actorSystemName = !string.IsNullOrEmpty(actorConfig.SystemName) 
            ? actorConfig.SystemName 
            : GetDefaultSystemName();
        
        // Register actor system
        services.AddSingleton(sp =>
        {
            // Set up actor system with dependency resolver
            var actorSystemSetup = BootstrapSetup
                .Create()
                .And(DependencyResolverSetup
                    .Create(sp));

            var actorSystem = Akka.Actor.ActorSystem
                .Create(actorSystemName, actorSystemSetup);
            
            // Register actors according to configuration
            foreach (var actorType in actorConfig.ActorTypes)
            {
                var type = Type.GetType(actorType.Key) ?? 
                           AppDomain.CurrentDomain.GetAssemblies()
                              .SelectMany(a => a.GetTypes())
                              .FirstOrDefault(t => t.FullName == actorType.Key || t.Name == actorType.Key);
                
                if (type != null && typeof(BaseActor).IsAssignableFrom(type))
                {
                    var config = actorType.Value;
                    
                    // Use generic method to register actor
                    var registerMethod = config.UseRouter
                        ? typeof(TopLevelActors).GetMethod(nameof(TopLevelActors.RegisterActorWithRouter))
                        : typeof(TopLevelActors).GetMethod(nameof(TopLevelActors.RegisterActor));
                    
                    if (registerMethod != null)
                    {
                        var genericMethod = registerMethod.MakeGenericMethod(type);
                        
                        if (config.UseRouter)
                        {
                            genericMethod.Invoke(null, new object[] { actorSystem, config.NumberOfInstances, config.UpperBound, "" });
                        }
                        else
                        {
                            genericMethod.Invoke(null, new object[] { actorSystem, "" });
                        }
                    }
                }
            }
            
            return actorSystem;
        });
        
        // Register actor system hosted service if requested
        if (addLifecycle)
        {
            services.AddHostedService<ActorSystemHostedService>();
        }

        return services;
        
        // Helper to get system name from assembly
        static string GetDefaultSystemName()
        {
            return Regex.Replace(Assembly.GetEntryAssembly()?.GetName().Name ?? "ActorSystem",
                @"[^a-zA-Z\s]+", "");
        }
    }
    
    /// <summary>
    /// Adds an actor system with lifecycle management
    /// </summary>
    public static IServiceCollection AddActorSystemWithLifecycle(
        this IServiceCollection services,
        Assembly[] assemblies,
        Action<ActorConfig> configure = null)
    {
        return services.AddActorSystem(assemblies, configure, true);
    }
    
    /// <summary>
    /// Initializes the actor system in the application
    /// </summary>
    /// <param name="app">The host application</param>
    /// <returns>The host application</returns>
    public static IHost UseActorSystem(this IHost app)
    {
        var actorSystem = app.Services.GetRequiredService<ActorSystem>();
        
        if (actorSystem == null)
        {
            throw new InvalidOperationException("Actor system not registered. Make sure to call AddActorSystem first.");
        }
        
        return app;
    }
    
    /// <summary>
    /// Discovers actor types from assemblies and adds them to the configuration
    /// </summary>
    private static void DiscoverActors(Assembly[] assemblies, ActorConfig actorConfig)
    {
        foreach (var assembly in assemblies)
        {
            var actorTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(BaseActor).IsAssignableFrom(t));
                
            foreach (var actorType in actorTypes)
            {
                // Skip if already registered
                if (actorConfig.ActorTypes.ContainsKey(actorType.FullName ?? actorType.Name))
                {
                    continue;
                }
                
                // Add to configuration
                actorConfig.ActorTypes[actorType.FullName ?? actorType.Name] = new ActorTypeConfig
                {
                    TypeName = actorType.FullName ?? actorType.Name,
                    NumberOfInstances = 1,
                    UpperBound = 10,
                    UseRouter = false
                };
            }
        }
    }
}