using Microsoft.Extensions.Hosting;

namespace QFace.Sdk.SendMessage.Extensions
{
    /// <summary>
    /// Extension methods for registering messaging services
    /// </summary>
    public static class SendMessageExtensions
    {
        /// <summary>
        /// Adds email services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureActorSystem">Optional action to configure the actor system</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddEmailServices(this IServiceCollection services, Action<ActorConfig>? configureActorSystem = null)
        {
            // Register email service
            services.AddScoped<IEmailService, EmailService>();
            
            // Configure actor system if not already registered
            if (!services.Any(s => s.ServiceType == typeof(ActorSystem)))
            {
                // Include the SendMessage SDK assembly
                var sdkAssembly = typeof(SendEmailActor).Assembly;
                var assemblies = new[] { Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), sdkAssembly };

                // Register actor system with these assemblies
                services.AddActorSystemWithLifecycle(assemblies, config =>
                {
                    // Configure actor system with default settings
                    config.SystemName = "EmailActorSystem";

                    // Apply custom configuration if provided
                    configureActorSystem?.Invoke(config);
                });
            }
            
            return services;
        }
    }
}