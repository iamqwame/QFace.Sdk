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
            new[] { assembly },
            actorConfig => hostContext.Configuration.GetSection(nameof(ActorConfig)).Bind(actorConfig)
        );
        
        services.AddRedisMq(
            hostContext.Configuration,
            new[] { typeof(Program).Assembly });
    })
    .Build();

// Initialize Redis
host.Services.UseRedisMqInConsumer();
host.UseActorSystem();
await host.RunAsync();
