using System.Reflection;
using Microsoft.Extensions.Options;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.Kafka.Actors;
using QFace.Sdk.Kafka.Services;

namespace QFace.Sdk.Kafka.Extensions;

public static class KafkaExtensions
{
    /// <summary>
    /// Adds Kafka functionality to the service collection with support for both producers and consumers
    /// </summary>
    public static IServiceCollection AddKafka(this IServiceCollection services,
        IConfiguration configuration, Assembly[] consumerAssemblies,
        Action<ActorConfig>? configureActorSystem = null)
    {
        // Use a default configuration if none is provided
        configureActorSystem ??= config =>
        {
            var guid = Guid.NewGuid().ToString("N");
            config.SystemName = $"KafkaConsumerSystem{guid}";
        };

        // Get options from configuration
        services.Configure<KafkaConsumerConfig>(configuration.GetSection("KafkaConsumerConfig"));
        services.Configure<KafkaProducerConfig>(configuration.GetSection("KafkaProducerConfig"));
        services.Configure<MessageGroupConsumerLogicConfig>(configuration.GetSection("MessageGroupConsumerLogicConfig"));

        // Add validation
        services.AddSingleton<IValidateOptions<KafkaConsumerConfig>, KafkaConsumerConfigValidator>();
        
        // Register ITopLevelActors wrapper
        services.AddSingleton<ITopLevelActors, TopLevelActorsWrapper>();

        // Register the actor system if it's not already registered
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            // Include the Kafka SDK assembly
            var sdkAssembly = typeof(KafkaProducerActor).Assembly;
            var allAssemblies = consumerAssemblies.Concat(new[] { sdkAssembly }).Distinct().ToArray();

            // Register actor system with these assemblies
            services.AddActorSystemWithLifecycle(allAssemblies, config =>
            {
                // Configure actor system with default settings
                config.SystemName = "KafkaActorSystem";

                // Apply custom configuration if provided
                configureActorSystem?.Invoke(config);
            });
        }

        // Register producer service
        services.AddScoped<IKafkaProducer, KafkaProducer>();

        // Register Kafka consumers if assemblies are provided
        if (consumerAssemblies.Length > 0)
        {
            services.AddKafkaConsumers(consumerAssemblies);
        }

