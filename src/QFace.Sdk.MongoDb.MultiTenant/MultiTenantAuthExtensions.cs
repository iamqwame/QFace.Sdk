namespace QFace.Sdk.MongoDb.MultiTenant;

// </summary>
    public static class MultiTenantAuthExtensions
    {
        /// <summary>
        /// Adds tenant authentication services
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>Authentication builder for chaining</returns>
        public static AuthenticationBuilder AddTenantAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure JWT options
            var jwtSection = configuration.GetSection("Jwt");
            var jwtOptions = new JwtOptions();
            jwtSection.Bind(jwtOptions);
            services.Configure<JwtOptions>(jwtSection);
            services.AddSingleton(jwtOptions);
            
            // Register token service
            services.AddSingleton<ITokenService, JwtTokenService>();
            
            // Register password hasher
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            
            // Configure JWT authentication
            return services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tenantAccessor = context.HttpContext.RequestServices.GetRequiredService<ITenantAccessor>();
                        var tenantId = context.Principal.FindFirstValue("tenant_id");
                        
                        if (!string.IsNullOrEmpty(tenantId))
                        {
                            // Set current tenant context from the token
                            tenantAccessor.SetCurrentTenantId(tenantId);
                        }
                    }
                };
            });
        }
        
        /// <summary>
        /// Adds tenant authorization policies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddTenantAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Role-based policies
                options.AddPolicy("SystemAdmin", policy => 
                    policy.RequireRole("SystemAdmin"));
                
                options.AddPolicy("TenantAdmin", policy => 
                    policy.RequireRole("SystemAdmin", "TenantAdmin"));
                
                options.AddPolicy("TenantUser", policy => 
                    policy.RequireRole("SystemAdmin", "TenantAdmin", "User"));
                
                // Permission-based policies
                options.AddPolicy("ManageUsers", policy =>
                    policy.RequireClaim("permission", "manage_users"));
                
                options.AddPolicy("AccessReports", policy =>
                    policy.RequireClaim("permission", "access_reports"));
                
                options.AddPolicy("ManageTenants", policy =>
                    policy.RequireClaim("permission", "manage_tenants"));
            });
            
            return services;
        }
    }