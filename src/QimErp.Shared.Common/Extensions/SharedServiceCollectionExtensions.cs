using System.Text.Json.Serialization;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using QFace.Sdk.RedisCache.Extensions;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Middlewares;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.Cache;
using QimErp.Shared.Common.Services.MultiTenancy;
using QimErp.Shared.Common.Services.Workflow;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace QimErp.Shared.Common.Extensions;

public static class SharedServiceCollectionExtensions
{

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

        return services;
    }
    public static IServiceCollection AddDbContextWithOutbox<TContext>(
           this IServiceCollection services, string connectionString) where TContext : ApplicationDbContext<TContext>
    {
        services.AddScoped<ITenantContext, TenantContext>();
        
        services
            .AddScoped<AuditEntitySaveChangesInterceptor>()
            .AddDbContext<TContext>((provider, options) =>
        {
            NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
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
    
    public static IServiceCollection AddDbContextWithOutboxConsumer<TContext>(
        this IServiceCollection services, string connectionString) where TContext : ApplicationDbContext<TContext>
    {
        services.AddSingleton<ITenantContext, TenantContext>();
        services.AddSingleton<ConsumerUserContextService>();
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<ConsumerUserContextService>());

        services
            .AddScoped<AuditEntitySaveChangesInterceptor>()
            .AddDbContext<TContext>((provider, options) =>
            {
                NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
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
        app.UseRequestLogging(configuration);
        app.UseResponseLogging(configuration);
        
        app.UseAntiforgery();
        
        app.UseCors("AllowAll");
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseTenantContext();
        app.UseAuthorization();

        app.MapDefaultEndpoints();
    }
    /// <summary>
    /// Configures core services including Authentication, CORS, MediatR, Carter, and JSON serialization settings.
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly, Assembly? workFlowAssembly = null)
    {

        var isAddWorkflow = workFlowAssembly!=null;

        var carterCatalog =isAddWorkflow? new DependencyContextAssemblyCatalog(
            workFlowAssembly,
            assembly
        ) : new DependencyContextAssemblyCatalog(assembly);


        services.AddMemoryCache(); // For configuration caching
        
        // Add antiforgery services (required by UseAntiforgery in UseAppSecurity)
        services.AddAntiforgery();
        
        // Register ITenantContext (required by UseTenantContext middleware in UseAppSecurity)
        // Use TryAddScoped to avoid duplicate registration if already registered by AddDbContextWithOutbox
        services.TryAddScoped<ITenantContext, TenantContext>();
        
        // Register SDK Redis Cache services (reads from "RedisCache" configuration section)
        services.AddRedisCache(configuration);
        
        // Register cache services (adapter that uses SDK)
        services.AddScoped<IDistributedCacheService, RedisCacheService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddAuth(configuration); // ✅ Ensure Authentication is Added Early
        services.AddCorsConfig(configuration);
        services.RegisterMediatR(isAddWorkflow ? [assembly,workFlowAssembly]:[assembly]);
        services.AddCarter(carterCatalog); // ✅ Carter will automatically scan for modules
        services.AddValidatorsFromAssembly(assembly);
        // Add health checks
        services.AddHealthChecks()
            .AddCheck("liveness", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck("readiness", () => HealthCheckResult.Healthy(), tags: ["ready"]);

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
        services.AddScoped<IWorkflowApprovalProcessor, WorkflowApprovalProcessor>();


        return services;
    }
    
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);
        JwtSettings jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings not found.");

        // ✅ Configure Authentication (JWT Bearer)
        services
            .AddAuthorization()
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
        services.AddScoped<ICurrentUserService, UserContextService>();

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
        try
        {
            // Ensure database exists and apply all migrations
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log the error
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


    
    public static IHostApplicationBuilder AddServiceDefaults<TDbContext>(this IHostApplicationBuilder builder)
        where TDbContext : DbContext
    {
        builder.ConfigureOpenTelemetry();

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

    private static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        bool useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    private static IHostApplicationBuilder AddDefaultHealthChecks<TDbContext>(this IHostApplicationBuilder builder)
        where TDbContext : DbContext
    {
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<TDbContext>(
                "Database Health Check",
                failureStatus: HealthStatus.Degraded, // ✅ Instead of failing completely, mark it as 'Degraded'
                tags: ["ready"])
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);


        return builder;
    }

    private static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Add health check endpoints
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            AllowCachingResponses = false, // Always return fresh health status
            Predicate = _ => true // Include all health checks
        });

        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"), // Liveness checks
            AllowCachingResponses = false
        });

        app.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"), // Readiness checks
            AllowCachingResponses = false
        });

        return app;
    }

}





