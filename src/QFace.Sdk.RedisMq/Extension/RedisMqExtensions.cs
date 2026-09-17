namespace QFace.Sdk.RedisMq.Extension;

/// <summary>
/// Redis MQ publisher registration helpers.
/// Message-consumer hosts are retired — use Temporal workers in WebApi processes instead.
/// </summary>
public static class RedisMqExtensions
{
    /// <summary>
    /// Adds Redis MQ publisher functionality. Consumer assemblies are ignored (consumers removed).
    /// Prefer <see cref="AddRedisMqProducer"/> for new code.
    /// </summary>
    [Obsolete("QimERP no longer uses RedisMq consumer hosts. Use AddRedisMqProducer + UseRedisMqInApi, or Temporal workers in WebApi.")]
    public static IServiceCollection AddRedisMq(this IServiceCollection services,
        IConfiguration configuration, Assembly[] consumerAssemblies,
        Action<ActorConfig>? configureActorSystem = null)
    {
        _ = consumerAssemblies;
        return AddRedisMqProducerCore(services, configuration, configureActorSystem);
    }

    /// <summary>
    /// Adds Redis MQ publisher functionality to the service collection.
    /// </summary>
    public static IServiceCollection AddRedisMqProducer(this IServiceCollection services,
        IConfiguration configuration,
        Action<ActorConfig>? configureActorSystem = null)
    {
        return AddRedisMqProducerCore(services, configuration, configureActorSystem);
    }

    /// <summary>
    /// Adds only the Redis publisher (assumes RedisMq options already configured).
    /// </summary>
    public static IServiceCollection AddRedisMqProducer(this IServiceCollection services)
    {
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            var sdkAssembly = typeof(RedisMqPublisherActor).Assembly;
            var assemblies = new[] { Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), sdkAssembly };

            services.AddActorSystemWithLifecycle(assemblies);
        }

        if (!services.Any(s => s.ServiceType == typeof(IRedisMqPublisher)))
        {
            services.AddScoped<IRedisMqPublisher, RedisMqPublisher>();
        }

        return services;
    }

    private static IServiceCollection AddRedisMqProducerCore(
        IServiceCollection services,
        IConfiguration configuration,
        Action<ActorConfig>? configureActorSystem)
    {
        configureActorSystem ??= config =>
        {
            var guid = Guid.NewGuid().ToString("N");
            config.SystemName = $"RedisMessagePublisherSystem{guid}";
        };

        services.Configure<RedisMqOptions>(configuration.GetSection("RedisMq"));

        if (!services.Any(s => s.ServiceType == typeof(RedisMqConnectionProvider)))
        {
            services.AddSingleton<RedisMqConnectionProvider>();
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                sp.GetRequiredService<RedisMqConnectionProvider>().Connection);
            services.AddSingleton<IDatabase>(sp =>
                sp.GetRequiredService<RedisMqConnectionProvider>().Database);
            services.AddSingleton<ISubscriber>(sp =>
                sp.GetRequiredService<RedisMqConnectionProvider>().Subscriber);
        }

        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            var sdkAssembly = typeof(RedisMqPublisherActor).Assembly;
            var entry = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var allAssemblies = new[] { entry, sdkAssembly }.Where(a => a is not null).Distinct().ToArray()!;

            services.AddActorSystemWithLifecycle(allAssemblies, config =>
            {
                config.SystemName = "RedisMQActorSystem";
                configureActorSystem?.Invoke(config);
            });
        }

        if (!services.Any(s => s.ServiceType == typeof(IRedisMqPublisher)))
        {
            services.AddScoped<IRedisMqPublisher, RedisMqPublisher>();
        }

        return services;
    }

    /// <summary>
    /// Initializes Redis publisher actors in a web application.
    /// </summary>
    public static IApplicationBuilder UseRedisMqInApi(this IApplicationBuilder app)
    {
        var actorSystem = app.ApplicationServices.GetRequiredService<ActorSystem>();
        var connectionProvider = app.ApplicationServices.GetRequiredService<RedisMqConnectionProvider>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<RedisMqPublisherActor>>();
        var options = app.ApplicationServices.GetRequiredService<IOptions<RedisMqOptions>>();

        try
        {
            var props = Props.Create(() => new RedisMqPublisherActor(
                logger,
                options,
                connectionProvider.Connection
            ));

            try
            {
                actorSystem.ActorOf(props, "redis-publisher");
                logger.LogInformation("[Redis] Successfully initialized publisher actor");
            }
            catch (InvalidActorNameException)
            {
                logger.LogInformation("[Redis] Publisher actor already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Redis] Failed to initialize publisher actor");
        }

        return app;
    }
}
