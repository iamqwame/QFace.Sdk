using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;

namespace QFace.Sdk.AI.Algorithms;

/// <summary>
/// ML.NET-based forecasting algorithm using time series analysis
/// </summary>
public class MLForecastAlgorithm : IForecastAlgorithm
{
    private readonly ILogger<MLForecastAlgorithm> _logger;
    private readonly AIOptions _options;
    private MLContext? _mlContext;

    /// <summary>
    /// Algorithm name
    /// </summary>
    public string AlgorithmName => "ML";

    /// <summary>
    /// Initializes a new instance of MLForecastAlgorithm
    /// </summary>
    public MLForecastAlgorithm(IOptions<AIOptions> options, ILogger<MLForecastAlgorithm> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ForecastResult> CalculateForecastAsync(ForecastRequest request, ForecastMethod method, CancellationToken cancellationToken = default)
    {
        if (!_options.EnableMLForecasting)
        {
            throw new InvalidOperationException("ML forecasting is not enabled. Set EnableMLForecasting to true in AIOptions.");
        }

        if (request.HistoricalData == null || request.HistoricalData.Count < 3)
        {
            throw new ArgumentException("At least 3 historical data points are required for ML forecasting");
        }

        _mlContext ??= new MLContext(seed: 0);

        try
        {
            var sortedData = request.HistoricalData.OrderBy(d => d.Date).ToList();
            
            // Convert to ML.NET format
            var mlData = sortedData.Select((d, index) => new TimeSeriesDataPoint
            {
                Time = index,
                Value = (float)d.Value
            }).ToList();

            // Create data view
            var dataView = _mlContext.Data.LoadFromEnumerable(mlData);

            // Configure forecasting pipeline
            var forecastingPipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "Forecast",
                inputColumnName: "Value",
                windowSize: Math.Min(mlData.Count / 2, 10),
                seriesLength: mlData.Count,
                trainSize: mlData.Count,
                horizon: 1);

            // Train the model
            var forecaster = forecastingPipeline.Fit(dataView);

            // Create forecasting engine
            var forecastingEngine = forecaster.CreateTimeSeriesEngine<TimeSeriesDataPoint, ForecastOutput>(_mlContext);

            // Get the last data point
            var lastPoint = mlData.Last();

            // Forecast
            var forecast = forecastingEngine.Predict();

            var forecastedValue = (decimal)forecast.Forecast[0];
            var confidenceLevel = request.ConfidenceLevel ?? 85;

            var result = new ForecastResult
            {
                ForecastedValue = forecastedValue,
                TargetDate = request.TargetDate,
                Method = method,
                ConfidenceLevel = confidenceLevel,
                Metadata = new Dictionary<string, object>
                {
                    { "WindowSize", Math.Min(mlData.Count / 2, 10) },
                    { "SeriesLength", mlData.Count }
                },
                Warnings = []
            };

            if (mlData.Count < 10)
            {
                result.Warnings.Add("Limited data for ML forecasting. Consider using Regression or Trend methods.");
            }

            _logger.LogInformation("ML forecast calculated: {Value} for {Date} with {Confidence}% confidence",
                forecastedValue, request.TargetDate, confidenceLevel);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ML forecast");
            throw;
        }
    }

    /// <summary>
    /// Data point for ML.NET time series
    /// </summary>
    private class TimeSeriesDataPoint
    {
        public int Time { get; set; }
        public float Value { get; set; }
    }

    /// <summary>
    /// Forecast output from ML.NET
    /// </summary>
    private class ForecastOutput
    {
        [VectorType(1)]
        public float[] Forecast { get; set; } = [];
    }
}

