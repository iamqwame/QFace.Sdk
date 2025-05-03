using QFace.Sdk.ActorSystems.Coordinator;

namespace QFace.Sdk.ActorSystems
{
    /// <summary>
    /// Extension methods for setting up coordinator actors and services
    /// </summary>
    public static class CoordinatorExtensions
    {
        /// <summary>
        /// Adds a coordinator actor and its worker actors to the actor system
        /// </summary>
        /// <typeparam name="TCoordinator">The coordinator actor type</typeparam>
        /// <typeparam name="TWorker">The worker actor type</typeparam>
        /// <param name="config">The actor configuration</param>
        /// <param name="workerInstances">Number of worker instances</param>
        /// <param name="workerUpperBound">Upper bound for worker instances</param>
        /// <param name="useRouterForWorkers">Whether to use a router for workers</param>
        /// <returns>The actor configuration</returns>
        public static ActorConfig AddCoordinator<TCoordinator, TWorker>(
            this ActorConfig config,
            int workerInstances = 5,
            int workerUpperBound = 20,
            bool useRouterForWorkers = true)
            where TCoordinator : CoordinatorActor
            where TWorker : WorkerActor
        {
            // Configure the coordinator (single instance)
            config.AddActorType<TCoordinator>(
                numberOfInstances: 1,
                upperBound: 1,
                useRouter: false);
            
            // Configure the workers
            config.AddActorType<TWorker>(
                numberOfInstances: workerInstances,
                upperBound: workerUpperBound,
                useRouter: useRouterForWorkers);
            
            return config;
        }
        
        /// <summary>
        /// Adds a coordinator background service to the service collection
        /// </summary>
        /// <typeparam name="TService">The coordinator service type</typeparam>
        /// <typeparam name="TCoordinator">The coordinator actor type</typeparam>
        /// <typeparam name="TWorker">The worker actor type</typeparam>
        /// <param name="services">The service collection</param>
        /// <param name="assemblies">The assemblies to scan for actors</param>
        /// <param name="configure">Optional configuration action</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddCoordinatorService<TService, TCoordinator, TWorker>(
            this IServiceCollection services,
            Assembly[] assemblies,
            Action<ActorConfig> configure = null)
            where TService : CoordinatorBackgroundService<TCoordinator>
            where TCoordinator : CoordinatorActor
            where TWorker : WorkerActor
        {
            // Add the actor system with lifecycle management
            services.AddActorSystemWithLifecycle(assemblies, config =>
            {
                // Add the coordinator and workers
                config.AddCoordinator<TCoordinator, TWorker>();
                
                // Apply additional configuration if provided
                configure?.Invoke(config);
            });
            
            // Add the background service
            services.AddHostedService<TService>();
            
            return services;
        }
        
        /// <summary>
        /// Adds a coordinator background service with minimal configuration
        /// </summary>
        /// <typeparam name="TService">The coordinator service type</typeparam>
        /// <typeparam name="TCoordinator">The coordinator actor type</typeparam>
        /// <typeparam name="TWorker">The worker actor type</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddCoordinatorService<TService, TCoordinator, TWorker>(
            this IServiceCollection services)
            where TService : CoordinatorBackgroundService<TCoordinator>
            where TCoordinator : CoordinatorActor
            where TWorker : WorkerActor
        {
            // Use the entry assembly plus the one containing the service
            var assemblies = new[]
            {
                Assembly.GetEntryAssembly(),
                typeof(TService).Assembly
            };
            
            return services.AddCoordinatorService<TService, TCoordinator, TWorker>(assemblies);
        }
    }
}