        return services;
    }

    /// <summary>
    /// Adds only the Kafka producer functionality to the service collection
    /// </summary>
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services)
    {
        // Check if the actor system is registered
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            // Include the Kafka SDK assembly for actor discovery
            var sdkAssembly = typeof(KafkaProducerActor).Assembly;
            var assemblies = new[] { Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), sdkAssembly };

            // Register minimal actor system for the producer
            services.AddActorSystemWithLifecycle(assemblies);
        }

        // Register producer service if not already registered
        if (!services.Any(s => s.ServiceType == typeof(IKafkaProducer)))
        {
            services.AddScoped<IKafkaProducer, KafkaProducer>();
        }

        return services;
    }

    /// <summary>
    /// Adds only the Kafka consumer functionality to the service collection
    /// </summary>
    public static IServiceCollection AddKafkaConsumers(this IServiceCollection services,
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
            .CreateLogger("Kafka Consumer Discovery");

        logger.LogInformation($"Scanning {assemblies.Length} assemblies for Kafka consumers");

        // Find all consumer classes
        var consumerTypes = new HashSet<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                logger.LogInformation($"Scanning assembly: {assembly.FullName}");

                var assemblyConsumers = assembly.GetTypes()
                    .Where(t =>
                    {
                        // Check for public types that inherit from KafkaConsumerBase
                        bool isConsumerType = t.IsPublic && !t.IsAbstract && 
                                            t.IsSubclassOf(typeof(KafkaConsumerBase));

                        if (isConsumerType)
                        {
                            logger.LogInformation($"Found Kafka consumer type: {t.FullName}");
                        }

                        return isConsumerType;
                    })
                    .ToList();

                logger.LogInformation($"Kafka consumers found in {assembly.FullName}: {assemblyConsumers.Count}");

                foreach (var consumerType in assemblyConsumers)
                {
                    consumerTypes.Add(consumerType);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error scanning assembly {assembly.FullName}");
            }
        }

        logger.LogInformation($"Total Kafka consumer types discovered: {consumerTypes.Count}");

        // Register all consumers in DI
        foreach (var consumerType in consumerTypes)
        {
            services.AddScoped(consumerType);
            logger.LogInformation($"Registered Kafka consumer: {consumerType.FullName}");
        }

        // Find and register consumer methods
        var consumerMetadata = DiscoverConsumerMethods(consumerTypes, services, logger);

        logger.LogInformation($"Total consumer metadata entries: {consumerMetadata.Count}");

        // Register consumer metadata
        services.AddSingleton(consumerMetadata);

        // Register consumer service if we have consumers
        if (consumerMetadata.Any() && !services.Any(s => s.ServiceType == typeof(KafkaConsumerService)))
        {
            services.AddHostedService<KafkaConsumerService>();
        }
        else if (!consumerMetadata.Any())
        {
            logger.LogWarning("No Kafka consumers found. Consumer service will not be registered.");
        }

        return services;
    }

    private static List<ConsumerMetadata> DiscoverConsumerMethods(IEnumerable<Type> consumerTypes, 
        IServiceCollection services, ILogger logger)
    {
        var consumerMetadata = new List<ConsumerMetadata>();
        
        // Build a temporary service provider to access configuration
        var tempServiceProvider = services.BuildServiceProvider();
        var kafkaConfig = tempServiceProvider.GetService<IOptions<KafkaConsumerConfig>>();
        
        foreach (var consumerType in consumerTypes)
        {
            var methods = consumerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m =>
                {
                    var topicAttribute = m.GetCustomAttribute<ConsumeTopicAttribute>();
                    if (topicAttribute != null)
                    {
                        logger.LogInformation(
                            $"Found topic method: {consumerType.FullName}.{m.Name} " +
                            $"(Group: {topicAttribute.TopicGroup ?? "none"}, DirectTopics: {string.Join(",", topicAttribute.DirectTopics ?? new string[0])})");
                    }

                    return topicAttribute != null;
                })
                .ToList();

            foreach (var method in methods)
            {
                var topicAttr = method.GetCustomAttribute<ConsumeTopicAttribute>();
                var consumerAttr = consumerType.GetCustomAttribute<KafkaConsumerAttribute>();
                
                try
                {
                    // Resolve topics for this consumer method
                    var topics = kafkaConfig != null ? 
                        TopicResolver.ResolveTopics(topicAttr, kafkaConfig) : 
                        new List<string>();

                    var metadata = new ConsumerMetadata
                    {
                        ConsumerType = consumerType,
                        HandlerMethod = method,
                        TopicAttribute = topicAttr,
                        Topics = topics,
                        TopicGroup = topicAttr.TopicGroup,
                        ConsumerGroupId = consumerAttr?.GroupId, // Can override default group ID
                        ProcessingConfig = new ProcessingConfig
                        {
                            MaxBatchSize = topicAttr.MaxBatchSize > 0 ? topicAttr.MaxBatchSize : 200,
                            BatchTimeoutMs = topicAttr.BatchTimeoutMs > 0 ? topicAttr.BatchTimeoutMs : 5000,
                            IsBulk = topicAttr.IsBulk,
                            CommitStrategy = OffsetCommitStrategy.AfterSuccessfulProcessing,
                            DeadLetterTopic = topicAttr.DeadLetterTopic
                        }
                    };
                    
                    consumerMetadata.Add(metadata);
                    
                    logger.LogInformation(
                        $"Registered consumer method: {consumerType.Name}.{method.Name} " +
                        $"-> Topics: [{string.Join(", ", topics)}]");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, 
                        $"Failed to resolve topics for {consumerType.FullName}.{method.Name}: {ex.Message}");
                    throw;
                }
            }
        }
        
        tempServiceProvider.Dispose();
        return consumerMetadata;
    }

    /// <summary>
    /// Initializes Kafka producer and consumer actors in a web application
    /// </summary>
    public static IApplicationBuilder UseKafkaInApi(this IApplicationBuilder app)
    {
        var actorSystem = app.ApplicationServices.GetRequiredService<ActorSystem>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<KafkaProducerActor>>();
        var producerConfig = app.ApplicationServices.GetRequiredService<IOptions<KafkaProducerConfig>>();

        try
        {
            // Create props with error handling
            var props = Props.Create(() => new KafkaProducerActor(
                logger,
                producerConfig
            ));

            // Try to create producer actor
            try
            {
                var producerActor = actorSystem.ActorOf(props, "kafka-producer");
                logger.LogInformation("[Kafka] Successfully initialized producer actor");
            }
            catch (InvalidActorNameException)
            {
                // Actor already exists
                logger.LogInformation("[Kafka] Producer actor already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Kafka] Failed to initialize producer actor");
        }

        return app;
    }

    /// <summary>
    /// Initializes Kafka in a non-ASP.NET Core application
    /// </summary>
    public static IServiceProvider UseKafkaInConsumer(this IServiceProvider serviceProvider)
    {
        var actorSystem = serviceProvider.GetRequiredService<ActorSystem>();
        var producerConfig = serviceProvider.GetRequiredService<IOptions<KafkaProducerConfig>>().Value;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<KafkaProducerActor>();
        
        try
        {
            InitializeConsumer(producerConfig);
            
            // Check if the actor is already registered via TopLevelActors
            bool actorExists = false;
            try
            {
                var actor = TopLevelActors.GetActor<KafkaProducerActor>("_KafkaProducerActor");
                actorExists = true;
                logger.LogInformation("[Kafka] Producer actor already registered with TopLevelActors");
            }
            catch (ArgumentOutOfRangeException)
            {
                logger.LogInformation("[Kafka] Producer actor not registered with TopLevelActors");
                actorExists = false;
            }

            // If the actor doesn't exist, create and register it
            if (!actorExists)
            {
                try
                {
                    // Create actor
                    var producerActorRef = actorSystem.ActorOf(
                        Props.Create(() => new KafkaProducerActor(
                            logger,
                            serviceProvider.GetRequiredService<IOptions<KafkaProducerConfig>>()
                        )),
                        "kafka-producer"
                    );

                    // Register it with TopLevelActors
                    var registered = TopLevelActors.RegisterActor<KafkaProducerActor>(actorSystem, "_KafkaProducerActor");

                    if (registered)
                    {
                        logger.LogInformation("[Kafka] Successfully registered producer actor with TopLevelActors");
                    }
                    else
                    {
                        logger.LogWarning("[Kafka] Failed to register producer actor with TopLevelActors");
                    }
                }
                catch (InvalidActorNameException)
                {
                    // Actor might already exist, which is fine
                    logger.LogInformation("[Kafka] Producer actor already exists");

                    try
                    {
                        var registered = TopLevelActors.RegisterActor<KafkaProducerActor>(actorSystem, "_KafkaProducerActor");
                        if (registered)
                        {
                            logger.LogInformation("[Kafka] Successfully registered existing producer actor with TopLevelActors");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[Kafka] Error registering existing producer actor with TopLevelActors");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Kafka] Failed to create and register producer actor");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[Kafka] Failed to initialize Kafka: {ex.Message}");
        }

        return serviceProvider;
    }
    
    public static void InitializeConsumer(KafkaProducerConfig config)
    {
        try
        {
            // Set console title if available
            Console.Title = "Kafka Consumer Service";
            
            // Generate border
            string border = new string('=', 40);
                
            // Display startup message
            Console.WriteLine(border);
            Console.WriteLine("Starting Kafka Consumer Service...");
            Console.WriteLine($"Bootstrap Servers: {config.BootstrapServers}");
            Console.WriteLine(border);
        }
        catch (Exception)
        {
            // Silently fail if console operations aren't supported
        }
    }
}

/// <summary>
/// Validator for Kafka consumer configuration
/// </summary>
public class KafkaConsumerConfigValidator : IValidateOptions<KafkaConsumerConfig>
{
    public ValidateOptionsResult Validate(string name, KafkaConsumerConfig options)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(options.BootstrapServers))
        {
            errors.Add("BootstrapServers is required");
        }
        
        if (string.IsNullOrWhiteSpace(options.GroupId))
        {
            errors.Add("GroupId is required");
        }
        
        // Validate topic groups
        foreach (var group in options.TopicGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Key))
            {
                errors.Add("Topic group names cannot be empty");
            }
            
            if (!group.Value.Any())
            {
                errors.Add($"Topic group '{group.Key}' contains no topics");
            }
            
            if (group.Value.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add($"Topic group '{group.Key}' contains empty topic names");
            }
        }
        
        // Validate batch processing settings
        if (options.MaxBatchSize <= 0)
        {
            errors.Add("MaxBatchSize must be greater than 0");
        }
        
        if (options.BatchTimeoutMs <= 0)
        {
            errors.Add("BatchTimeoutMs must be greater than 0");
        }
        
        return errors.Any() 
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
