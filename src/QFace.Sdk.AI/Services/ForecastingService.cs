using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QFace.Sdk.AI.Algorithms;
using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Services;

/// <summary>
/// Service for calculating forecasts using various algorithms
/// </summary>
public class ForecastingService : IForecastingService
{
    private readonly Dictionary<string, IForecastAlgorithm> _algorithms;
    private readonly ILogger<ForecastingService> _logger;
    private readonly AIOptions _options;

    /// <summary>
    /// Initializes a new instance of ForecastingService
    /// </summary>
    public ForecastingService(
        IEnumerable<IForecastAlgorithm> algorithms,
        IOptions<AIOptions> options,
        ILogger<ForecastingService> logger)
    {
        _logger = logger;
        _options = options.Value;
        _algorithms = algorithms.ToDictionary(a => a.AlgorithmName, a => a);
        
        _logger.LogInformation("ForecastingService initialized with {Count} algorithms. Default: {DefaultMethod}",
            _algorithms.Count, _options.DefaultForecastMethod);
    }

    /// <inheritdoc />
    public async Task<ForecastResult> CalculateForecastAsync(ForecastRequest request, ForecastMethod? method = null, CancellationToken cancellationToken = default)
    {
        var forecastMethod = method ?? request.Method ?? _options.DefaultForecastMethod;
        var methodName = forecastMethod.ToString();
        
        if (!_algorithms.TryGetValue(methodName, out var algorithm))
        {
            var availableMethods = string.Join(", ", _algorithms.Keys);
            throw new ArgumentException(
                $"Forecast algorithm '{methodName}' not found. Available methods: {availableMethods}");
        }

        _logger.LogInformation("Calculating forecast using {Method} algorithm for target date {Date}",
            methodName, request.TargetDate);

        return await algorithm.CalculateForecastAsync(request, forecastMethod, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ForecastResult>> CalculateMultipleForecastsAsync(
        List<ForecastRequest> requests, 
        CancellationToken cancellationToken = default)
    {
        var results = new List<ForecastResult>();
        
        foreach (var request in requests)
        {
            var result = await CalculateForecastAsync(request, cancellationToken);
            results.Add(result);
        }

        return results;
    }
}

