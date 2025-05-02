using Microsoft.OpenApi.Models;
using QFace.Sdk.MongoDb.MultiTenant;
using Demo.MongoDb.MultiTenant.Api;
using QFace.Sdk.Logging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Increase logging level to see more details
builder.Host.AddQFaceLogging();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Multi-Tenant API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

// Add core services
builder.Services.AddSingleton<ITenantAccessor, TenantAccessor>();

var assemblies = new[] { typeof(Program).Assembly };
// Add Multi-Tenant MongoDB Support without automatic scanning
builder.Services.AddMongoDbMultiTenancy(
    builder.Configuration,
    tenantManagementSectionName: "MongoDbManagement",
    tenantDataSectionName: "DefaultTenantDbData",
    assembliesToScan: assemblies
);


// Add Authentication
builder.Services.AddTenantAuthentication(builder.Configuration);

// Add Authorization
builder.Services.AddTenantAuthorization();

// Register controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add tenant resolution middleware
app.UseTenantResolution(options =>
{
    options.UseHeaderResolution = true;
    options.UseRouteResolution = true;
    options.UseQueryStringResolution = true;
    options.UseAuthClaimResolution = true;
    options.TenantExemptPaths.Add("/api/tenants");
    options.TenantExemptPaths.Add("/api/auth");
    options.ExcludedPaths.Add("/swagger");
});

app.UseAuthentication();
app.UseAuthorization();

// Add a simple index page
app.MapGet("/", () => Results.Content(GetIndexHtml(), "text/html"));
app.MapControllers();

//Initialize sample data with improved error handling
using (var scope = app.Services.CreateScope())
{
    try 
    {
        await SetupHelper.InitializeSampleDataAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing sample data");
    }
}

app.Run();

string GetIndexHtml()
{
    return @"
<!DOCTYPE html>
<html>
<head>
    <title>Multi-Tenant Sample</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; padding: 20px; max-width: 800px; margin: 0 auto; }
        h1 { color: #333; }
        h2 { color: #555; margin-top: 30px; }
        pre { background-color: #f4f4f4; padding: 10px; border-radius: 5px; overflow-x: auto; }
        .endpoint { margin-bottom: 20px; }
        .method { font-weight: bold; display: inline-block; width: 70px; }
        .url { color: #0066cc; }
        .description { margin-top: 5px; }
    </style>
</head>
<body>
    <h1>Multi-Tenant Sample API</h1>
    <p>This is a sample application demonstrating the QFace.Sdk.MongoDb.MultiTenant functionality.</p>
    
    <h2>Sample Data</h2>
    <p>The application has been initialized with the following sample data:</p>
    <ul>
        <li><strong>System Admin:</strong> Email: admin@example.com, Password: Admin123!</li>
        <li><strong>Tenant 1:</strong> Acme Corporation (Code: acme)
            <ul>
                <li>Admin: Email: admin@acme.com, Password: Acme123!</li>
                <li>5 sample products</li>
            </ul>
        </li>
        <li><strong>Tenant 2:</strong> GlobalTech (Code: globaltech)
            <ul>
                <li>Admin: Email: admin@globaltech.com, Password: Global123!</li>
                <li>5 sample products</li>
            </ul>
        </li>
    </ul>
    
    <h2>API Documentation</h2>
    <p>You can explore the full API documentation at <a href='/swagger'>/swagger</a></p>
    
    <h2>Key Endpoints</h2>
    
    <div class='endpoint'>
        <div><span class='method'>POST</span> <span class='url'>/api/auth/login</span></div>
        <div class='description'>Log in to a tenant with email/password</div>
        <pre>{
  ""email"": ""admin@acme.com"",
  ""password"": ""Acme123!"",
  ""tenantCode"": ""acme""
}</pre>
    </div>
    
    <div class='endpoint'>
        <div><span class='method'>GET</span> <span class='url'>/api/products</span></div>
        <div class='description'>Get all products for the current tenant (requires authentication)</div>
    </div>
    
    <div class='endpoint'>
        <div><span class='method'>POST</span> <span class='url'>/api/tenants</span></div>
        <div class='description'>Create a new tenant</div>
    </div>
    
    <div class='endpoint'>
        <div><span class='method'>GET</span> <span class='url'>/api/tenant-users</span></div>
        <div class='description'>Get all users for the current tenant (requires admin authentication)</div>
    </div>
    
    <h2>Testing with cURL</h2>
    <p>Here's an example of how to authenticate and access tenant-specific data:</p>
    <pre>
# 1. Log in to get an access token
curl -X POST ""http://localhost:5000/api/auth/login"" \\
     -H ""Content-Type: application/json"" \\
     -d '{ ""email"": ""admin@acme.com"", ""password"": ""Acme123!"", ""tenantCode"": ""acme"" }'

# 2. Use the access token to get products
curl -X GET ""http://localhost:5000/api/products"" \\
     -H ""Authorization: Bearer YOUR_ACCESS_TOKEN""
</pre>
</body>
</html>";
}