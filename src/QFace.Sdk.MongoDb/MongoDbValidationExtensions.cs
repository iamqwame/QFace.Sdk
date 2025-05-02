namespace QFace.Sdk.MongoDb;

/// <summary>
/// Extension methods for validating MongoDB repository registration
/// </summary>
public static class MongoDbValidationExtensions
{
    /// <summary>
    /// Validates that all MongoDB repositories used in the application are properly registered.
    /// This should be called during service registration, before building the application.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ValidateMongoRepositories(this IServiceCollection services)
    {
        // Get all methods from the entry assembly that might be endpoints
        var entryAssembly = Assembly.GetEntryAssembly();
        var methods = new List<MethodInfo>();
        
        // Scan the assembly containing the Program class (where the app is built)
        if (entryAssembly != null)
        {
            methods.AddRange(entryAssembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                .Where(m => m.GetParameters().Any(p => 
                    p.ParameterType.IsGenericType && 
                    p.ParameterType.GetGenericTypeDefinition() == typeof(IMongoRepository<>))));
        }
        
        // Also scan controllers for API endpoints
        methods.AddRange(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.Name.EndsWith("Controller"))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(m => m.GetParameters().Any(p => 
                p.ParameterType.IsGenericType && 
                p.ParameterType.GetGenericTypeDefinition() == typeof(IMongoRepository<>))));
        
        // Check each method for IMongoRepository<T> parameters
        var missingRepositories = new Dictionary<Type, Dictionary<Type, List<MethodInfo>>>();
        
        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                if (paramType.IsGenericType && 
                    paramType.GetGenericTypeDefinition() == typeof(IMongoRepository<>))
                {
                    // Get document type from repository type
                    var documentType = paramType.GetGenericArguments()[0];
                    
                    // Check if the repository is registered
                    var isRegistered = services.Any(sd => sd.ServiceType == paramType);
                    
                    // If not registered, add to missing repositories
                    if (!isRegistered)
                    {
                        if (!missingRepositories.ContainsKey(paramType))
                        {
                            missingRepositories[paramType] = new Dictionary<Type, List<MethodInfo>>();
                        }
                        
                        if (!missingRepositories[paramType].ContainsKey(documentType))
                        {
                            missingRepositories[paramType][documentType] = new List<MethodInfo>();
                        }
                        
                        missingRepositories[paramType][documentType].Add(method);
                    }
                }
            }
        }
        
        // If missing repositories found, throw an exception
        if (missingRepositories.Count > 0)
        {
            var errorMessage = new System.Text.StringBuilder("Missing MongoDB repository registrations detected:\n\n");
            
            errorMessage.AppendLine("To fix this, you have two options:\n");
            
            // Option 1: Register specific repositories
            errorMessage.AppendLine("OPTION 1: Register specific repositories:");
            foreach (var entry in missingRepositories)
            {
                foreach (var docEntry in entry.Value)
                {
                    var documentType = docEntry.Key;
                    errorMessage.AppendLine($"  services.AddMongoRepository<{documentType.Name}>();");
                }
            }
            
            // Option 2: Use assembly scanning
            errorMessage.AppendLine("\nOPTION 2: Use assembly scanning (recommended):");
            errorMessage.AppendLine("  builder.Services.AddMongoDb(");
            errorMessage.AppendLine("      builder.Configuration,");
            errorMessage.AppendLine("      assembliesToScan: new[] { Assembly.GetExecutingAssembly() }");
            errorMessage.AppendLine("  );");
            
            errorMessage.AppendLine("\nDetailed missing repository information:");
            foreach (var entry in missingRepositories)
            {
                foreach (var docEntry in entry.Value)
                {
                    var repoType = entry.Key;
                    var documentType = docEntry.Key;
                    var usedInMethods = docEntry.Value;
                    
                    errorMessage.AppendLine($"* {repoType.Name} for {documentType.Name} is not registered but is used in:");
                    foreach (var method in usedInMethods)
                    {
                        var methodName = method.Name;
                        // Clean up lambda method names for better readability
                        if (methodName.Contains("<") && methodName.Contains(">"))
                        {
                            methodName = methodName.Substring(methodName.IndexOf("<") + 1, 
                                methodName.IndexOf(">") - methodName.IndexOf("<") - 1);
                        }
                        
                        errorMessage.AppendLine($"  - {method.DeclaringType?.Name}.{methodName}");
                    }
                }
            }
            
            throw new InvalidOperationException(errorMessage.ToString());
        }
        
        return services;
    }
}