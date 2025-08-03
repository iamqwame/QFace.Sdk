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
        
        services.AddKafka(
            hostContext.Configuration,
            [typeof(Program).Assembly]);
    })
    .Build();

// Initialize Kafka for consumer
host.Services.UseKafkaInConsumer();
await host.RunAsync();
