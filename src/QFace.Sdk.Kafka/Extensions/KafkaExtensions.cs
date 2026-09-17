using System.Reflection;
using Microsoft.Extensions.Options;
using QFace.Sdk.ActorSystems;
using QFace.Sdk.Kafka.Actors;
using QFace.Sdk.Kafka.Services;

namespace QFace.Sdk.Kafka.Extensions;

/// <summary>
/// Kafka producer registration helpers.
/// Message-consumer hosts are retired — use Temporal workers in WebApi processes instead.
/// </summary>
public static class KafkaExtensions
{
    /// <summary>
    /// Adds Kafka producer functionality. Consumer assemblies are ignored (consumers removed).
    /// Prefer <see cref="AddKafkaProducer"/> for new code.
    /// </summary>
    [Obsolete("QimERP no longer uses Kafka consumer hosts. Use AddKafkaProducer + UseKafkaInApi, or Temporal workers in WebApi.")]
    public static IServiceCollection AddKafka(this IServiceCollection services,
        IConfiguration configuration, Assembly[] consumerAssemblies,
        Action<ActorConfig>? configureActorSystem = null)
    {
        _ = consumerAssemblies;
        return AddKafkaProducerCore(services, configuration, configureActorSystem);
    }

    /// <summary>
    /// Adds Kafka producer functionality to the service collection.
    /// </summary>
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ActorConfig>? configureActorSystem = null)
    {
        return AddKafkaProducerCore(services, configuration, configureActorSystem);
    }

    /// <summary>
    /// Adds only the Kafka producer functionality to the service collection (no config binding).
    /// </summary>
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services)
    {
        return AddKafkaProducerCore(services, configuration: null, configureActorSystem: null);
    }

    private static IServiceCollection AddKafkaProducerCore(
        IServiceCollection services,
        IConfiguration? configuration,
        Action<ActorConfig>? configureActorSystem)
    {
        configureActorSystem ??= config =>
        {
            var guid = Guid.NewGuid().ToString("N");
            config.SystemName = $"KafkaProducerSystem{guid}";
        };

        if (configuration is not null)
        {
            services.Configure<KafkaProducerConfig>(configuration.GetSection("KafkaProducerConfig"));
        }

        services.AddSingleton<IValidateOptions<KafkaProducerConfig>, KafkaProducerConfigValidator>();
        services.AddSingleton<ITopLevelActors, TopLevelActorsWrapper>();

        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            var sdkAssembly = typeof(KafkaProducerActor).Assembly;
            var entry = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var assemblies = new[] { entry, sdkAssembly }.Where(a => a is not null).Distinct().ToArray()!;

            services.AddActorSystemWithLifecycle(assemblies, config =>
            {
                config.SystemName = "KafkaActorSystem";
                configureActorSystem?.Invoke(config);
            });
        }

        if (!services.Any(s => s.ServiceType == typeof(IKafkaProducer)))
        {
            services.AddScoped<IKafkaProducer, KafkaProducer>();
        }

        return services;
    }

    /// <summary>
    /// Initializes Kafka producer actors in a web application.
    /// </summary>
    public static IApplicationBuilder UseKafkaInApi(this IApplicationBuilder app)
    {
        var actorSystem = app.ApplicationServices.GetRequiredService<ActorSystem>();
        var logger = app.ApplicationServices.GetRequiredService<ILogger<KafkaProducerActor>>();
        var producerConfig = app.ApplicationServices.GetRequiredService<IOptions<KafkaProducerConfig>>();

        try
        {
            var props = Props.Create(() => new KafkaProducerActor(
                logger,
                producerConfig
            ));

            try
            {
                actorSystem.ActorOf(props, "kafka-producer");
                logger.LogInformation("[Kafka] Successfully initialized producer actor");
            }
            catch (InvalidActorNameException)
            {
                logger.LogInformation("[Kafka] Producer actor already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Kafka] Failed to initialize producer actor");
        }

        return app;
    }
}

/// <summary>
/// Validator for Kafka producer configuration
/// </summary>
public class KafkaProducerConfigValidator : IValidateOptions<KafkaProducerConfig>
{
    public ValidateOptionsResult Validate(string name, KafkaProducerConfig options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BootstrapServers))
        {
            errors.Add("BootstrapServers is required for Kafka producer");
        }

        if (options.ProducerInstances <= 0)
        {
            errors.Add("ProducerInstances must be greater than 0");
        }

        if (options.ProducerUpperBound <= 0)
        {
            errors.Add("ProducerUpperBound must be greater than 0");
        }

        if (options.ProducerInstances > options.ProducerUpperBound)
        {
            errors.Add("ProducerInstances cannot be greater than ProducerUpperBound");
        }

        return errors.Any()
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
