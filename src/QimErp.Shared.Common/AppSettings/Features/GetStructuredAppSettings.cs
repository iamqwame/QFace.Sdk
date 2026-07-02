using QimErp.Shared.Common.AppSettings.Contracts;

namespace QimErp.Shared.Common.AppSettings.Features;

public sealed record GetStructuredAppSettingsQuery<TResponse> : IRequest<Result<TResponse>>;

public sealed class GetStructuredAppSettingsHandler<TResponse>(
    ILogger<GetStructuredAppSettingsHandler<TResponse>> logger,
    IAppSettingsService appSettingsService,
    IStructuredSettingsMapper<TResponse> mapper)
    : IRequestHandler<GetStructuredAppSettingsQuery<TResponse>, Result<TResponse>>
{
    public async Task<Result<TResponse>> Handle(
        GetStructuredAppSettingsQuery<TResponse> request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing GetStructuredAppSettings for {ResponseType}", typeof(TResponse).Name);

        try
        {
            var settings = await appSettingsService.GetAllSettingsAsync();
            var dict = settings
                .Where(s => mapper.IsStructuredSettingKey(s.Key))
                .GroupBy(s => s.Key)
                .ToDictionary(g => g.Key, g => g.Last().Value);

            var response = mapper.ToStructuredResponse(dict);
            logger.LogInformation("Successfully retrieved structured settings for {ResponseType}", typeof(TResponse).Name);
            return Result.WithSuccess(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error processing GetStructuredAppSettings for {ResponseType}", typeof(TResponse).Name);
            return Result.WithFailure<TResponse>(
                new Error("GetStructuredAppSettings.Error", "An error occurred."));
        }
    }
}
