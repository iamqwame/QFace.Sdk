var assembly = typeof(Program).Assembly;
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddEnvironmentVariables();
        config.AddCommandLine(args);
    })
    .ConfigureServices((hostContext, services) =>
    {
        
        services.AddActorSystemWithLifecycle(
            [assembly],
            actorConfig => hostContext.Configuration.GetSection(nameof(ActorConfig)).Bind(actorConfig)
        );
        
        services.AddRabbitMq(
            hostContext.Configuration,
            [typeof(Program).Assembly]);
    })
    .Build();

// Initialize RabbitMQ
host.Services.UseRabbitMqInConsumer();
host.UseActorSystem();
await host.RunAsync();