namespace QFace.Sdk.AI.Extensions;

/// <summary>
/// Extension methods for registering AI services
/// </summary>
public static class AIExtensions
{
    /// <summary>
    /// Adds AI services (forecasting, skills analysis, LLM) to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="sectionName">The configuration section name (default: "AI")</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAIServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "AI")
    {
        // Register options
        services.Configure<AIOptions>(configuration.GetSection(sectionName));
        
        // Register HttpClient for Google Gemini provider
        services.AddSingleton<HttpClient>();
        
        // Register LLM providers
        services.AddSingleton<ILLMProvider, OpenAIProvider>();
        services.AddSingleton<ILLMProvider, AnthropicProvider>();
        services.AddSingleton<ILLMProvider, GoogleGeminiProvider>();
        services.AddSingleton<LLMProviderFactory>();
        
        // Register forecasting algorithms
        services.AddSingleton<IForecastAlgorithm, TrendAnalysisAlgorithm>();
        services.AddSingleton<IForecastAlgorithm, RegressionAnalysisAlgorithm>();
        services.AddSingleton<IForecastAlgorithm, ManualForecastAlgorithm>();
        
        // Conditionally register ML algorithm
        var aiOptions = configuration.GetSection(sectionName).Get<AIOptions>();
        if (aiOptions?.EnableMLForecasting == true)
        {
            services.AddSingleton<IForecastAlgorithm, MLForecastAlgorithm>();
        }
        
        // Register core services
        services.AddScoped<IForecastingService, ForecastingService>();
        services.AddScoped<ISkillsAnalysisService, SkillsAnalysisService>();
        services.AddScoped<ILLMService, LLMService>();
        
        return services;
    }
}

