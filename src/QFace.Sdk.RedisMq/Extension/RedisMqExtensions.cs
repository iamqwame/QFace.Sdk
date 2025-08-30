namespace QFace.Sdk.RedisMq.Extension;

public static class RedisMqExtensions
{
    /// <summary>
    /// Adds Redis pub/sub functionality to the service collection with support for both producers and consumers
    /// </summary>
    public static IServiceCollection AddRedisMq(this IServiceCollection services,
        IConfiguration configuration, Assembly[] consumerAssemblies,
        Action<ActorConfig>? configureActorSystem = null)
    {
        // Use a default configuration if none is provided
        configureActorSystem ??= config =>
        {
            var guid = Guid.NewGuid().ToString("N");
            config.SystemName = $"RedisMessageConsumerSystem{guid}";
        };

        // Get options from configuration
        services.Configure<RedisMqOptions>(configuration.GetSection("RedisMq"));

        // Register connection provider as singleton
        services.AddSingleton<RedisMqConnectionProvider>();
        
        services.AddSingleton<IConnectionMultiplexer>(sp => 
            sp.GetRequiredService<RedisMqConnectionProvider>().Connection);
        services.AddSingleton<IDatabase>(sp => 
            sp.GetRequiredService<RedisMqConnectionProvider>().Database);
        services.AddSingleton<ISubscriber>(sp => 
            sp.GetRequiredService<RedisMqConnectionProvider>().Subscriber);

        // Register the actor system if it's not already registered
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            // Include the Redis SDK assembly
            var sdkAssembly = typeof(RedisMqPublisherActor).Assembly;
            var allAssemblies = consumerAssemblies.Concat(new[] { sdkAssembly }).Distinct().ToArray();

            // Register actor system with these assemblies
            services.AddActorSystemWithLifecycle(allAssemblies, config =>
            {
                // Configure actor system with default settings
                config.SystemName = "RedisMQActorSystem";

                // Apply custom configuration if provided
                configureActorSystem?.Invoke(config);
            });
        }

        // Register publisher service
        services.AddScoped<IRedisMqPublisher, RedisMqPublisher>();

        // Register Redis consumers if assemblies are provided
        if (consumerAssemblies.Length > 0)
        {
            services.AddRedisMqConsumers(consumerAssemblies);
        }

