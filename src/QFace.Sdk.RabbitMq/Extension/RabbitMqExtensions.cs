namespace QFace.Sdk.RabbitMq.Extension;

public static class RabbitMqExtensions
{
    /// <summary>
    /// Adds RabbitMQ functionality to the service collection with support for both producers and consumers
    /// </summary>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services,
        IConfiguration configuration, Assembly[] consumerAssemblies,
        Action<ActorConfig>? configureActorSystem = null)
    {
        // Use a default configuration if none is provided
        configureActorSystem ??= config =>
        {
            var guid = Guid.NewGuid().ToString("N");
            config.SystemName = $"MessageConsumerSystem{guid}";
        };

        // Get options from configuration
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        // Register connection provider as singleton
        services.AddSingleton<RabbitMqConnectionProvider>();
        
        services.AddSingleton<IConnection>(sp => 
            sp.GetRequiredService<RabbitMqConnectionProvider>().Connection);
        services.AddSingleton<IModel>(sp => 
            sp.GetRequiredService<RabbitMqConnectionProvider>().Channel);

        // Register the actor system if it's not already registered
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            // Include the RabbitMQ SDK assembly
            var sdkAssembly = typeof(RabbitMqPublisherActor).Assembly;
            var allAssemblies = consumerAssemblies.Concat(new[] { sdkAssembly }).Distinct().ToArray();

            // Register actor system with these assemblies
            services.AddActorSystemWithLifecycle(allAssemblies, config =>
            {
                // Configure actor system with default settings
                config.SystemName = "RabbitMQActorSystem";

                // Apply custom configuration if provided
                configureActorSystem?.Invoke(config);
            });
        }

        // Register publisher service
        services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

        // Register RabbitMQ consumers if assemblies are provided
        if (consumerAssemblies.Length > 0)
        {
            services.AddRabbitMqConsumers(consumerAssemblies);
        }

        return services;
    }

    /// <summary>
    /// Adds only the RabbitMQ producer functionality to the service collection
    /// </summary>
    public static IServiceCollection AddRabbitMqProducer(this IServiceCollection services)
    {
        // Check if the actor system is registered
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            // Include the RabbitMQ SDK assembly for actor discovery
            var sdkAssembly = typeof(RabbitMqPublisherActor).Assembly;
            var assemblies = new[] { Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), sdkAssembly };

            // Register minimal actor system for the producer
            services.AddActorSystemWithLifecycle(assemblies);
        }

        // Register publisher service if not already registered
        if (!services.Any(s => s.ServiceType == typeof(IRabbitMqPublisher)))
        {
            services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
        }

        return services;
    }

    /// <summary>
    /// Adds only the RabbitMQ consumer functionality to the service collection
    /// </summary>
    public static IServiceCollection AddRabbitMqConsumers(this IServiceCollection services,
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
            .CreateLogger("RabbitMQ Consumer Discovery");

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
                    var topicAttribute = m.GetCustomAttribute<TopicAttribute>();
                    if (topicAttribute != null)
                    {
                        logger.LogInformation(
                            $"Found topic method: {consumerType.FullName}.{m.Name} with routing key {topicAttribute.RoutingKey}");
                    }

                    return topicAttribute != null;
                })
                .ToList();

            foreach (var method in methods)
            {
                var topicAttr = method.GetCustomAttribute<TopicAttribute>();
                consumerMetadata.Add(new ConsumerMetadata
                {
                    ConsumerType = consumerType,
                    HandlerMethod = method,
                    TopicAttribute = topicAttr
                });
            }
        }

        logger.LogInformation($"Total consumer metadata entries: {consumerMetadata.Count}");

        // Register consumer metadata
        services.AddSingleton(consumerMetadata);

        // Register consumer service if we have consumers
        if (consumerMetadata.Any() && !services.Any(s => s.ServiceType == typeof(RabbitMqConsumerService)))
        {
            services.AddHostedService<RabbitMqConsumerService>();
        }
        else if (!consumerMetadata.Any())
        {
            logger.LogWarning("No consumers found. RabbitMQ Consumer Service will not be registered.");
        }

        return services;
    }

    /// <summary>
    /// Initializes RabbitMQ publisher and consumer actors in a web application
    /// </summary>
    public static IApplicationBuilder UseRabbitMqInApi(this IApplicationBuilder app)
    {
        var actorSystem = app.ApplicationServices.GetRequiredService<ActorSystem>();
        var connectionProvider = app.ApplicationServices.GetRequiredService<RabbitMqConnectionProvider>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<RabbitMqPublisherActor>>();
        var options = app.ApplicationServices.GetRequiredService<IOptions<RabbitMqOptions>>();

        try
        {
            // Create props with error handling
            var props = Props.Create(() => new RabbitMqPublisherActor(
                logger,
                options,
                connectionProvider.Connection,
                connectionProvider.Channel
            ));

            // Try to create publisher actor
            try
            {
                var publisherActor = actorSystem.ActorOf(props, "rabbitmq-publisher");
                logger.LogInformation("[RabbitMQ] Successfully initialized publisher actor");
            }
            catch (InvalidActorNameException)
            {
                // Actor already exists
                logger.LogInformation("[RabbitMQ] Publisher actor already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[RabbitMQ] Failed to initialize publisher actor");
        }

        return app;
    }

    /// <summary>
    /// Initializes RabbitMQ in a non-ASP.NET Core application
    /// </summary>
    public static IServiceProvider UseRabbitMqInConsumer(this IServiceProvider serviceProvider)
    {
        var actorSystem = serviceProvider.GetRequiredService<ActorSystem>();
        var options = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<RabbitMqPublisherActor>();
        
        try
        {
            InitializeConsumer(options);
            var factory = new ConnectionFactory
            {
                Uri = new Uri(options.ConnectionString),
                AutomaticRecoveryEnabled = options.AutomaticRecoveryEnabled
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            // First check if the actor is already registered via TopLevelActors
            bool actorExists = false;
            try
            {
                // Try to get the actor - if this succeeds, it's already registered
                var actor = TopLevelActors.GetActor<RabbitMqPublisherActor>("_RabbitMqPublisherActor");
                actorExists = true;
                logger.LogInformation("[RabbitMQ] Publisher actor already registered with TopLevelActors");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Actor not registered with TopLevelActors
                logger.LogInformation("[RabbitMQ] Publisher actor not registered with TopLevelActors");
                actorExists = false;
            }

            // If the actor doesn't exist, create and register it
            if (!actorExists)
            {
                try
                {
                    // Create actor the original way
                    var publisherActorRef = actorSystem.ActorOf(
                        Props.Create(() => new RabbitMqPublisherActor(
                            logger,
                            serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>(),
                            connection,
                            channel
                        )),
                        "rabbitmq-publisher"
                    );

                    // Register it with TopLevelActors
                    var registered =
                        TopLevelActors.RegisterActor<RabbitMqPublisherActor>(actorSystem, "_RabbitMqPublisherActor");

                    if (registered)
                    {
                        logger.LogInformation("[RabbitMQ] Successfully registered publisher actor with TopLevelActors");
                    }
                    else
                    {
                        logger.LogWarning("[RabbitMQ] Failed to register publisher actor with TopLevelActors");
                    }
                }
                catch (InvalidActorNameException)
                {
                    // Actor might already exist, which is fine
                    logger.LogInformation("[RabbitMQ] Publisher actor already exists");

                    try
                    {
                        var registered =
                            TopLevelActors.RegisterActor<RabbitMqPublisherActor>(actorSystem,
                                "_RabbitMqPublisherActor");

                        if (registered)
                        {
                            logger.LogInformation(
                                "[RabbitMQ] Successfully registered existing publisher actor with TopLevelActors");
                        }
                        else
                        {
                            logger.LogWarning(
                                "[RabbitMQ] Failed to register existing publisher actor with TopLevelActors");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex,
                            "[RabbitMQ] Error registering existing publisher actor with TopLevelActors");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[RabbitMQ] Failed to create and register publisher actor");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[RabbitMQ] Failed to initialize RabbitMQ: {ex.Message}");
        }

        return serviceProvider;
    }
    
    public static void InitializeConsumer(RabbitMqOptions options)
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