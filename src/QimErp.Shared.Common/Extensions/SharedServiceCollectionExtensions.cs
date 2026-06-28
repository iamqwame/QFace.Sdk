using System.Text.Json.Serialization;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using QFace.Sdk.RedisCache.Extensions;
using QFace.Sdk.RedisCache.Models;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Behaviours;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Middlewares;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.TenantActivity;
using QimErp.Shared.Common.Services.Workflow;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace QimErp.Shared.Common.Extensions;

public static class SharedServiceCollectionExtensions
{
    /// <summary>
    /// Registers QimErp configuration options (FrontendSettings, System).
    /// Call from consuming application startup.
    /// </summary>
    public static IServiceCollection AddQimErpConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQimErpConfigurationWithDefaults();
        services.Configure<FrontendSettings>(configuration.GetSection(FrontendSettings.SectionName));
        services.Configure<SystemOptions>(configuration.GetSection(SystemOptions.SectionName));
        return services;
    }

    /// <summary>
    /// Registers QimErp options with default values.
    /// Use when IConfiguration is not available (e.g. AddDbContextWithOutbox without config).
    /// Ensures IOptions&lt;T&gt; resolves for FrontendSettings and SystemOptions.
    /// </summary>
    public static IServiceCollection AddQimErpConfigurationWithDefaults(this IServiceCollection services)
    {
        services.Configure<FrontendSettings>(_ => { });
        services.Configure<SystemOptions>(_ => { });
        return services;
    }

    public static void AddApplicationTestDbContext<TContext>(
        this IServiceCollection services, string connectionString) where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<TContext>(options =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();
            options.UseNpgsql(dataSource);
        });
    }

    public static IServiceCollection RegisterMediatR(
        this IServiceCollection services, Assembly[] allCurrentAssemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(allCurrentAssemblies);
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehaviour<,>));

        return services;
    }
    /// <summary>
    /// Registers DbContext with outbox support and audit interceptor.
    /// </summary>
    /// <param name="configuration">Optional. When provided, options bind from config; when null, default values are used.</param>
    /// <remarks>
    /// Requires ICurrentUserService. Registers DesignTimeCurrentUserService as fallback via TryAdd.
    /// Call AddCoreServices/AddAuth before or after to use UserContextService (HTTP-based user) instead.
    /// </remarks>
    public static IServiceCollection AddDbContextWithOutbox<TContext>(
        this IServiceCollection services, string connectionString, IConfiguration? configuration = null) where TContext : ApplicationDbContext<TContext>
    {
        if (configuration != null)
            services.AddQimErpConfiguration(configuration);
        else
            services.AddQimErpConfigurationWithDefaults();

        services.TryAddScoped<ICurrentUserService, DesignTimeCurrentUserService>();
        services.AddScoped<ITenantContext, TenantContext>();

        services
            .AddScoped<AuditEntitySaveChangesInterceptor>()
            .AddDbContext<TContext>((provider, options) =>
        {
#pragma warning disable CS0618
            NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618
            options.UseNpgsql(connectionString);
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
            options.ConfigureWarnings(warnings => warnings.Ignore(
                RelationalEventId.CommandExecuting,
                RelationalEventId.CommandExecuted,
                RelationalEventId.PendingModelChangesWarning));
            var interceptor = provider.GetRequiredService<AuditEntitySaveChangesInterceptor>();
            options.AddInterceptors(interceptor);
        });

        services.AddScoped<IWorkflowAwareContext>(provider =>
            provider.GetRequiredService<TContext>());

        return services;
    }

    /// <summary>
    /// Registers DbContext with outbox support and audit interceptor for consumer/background apps.
    /// </summary>
    /// <param name="configuration">Optional. When provided, options bind from config; when null, default values are used.</param>
    public static IServiceCollection AddDbContextWithOutboxConsumer<TContext>(
        this IServiceCollection services, string connectionString, IConfiguration? configuration = null) where TContext : ApplicationDbContext<TContext>
    {
        if (configuration != null)
            services.AddQimErpConfiguration(configuration);
        else
            services.AddQimErpConfigurationWithDefaults();

        services.AddSingleton<TenantContext>();
        services.AddSingleton<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        // ITenantScopeSetter allows Temporal activity interceptors to seed ITenantContext so that
        // EF global query filters receive the correct TenantId during background activity execution.
        services.AddSingleton<ITenantScopeSetter>(
            sp => sp.GetRequiredService<TenantContext>());
        services.AddSingleton<ConsumerUserContextService>();
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<ConsumerUserContextService>());
        services.AddSingleton<ITenantContextSetter>(sp => sp.GetRequiredService<ConsumerUserContextService>());

        services
            .AddScoped<AuditEntitySaveChangesInterceptor>()
            .AddDbContext<TContext>((provider, options) =>
            {
    #pragma warning disable CS0618
            NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618
                options.UseNpgsql(connectionString);
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
                options.ConfigureWarnings(warnings => warnings.Ignore(
                    RelationalEventId.CommandExecuting,
                    RelationalEventId.CommandExecuted));
                var interceptor = provider.GetRequiredService<AuditEntitySaveChangesInterceptor>();
                options.AddInterceptors(interceptor);
            });
        services.AddScoped<IWorkflowAwareContext>(provider =>
            provider.GetRequiredService<TContext>());

        return services;
    }

    /// <summary>
    /// Adds CORS configuration allowing all origins, methods, and headers.
    /// Also creates a SignalR-specific CORS policy with credentials support.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration to read CORS origins from (optional)</param>
    /// <remarks>
    /// SignalR requires specific origins with AllowCredentials() for JWT authentication.
    /// The browser CORS specification prohibits AllowAnyOrigin() with AllowCredentials() together.
    /// </remarks>
    public static IServiceCollection AddCorsConfig(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddCors(options =>
        {
            // General API CORS policy (for development, allows any origin without credentials)
            options.AddPolicy("AllowAll",
                policy => policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

            // SignalR CORS policy with credentials support
            // Must use specific origins when AllowCredentials() is used (cannot use AllowAnyOrigin())
            var allowedOrigins = configuration?.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                 ?? ["http://localhost:3000", "https://localhost:3000"];

            options.AddPolicy("SignalRCors",
                policy => policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });
        return services;
    }

    /// <summary>
    /// Adds Swagger documentation services with JWT Bearer authentication support.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services,
        Assembly apiAssembly, string title, string version = "v1", string? description = null)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = description ?? $"{title} API Documentation"
            });

            // Add custom schema ID generation to prevent conflicts
            options.CustomSchemaIds(type =>
            {
                // Handle generic types properly by using the full type name
                if (type.IsGenericType)
                {
                    // Get the generic type definition name (e.g., "Result`1")
                    string genericTypeName = type.GetGenericTypeDefinition().Name;
                    // Get the generic type arguments
                    var typeArguments = type.GetGenericArguments();
                    var typeArgumentNames = typeArguments.Select(t =>
                    {
                        // Handle nested generic types
                        return t.IsGenericType ? $"{t.GetGenericTypeDefinition().Name}_{string.Join("_", t.GetGenericArguments().Select(ta => ta.Name))}" : t.Name;
                    }).ToArray();

                    // Create a unique schema ID that includes the generic arguments
                    return $"{genericTypeName}_{string.Join("_", typeArgumentNames)}";
                }

                // Handle nested classes by including the declaring type
                if (type.DeclaringType != null)
                {
                    string className = type.Name;
                    string declaringTypeName = type.DeclaringType.Name;
                    string nameSpace = type.Namespace?.Split('.').SkipLast(1).LastOrDefault() ?? "";
                    return $"{nameSpace}_{declaringTypeName}_{className}";
                }

                // For non-generic, non-nested types, use the original logic
                string simpleClassName = type.Name;
                string simpleNameSpace = type.FullName?.Split('.').SkipLast(1).LastOrDefault() ?? "";
                return $"{simpleNameSpace}_{simpleClassName}";
            });

            // Add JWT Bearer authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
            });

            // Include XML documentation from the API assembly
            string xmlFile = $"{apiAssembly.GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Ensure controllers from the API assembly are discovered
            options.DocInclusionPredicate((docName, description) => true);
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware.
    /// </summary>
    public static void UseSwaggerDocumentation(this WebApplication app,
        string title,
        string version = "v1")
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            // Get proxy path from environment variable or configuration
            var proxyPath = app.Configuration["Swagger:ProxyPath"] ??
                           Environment.GetEnvironmentVariable("SWAGGER_PROXY_PATH");

            var swaggerEndpoint = !string.IsNullOrEmpty(proxyPath)
                ? $"{proxyPath}/swagger/{version}/swagger.json"
                : $"/swagger/{version}/swagger.json";

            options.SwaggerEndpoint(swaggerEndpoint, $"{title} {version}");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
            options.DocExpansion(DocExpansion.None);
            options.DefaultModelsExpandDepth(-1);
        });
    }

    /// <summary>
    /// Configures authentication, authorization, and default middleware for the application.
    /// </summary>
    public static void UseAppSecurity(this WebApplication app, IConfiguration configuration)
    {
        app.UseCorrelationId();
        app.UseRequestLogging(configuration);
        app.UseResponseLogging(configuration);

        app.UseAntiforgery();

        app.UseCors("AllowAll");
        var httpsPort = configuration["HTTPS_PORT"]
            ?? configuration["ASPNETCORE_HTTPS_PORT"];
        var aspNetCoreUrls = configuration["ASPNETCORE_URLS"];
        var hasHttpsBinding = !string.IsNullOrWhiteSpace(httpsPort)
            || (!string.IsNullOrWhiteSpace(aspNetCoreUrls)
                && aspNetCoreUrls.Contains("https://", StringComparison.OrdinalIgnoreCase));

        if (hasHttpsBinding)
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseTenantContext();
        app.UseAuthorization();
        app.UseUserLoggingScope();

        app.MapDefaultEndpoints();
    }
    /// <summary>
    /// Configures core services including Authentication, CORS, MediatR, Carter, and JSON serialization settings.
    /// OpenTelemetry export is not registered here; call <see cref="AddQimErpOpenTelemetryDefaults"/> on <see cref="WebApplicationBuilder"/> early in host setup when you want ASP.NET/EF/OTLP instrumentation.
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly, Assembly? workFlowAssembly = null)
    {

        var isAddWorkflow = workFlowAssembly != null;

        var carterCatalog = isAddWorkflow ? new DependencyContextAssemblyCatalog(
            workFlowAssembly,
            assembly
        ) : new DependencyContextAssemblyCatalog(assembly);


        services.AddMemoryCache(); // For configuration caching
        services.AddQimErpConfiguration(configuration);

        // Add antiforgery services (required by UseAntiforgery in UseAppSecurity)
        services.AddAntiforgery();

        // Register ITenantContext (required by UseTenantContext middleware in UseAppSecurity)
        // Use TryAddScoped to avoid duplicate registration if already registered by AddDbContextWithOutbox
        services.TryAddScoped<ITenantContext, TenantContext>();

        // Register SDK Redis Cache services (reads from "RedisCache" configuration section)
        services.AddRedisCache(configuration);

        // use RedisCache:ConnectionString and convert redis:// URLs to StackExchange format.
        services.PostConfigure<RedisCacheOptions>(opts =>
        {
            var useFlatConfig = string.IsNullOrWhiteSpace(opts.StackExchange.ConnectionString) ||
                               opts.StackExchange.ConnectionString == "localhost:6379";
            var flatConnectionString = configuration["RedisCache:ConnectionString"];
            if (useFlatConfig && !string.IsNullOrWhiteSpace(flatConnectionString))
            {
                opts.StackExchange.ConnectionString = ToStackExchangeConnectionString(flatConnectionString);
                opts.StackExchange.Database = configuration.GetValue("RedisCache:Database", opts.StackExchange.Database);
                opts.StackExchange.ConnectTimeout = configuration.GetValue("RedisCache:ConnectTimeout", opts.StackExchange.ConnectTimeout);
                opts.StackExchange.SyncTimeout = configuration.GetValue("RedisCache:SyncTimeout", opts.StackExchange.SyncTimeout);
                opts.StackExchange.AbortOnConnectFail = configuration.GetValue("RedisCache:AbortOnConnectFail", opts.StackExchange.AbortOnConnectFail);
            }
        });

        // Register cache services (adapter that uses SDK)
        services.AddScoped<IDistributedCacheService, RedisCacheService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddAuth(configuration);
        services.AddCorsConfig(configuration);
        services.RegisterMediatR(isAddWorkflow ? [assembly, workFlowAssembly!] : [assembly]);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        services.AddCarter(carterCatalog);
        services.AddValidatorsFromAssembly(assembly);
        services.AddHealthChecks()
            .AddCheck("liveness", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck("readiness", () => HealthCheckResult.Healthy(), tags: ["ready"])
            .AddQimErpRedisHealthCheck();

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            options.SerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());

            // Performance optimizations
            options.SerializerOptions.WriteIndented = false; // Explicitly disable indentation for faster serialization
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // Skip null properties
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // Consistent camelCase naming
        });

        // Always register workflow cache service and workflow service for all modules
        services.AddScoped<IWorkflowConfigCacheService, WorkflowConfigCacheService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IWorkflowValidationService, WorkflowValidationService>();
        services.AddScoped<IDynamicHtmlGenerator, DynamicHtmlGenerator>();
        services.AddScoped<IWorkflowApprovalProcessor, WorkflowApprovalProcessor>();
        services.AddScoped<IWorkflowRejectionProcessor, WorkflowRejectionProcessor>();
        services.AddScoped<IWorkflowEntityApprovalHandlerRegistry, WorkflowEntityApprovalHandlerRegistry>();


        return services;
    }

    /// <summary>
    /// Converts redis:// URL to StackExchange format (host:port,password=xxx).
    /// ConfigurationOptions.Parse does not support redis:// URLs.
    /// </summary>
    private static string ToStackExchangeConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        if (!connectionString.StartsWith("redis://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        var uri = new Uri(connectionString);
        var port = uri.Port > 0 ? uri.Port : 6379;
        var password = uri.UserInfo?.Split(':').LastOrDefault();
        var parts = new List<string> { $"{uri.Host}:{port}" };
        if (!string.IsNullOrEmpty(password))
            parts.Add($"password={password}");
        if (uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase))
            parts.Add("ssl=true");
        return string.Join(",", parts);
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);
        JwtSettings jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings not found.");

        // ✅ Configure Authentication (JWT Bearer)
        services
            .AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            })
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero, // No extra delay for token expiration
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    RequireExpirationTime = true,
                    ValidateLifetime = true // Ensures expired tokens are rejected
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
            });

        // ✅ Apply Global Authorization Policy
        // services.AddAuthorization(options =>
        // {
        //     options.FallbackPolicy = new AuthorizationPolicyBuilder()
        //         .RequireAuthenticatedUser()
        //         .Build();
        //     
        //     // 🔹 Allow `/scalar/*` endpoints to be accessed without authentication
        //     options.AddPolicy("AllowScalar", policy =>
        //     {
        //         policy.RequireAssertion(context =>
        //         {
        //             if (context.Resource is HttpContext httpContext)
        //             {
        //                 return httpContext.Request.Path.StartsWithSegments("/scalar");
        //             }
        //             return false;
        //         });
        //     });
        // });

        services.AddHttpContextAccessor();
        // Register UserContextService as the concrete type first so both interfaces resolve to the same instance.
        services.AddScoped<UserContextService>();
        services.AddScoped<ICurrentUserService>(sp => sp.GetRequiredService<UserContextService>());
        // ITenantContextSetter allows Temporal activity interceptors to seed ambient TenantId
        // without a direct reference to ICurrentUserService.
        services.AddScoped<QFace.Sdk.Temporal.Interceptors.ITenantContextSetter>(
            sp => sp.GetRequiredService<UserContextService>());
        // ITenantScopeSetter allows Temporal activity interceptors to seed ITenantContext so that
        // EF global query filters receive the correct TenantId during background activity execution.
        services.TryAddScoped<ITenantContext, TenantContext>();
        services.AddScoped<QFace.Sdk.Temporal.Interceptors.ITenantScopeSetter>(
            sp => (QFace.Sdk.Temporal.Interceptors.ITenantScopeSetter)sp.GetRequiredService<ITenantContext>());

        services.TryAddScoped<ITenantActivityRecorder, TenantActivityRecorder>();

        return services;
    }


    public static void ApplyMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        ApplyDatabaseMigrations<TContext>(scope);
    }

    private static void ApplyDatabaseMigrations<TContext>(IServiceScope scope) where TContext : DbContext
    {
        TContext dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(TContext));
        try
        {
            // Ensure database exists and apply all migrations
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // 42P04 = database already exists (Postgres error code).
            // This is a benign race condition: on the very first container boot the DB
            // may not yet exist when Migrate() checks, but by the time CREATE DATABASE
            // executes another process has already created it.  The DB is usable — skip
            // the creation step and apply only pending migrations on the next restart.
            var isDbAlreadyExists = ex.Message.Contains("42P04")
                || (ex.InnerException?.Message?.Contains("42P04") ?? false);

            if (isDbAlreadyExists)
            {
                logger.LogInformation("Database already exists, skipping database creation");
                try
                {
                    var pending = dbContext.Database.GetPendingMigrations().ToList();
                    if (pending.Count > 0)
                    {
                        Console.WriteLine($"[Migration] Applying {pending.Count} pending migration(s)...");
                        dbContext.Database.Migrate();
                    }
                    else
                    {
                        Console.WriteLine("[Migration] No pending migrations.");
                    }
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"[Migration] Pending migration apply failed: {innerEx.Message}");
                    // Non-fatal on second attempt — let the app start; schema should already be current.
                }
                return;
            }

            Console.WriteLine($"Migration failed: {ex.Message}");
            throw;
        }
    }


    public static void ApplyMigrations<TDbContext>(this IServiceProvider serviceProvider) where TDbContext : DbContext
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        try
        {
            // Check if there are pending migrations
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                // Apply migrations with error handling
                dbContext.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            //  Log the error and try to create database from scratch
            Console.WriteLine($"Migration failed: {ex.Message}");
            Console.WriteLine("Attempting to recreate database...");

        }
    }

    public static async Task SeedDatabaseAsync<TContext>(
        this IApplicationBuilder app, Func<TContext, Task> seedFunc) where TContext : DbContext
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        TContext dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await seedFunc(dbContext);
    }

    /// <summary>
    /// Seeds lookup data on application startup
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="app">The application builder</param>
    /// <param name="seedFunc">The seeding function to execute</param>
    public static void SeedLookups<TContext>(
        this IApplicationBuilder app,
        Func<TContext, IServiceProvider, Task> seedFunc) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        try
        {
            logger.LogInformation("Starting lookup data seeding for {Context}...", typeof(TContext).Name);

            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            seedFunc(dbContext, scope.ServiceProvider).GetAwaiter().GetResult();

            logger.LogInformation("Lookup data seeding completed successfully for {Context}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding lookup data for {Context}", typeof(TContext).Name);
            // Don't throw - allow app to start even if seeding fails
        }
    }

    /// <summary>
    /// Registers the shared AppSettings services for a module
    /// </summary>
    /// <typeparam name="TContext">The module's ApplicationDbContext</typeparam>
    /// <typeparam name="TAppSettingsService">The module's AppSettingsService implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAppSettings<TContext, TAppSettingsService>(
        this IServiceCollection services)
        where TContext : ApplicationDbContext<TContext>
        where TAppSettingsService : class, IAppSettingsService
    {
        services.AddScoped<IAppSettingsService, TAppSettingsService>();
        return services;
    }

    /// <summary>
    /// Registers the shared Import services for a module
    /// </summary>
    /// <typeparam name="TContext">The module's ApplicationDbContext</typeparam>
    /// <typeparam name="TImportService">The module's ImportService implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddImports<TContext, TImportService>(
        this IServiceCollection services)
        where TContext : ApplicationDbContext<TContext>
        where TImportService : class, IImportService
    {
        services.AddScoped<IImportService, TImportService>();
        return services;
    }



    /// <summary>
    /// Registers OpenTelemetry logging, metrics, and tracing (including ASP.NET Core, HttpClient, EF Core, and MediatR <see cref="ObservabilityTelemetry.MediatRActivitySourceName"/>).
    /// OTLP export is enabled when <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is set.
    /// </summary>
    public static WebApplicationBuilder AddQimErpOpenTelemetryDefaults(this WebApplicationBuilder builder)
    {
        RegisterOpenTelemetryCore(
            builder.Services,
            builder.Logging,
            builder.Environment,
            builder.Configuration);
        return builder;
    }

    public static IHostApplicationBuilder AddServiceDefaults<TDbContext>(this IHostApplicationBuilder builder)
        where TDbContext : DbContext
    {
        RegisterOpenTelemetryCore(
            builder.Services,
            builder.Logging,
            builder.Environment,
            builder.Configuration);

        builder.AddDefaultHealthChecks<TDbContext>();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    private static void RegisterOpenTelemetryCore(
        IServiceCollection services,
        ILoggingBuilder logging,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        logging.AddOpenTelemetry(loggingOptions =>
        {
            loggingOptions.IncludeFormattedMessage = true;
            loggingOptions.IncludeScopes = true;
        });

        var otelBuilder = services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(environment.ApplicationName)
                    .AddSource(ObservabilityTelemetry.MediatRActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();
            });

        if (!string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            otelBuilder.UseOtlpExporter();
    }

    private static IHostApplicationBuilder AddDefaultHealthChecks<TDbContext>(this IHostApplicationBuilder builder)
        where TDbContext : DbContext
    {
        var hcBuilder = builder.Services.AddHealthChecks()
            .AddDbContextCheck<TDbContext>(
                "Database Health Check",
                failureStatus: HealthStatus.Degraded, // ✅ Instead of failing completely, mark it as 'Degraded'
                tags: ["ready"])
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

        // Redis readiness check — register the mux as singleton so AddQimErpRedisHealthCheck can resolve it
        var redisConn = builder.Configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            var connStr = ToStackExchangeConnectionString(redisConn);
            builder.Services.TryAddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
                _ => StackExchange.Redis.ConnectionMultiplexer.Connect(connStr));
        }
        hcBuilder.AddQimErpRedisHealthCheck();

        // Temporal readiness check
        var temporalAddress = builder.Configuration["Temporal:Address"];
        if (!string.IsNullOrWhiteSpace(temporalAddress))
        {
            var capturedAddress = temporalAddress;
            hcBuilder.AddCheck("temporal", () =>
            {
                try
                {
                    var parts = capturedAddress.Split(':');
                    var host = parts[0];
                    var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 7233;
                    using var tcp = new System.Net.Sockets.TcpClient();
                    var connected = tcp.ConnectAsync(host, port).Wait(TimeSpan.FromSeconds(3));
                    return connected
                        ? HealthCheckResult.Healthy()
                        : HealthCheckResult.Degraded("Temporal TCP connect timed out after 3 s");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Degraded(ex.Message);
                }
            }, tags: ["ready"]);
        }
        else
        {
            hcBuilder.AddCheck("temporal", () => HealthCheckResult.Healthy("Temporal not configured"), tags: ["ready"]);
        }

        return builder;
    }

    private static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Health check endpoints must be publicly accessible — they're hit by load balancers
        // and monitoring without credentials. AllowAnonymous overrides any FallbackPolicy.
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            Predicate = _ => true
        }).AllowAnonymous();

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
            AllowCachingResponses = false
        }).AllowAnonymous();

        app.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            AllowCachingResponses = false
        }).AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Adds a Redis readiness health check that reuses the <see cref="StackExchange.Redis.IConnectionMultiplexer"/>
    /// singleton already registered in DI (e.g. via <c>AddRedis</c>).
    /// If no multiplexer is registered the check reports Healthy with "Redis not configured" so modules
    /// without Redis start cleanly.
    /// </summary>
    // Marker type used by AddQimErpRedisHealthCheck to detect duplicate calls.
    private sealed class RedisHealthCheckRegisteredMarker { }

    public static IHealthChecksBuilder AddQimErpRedisHealthCheck(this IHealthChecksBuilder builder)
    {
        // Idempotent guard — AddCoreServices and AddDefaultHealthChecks both call this; the second
        // call must be a no-op to prevent "Duplicate health checks" crash at startup.
        // Health checks are stored in IConfigureOptions<HealthCheckServiceOptions>, not as direct
        // ServiceDescriptors, so we use a private marker singleton as the registration flag.
        if (builder.Services.Any(sd => sd.ServiceType == typeof(RedisHealthCheckRegisteredMarker)))
            return builder;
        builder.Services.AddSingleton<RedisHealthCheckRegisteredMarker>();

        var muxRegistered = builder.Services.Any(sd => sd.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
        if (!muxRegistered)
            return builder.AddCheck("redis", () => HealthCheckResult.Healthy("Redis not configured"), tags: ["ready"]);

        builder.Services.TryAddSingleton<QimErpRedisHealthCheck>();
        return builder.AddCheck<QimErpRedisHealthCheck>("redis", failureStatus: HealthStatus.Degraded, tags: ["ready"]);
    }

    /// <summary>
    /// Bootstraps a standard QimErp module WebApi host: logging, OTel, DbContext with outbox,
    /// core services (auth, CORS, MediatR, Carter, validation pipeline, health checks, Redis),
    /// Swagger, service discovery, and resilience — all from one call.
    /// Only add module-specific registrations (Temporal workers, repos, domain services) after this.
    /// </summary>
    public static WebApplicationBuilder AddQimErpModule<TContext>(
        this WebApplicationBuilder builder,
        string title,
        string version,
        string description)
        where TContext : ApplicationDbContext<TContext>
    {
        builder.AddQimErpOpenTelemetryDefaults();

        var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required.");
        builder.Services.AddDbContextWithOutbox<TContext>(connStr, builder.Configuration);

        builder.Services.AddCoreServices(builder.Configuration, typeof(TContext).Assembly);

        builder.Services.AddSwaggerDocumentation(
            System.Reflection.Assembly.GetCallingAssembly()!,
            title, version, description);

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        builder.Services.AddOpenApi();

        return builder;
    }
}

internal sealed class QimErpRedisHealthCheck : IHealthCheck
{
    private readonly StackExchange.Redis.IConnectionMultiplexer _mux;
    public QimErpRedisHealthCheck(StackExchange.Redis.IConnectionMultiplexer mux) => _mux = mux;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _mux.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(ex.Message);
        }
    }
}