        return services;
    }

    /// <summary>
    /// Adds only the Redis producer functionality to the service collection
    /// </summary>
    public static IServiceCollection AddRedisMqProducer(this IServiceCollection services)
    {
        // Check if the actor system is registered
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            // Include the Redis SDK assembly for actor discovery
            var sdkAssembly = typeof(RedisMqPublisherActor).Assembly;
            var assemblies = new[] { Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), sdkAssembly };

            // Register minimal actor system for the producer
            services.AddActorSystemWithLifecycle(assemblies);
        }

        // Register publisher service if not already registered
        if (!services.Any(s => s.ServiceType == typeof(IRedisMqPublisher)))
        {
            services.AddScoped<IRedisMqPublisher, RedisMqPublisher>();
        }

        return services;
    }

    /// <summary>
    /// Adds only the Redis consumer functionality to the service collection
    /// </summary>
    public static IServiceCollection AddRedisMqConsumers(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Validate assemblies
        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided for consumer discovery",
                nameof(assemblies));
        }

        // Configure logging for diagnostic purposes
        var logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger("Redis Consumer Discovery");

        logger.LogInformation($"Scanning {assemblies.Length} assemblies for consumers");

        // Find all consumer classes using a HashSet to ensure uniqueness
        var consumerTypes = new HashSet<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                logger.LogInformation($"Scanning assembly: {assembly.FullName}");

                var assemblyConsumers = assembly.GetTypes()
                    .Where(t =>
                    {
                        // Check for public, non-abstract types with ConsumerAttribute
                        bool isPublicType = t.IsPublic && !t.IsAbstract;
                        bool hasConsumerAttribute = t.GetCustomAttribute<ConsumerAttribute>() != null;

                        if (isPublicType && hasConsumerAttribute)
                        {
                            logger.LogInformation($"Found consumer type: {t.FullName}");
                            logger.LogInformation($"  Namespace: {t.Namespace}");
                            logger.LogInformation($"  Assembly: {t.Assembly.FullName}");
                        }

                        return isPublicType && hasConsumerAttribute;
                    })
                    .ToList();

                logger.LogInformation($"Consumers found in {assembly.FullName}: {assemblyConsumers.Count}");

                // Add unique consumers to the HashSet
                foreach (var consumerType in assemblyConsumers)
                {
                    consumerTypes.Add(consumerType);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unexpected error scanning assembly {assembly.FullName}");
            }
        }

        logger.LogInformation($"Total unique consumer types discovered: {consumerTypes.Count}");

        // Register all consumers in DI
        foreach (var consumerType in consumerTypes)
        {
            services.AddScoped(consumerType);
            logger.LogInformation($"Registered consumer: {consumerType.FullName}");
        }

        // Find and register consumer methods
        var consumerMetadata = new List<ConsumerMetadata>();
        foreach (var consumerType in consumerTypes)
        {
            var methods = consumerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m =>
                {
                    var channelAttribute = m.GetCustomAttribute<ChannelAttribute>();
                    if (channelAttribute != null)
                    {
                        logger.LogInformation(
                            $"Found channel method: {consumerType.FullName}.{m.Name} with channel {channelAttribute.ChannelName}");
                    }

                    return channelAttribute != null;
                })
                .ToList();

            foreach (var method in methods)
            {
                var channelAttr = method.GetCustomAttribute<ChannelAttribute>();
                consumerMetadata.Add(new ConsumerMetadata
                {
                    ConsumerType = consumerType,
                    HandlerMethod = method,
                    ChannelAttribute = channelAttr
                });
            }
        }

        logger.LogInformation($"Total consumer metadata entries: {consumerMetadata.Count}");

        // Register consumer metadata
        services.AddSingleton(consumerMetadata);

        // Register consumer service if we have consumers
        if (consumerMetadata.Any() && !services.Any(s => s.ServiceType == typeof(RedisMqConsumerService)))
        {
            services.AddHostedService<RedisMqConsumerService>();
        }
        else if (!consumerMetadata.Any())
        {
            logger.LogWarning("No consumers found. Redis Consumer Service will not be registered.");
        }

        return services;
    }

    /// <summary>
    /// Initializes Redis publisher and consumer actors in a web application
    /// </summary>
    public static IApplicationBuilder UseRedisMqInApi(this IApplicationBuilder app)
    {
        var actorSystem = app.ApplicationServices.GetRequiredService<ActorSystem>();
        var connectionProvider = app.ApplicationServices.GetRequiredService<RedisMqConnectionProvider>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<RedisMqPublisherActor>>();
        var options = app.ApplicationServices.GetRequiredService<IOptions<RedisMqOptions>>();

        try
        {
            // Create props with error handling
            var props = Props.Create(() => new RedisMqPublisherActor(
                logger,
                options,
                connectionProvider.Connection
            ));

            // Try to create publisher actor
            try
            {
                var publisherActor = actorSystem.ActorOf(props, "redis-publisher");
                logger.LogInformation("[Redis] Successfully initialized publisher actor");
            }
            catch (InvalidActorNameException)
            {
                // Actor already exists
                logger.LogInformation("[Redis] Publisher actor already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Redis] Failed to initialize publisher actor");
        }

        return app;
    }

    /// <summary>
    /// Initializes Redis in a non-ASP.NET Core application
    /// </summary>
    public static IServiceProvider UseRedisMqInConsumer(this IServiceProvider serviceProvider)
    {
        var actorSystem = serviceProvider.GetRequiredService<ActorSystem>();
        var options = serviceProvider.GetRequiredService<IOptions<RedisMqOptions>>().Value;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RedisMqPublisherActor>();
        
        try
        {
            InitializeConsumer(options);
            var connectionProvider = serviceProvider.GetRequiredService<RedisMqConnectionProvider>();

            // First check if the actor is already registered via TopLevelActors
            bool actorExists = false;
            try
            {
                // Try to get the actor - if this succeeds, it's already registered
                var actor = TopLevelActors.GetActor<RedisMqPublisherActor>("_RedisMqPublisherActor");
                actorExists = true;
                logger.LogInformation("[Redis] Publisher actor already registered with TopLevelActors");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Actor not registered with TopLevelActors
                logger.LogInformation("[Redis] Publisher actor not registered with TopLevelActors");
                actorExists = false;
            }

            // If the actor doesn't exist, create and register it
            if (!actorExists)
            {
                try
                {
                    // Create actor the original way
                    var publisherActorRef = actorSystem.ActorOf(
                        Props.Create(() => new RedisMqPublisherActor(
                            logger,
                            serviceProvider.GetRequiredService<IOptions<RedisMqOptions>>(),
                            connectionProvider.Connection
                        )),
                        "redis-publisher"
                    );

                    // Register it with TopLevelActors
                    var registered =
                        TopLevelActors.RegisterActor<RedisMqPublisherActor>(actorSystem, "_RedisMqPublisherActor");

                    if (registered)
                    {
                        logger.LogInformation("[Redis] Successfully registered publisher actor with TopLevelActors");
                    }
                    else
                    {
                        logger.LogWarning("[Redis] Failed to register publisher actor with TopLevelActors");
                    }
                }
                catch (InvalidActorNameException)
                {
                    // Actor might already exist, which is fine
                    logger.LogInformation("[Redis] Publisher actor already exists");

                    try
                    {
                        var registered =
                            TopLevelActors.RegisterActor<RedisMqPublisherActor>(actorSystem,
                                "_RedisMqPublisherActor");

                        if (registered)
                        {
                            logger.LogInformation(
                                "[Redis] Successfully registered existing publisher actor with TopLevelActors");
                        }
                        else
                        {
                            logger.LogWarning(
                                "[Redis] Failed to register existing publisher actor with TopLevelActors");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex,
                            "[Redis] Error registering existing publisher actor with TopLevelActors");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Redis] Failed to create and register publisher actor");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[Redis] Failed to initialize Redis: {ex.Message}");
        }

        return serviceProvider;
    }
    
    public static void InitializeConsumer(RedisMqOptions options)
    {
        if (string.IsNullOrEmpty(options.Title))
            return;
            
        try
        {
            // Set console title
            Console.Title = options.Title;
            
            // Generate border (35 '=' characters)
            string border = new string('=', 35);
                
            // Generate startup message based on console title
            string startupMessage = $"Starting {options.Title}...";
                
            // Display startup message
            Console.WriteLine(border);
            Console.WriteLine(startupMessage);
            Console.WriteLine(border);
        }
        catch (Exception)
        {
            // Silently fail if console operations aren't supported
            // (e.g., when running as a service)
        }
    }
}
