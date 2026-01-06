namespace QFace.Sdk.SendMessage.Extensions;

/// <summary>
/// Extension methods for registering messaging services
/// </summary>
public static class SendMessageExtensions
{
    /// <summary>
    /// Adds messaging services (email and SMS) to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="configureActorSystem">Optional action to configure the actor system</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ActorConfig> configureActorSystem = null)
    {
        // Register configuration
        services.Configure<MessageConfig>(configuration.GetSection("MessageSettings"));
            
        // Register email provider based on configuration
        var messageConfig = configuration.GetSection("MessageSettings").Get<MessageConfig>();
        var emailProvider = messageConfig?.Email?.Provider?.ToUpperInvariant();
        
        if (emailProvider == "GRAPH")
        {
            services.AddSingleton<IEmailProvider, GraphEmailProvider>();
        }
        else
        {
            services.AddSingleton<IEmailProvider, SmtpEmailProvider>();
        }
        
        // Register SMS provider
        services.AddSingleton<ISmsProvider, RestSmsProvider>();
            
        // Register message service
        services.AddScoped<IMessageService, MessageService>();
            
        // Configure actor system if not already registered
        RegisterActorSystem(services, configureActorSystem);
            
        return services;
    }
        
    private static void RegisterActorSystem(
        IServiceCollection services, 
        Action<ActorConfig> configureActorSystem)
    {
        // Ensure actor system is not already registered
        if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
        {
            // Include the SendMessage SDK assembly
            var sdkAssembly = typeof(SendMessageActor).Assembly;
            var assemblies = new[] { Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), sdkAssembly };

            // Register actor system with these assemblies
            services.AddActorSystemWithLifecycle(assemblies, config =>
            {
                // Configure actor system with default settings
                config.SystemName = "MessagingActorSystem";

                // Apply custom configuration if provided
                configureActorSystem?.Invoke(config);
            });
        }

        // Register a factory for creating SendMessageActor
        services.AddTransient(provider =>
        {
            return (Func<IActorRef>)(() =>
            {
                var actorSystem = provider.GetRequiredService<ActorSystem>();
                var logger = provider.GetRequiredService<ILogger<SendMessageActor>>();
                var emailProvider = provider.GetRequiredService<IEmailProvider>();
                var smsProvider = provider.GetRequiredService<ISmsProvider>();

                return actorSystem.ActorOf(
                    SendMessageActor.Create(logger, emailProvider, smsProvider), 
                    "SendMessageActor"
                );
            });
        });
    }
}