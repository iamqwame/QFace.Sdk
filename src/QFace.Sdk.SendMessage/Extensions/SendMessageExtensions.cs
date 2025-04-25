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
        
        /// <summary>
        /// Initializes the email actor system in an application
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>The service provider for chaining</returns>
        public static IServiceProvider UseEmailServices(this IServiceProvider serviceProvider)
        {
            var actorSystem = serviceProvider.GetRequiredService<ActorSystem>();
            
            try
            {
                // Create email actor
                var props = DependencyResolver.For(actorSystem).Props<SendEmailActor>();
                var emailActor = actorSystem.ActorOf(props, "email-sender");
                
                // Register with TopLevelActors
                TopLevelActors.RegisterActor<SendEmailActor>(actorSystem, "_SendEmailActor");
            }
            catch (InvalidActorNameException)
            {
                // Actor already exists, which is fine
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetService<ILogger<SendEmailActor>>();
                logger?.LogError(ex, "Failed to initialize email actor");
            }
            
            return serviceProvider;
        }
        
        /// <summary>
        /// Sends an email using the actor system
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="command">The email command</param>
        public static void SendEmail(this IServiceProvider serviceProvider, SendEmailCommand command)
        {
            try
            {
                var actorSystem = serviceProvider.GetRequiredService<ActorSystem>();
                var emailActor = actorSystem.ActorSelection("/user/email-sender").ResolveOne(TimeSpan.FromSeconds(1)).Result;
                emailActor.Tell(command);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetService<ILogger<SendEmailActor>>();
                logger?.LogError(ex, "Failed to send email through actor system");
                
                // Fallback to direct email service
                try
                {
                    var emailService = serviceProvider.GetRequiredService<IEmailService>();
                    if (string.IsNullOrEmpty(command.Template))
                    {
                        emailService.SendEmailAsync(command.ToEmails, command.Subject, command.Body).Wait();
                    }
                    else
                    {
                        emailService.SendEmailWithTemplateAsync(command.ToEmails, command.Subject, command.Template, command.Replacements).Wait();
                    }
                }
                catch (Exception fallbackEx)
                {
                    logger?.LogError(fallbackEx, "Failed to send email through fallback method");
                }
            }
        }
    }
}