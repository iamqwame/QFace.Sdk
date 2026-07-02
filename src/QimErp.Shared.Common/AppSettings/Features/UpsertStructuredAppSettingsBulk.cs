using QimErp.Shared.Common.AppSettings.Contracts;
using QimErp.Shared.Common.AppSettings.Options;

namespace QimErp.Shared.Common.AppSettings.Features;

public sealed record UpsertStructuredAppSettingsBulkCommand<TResponse>(TResponse Settings)
    : IRequest<Result<TResponse>>;

public sealed class UpsertStructuredAppSettingsBulkHandler<TResponse>(
    ILogger<UpsertStructuredAppSettingsBulkHandler<TResponse>> logger,
    IAppSettingsService appSettingsService,
    IStructuredSettingsMapper<TResponse> mapper,
    StructuredAppSettingsApiOptions<TResponse> options)
    : IRequestHandler<UpsertStructuredAppSettingsBulkCommand<TResponse>, Result<TResponse>>
{
    public async Task<Result<TResponse>> Handle(
        UpsertStructuredAppSettingsBulkCommand<TResponse> request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing UpsertStructuredAppSettingsBulk for {ResponseType}", typeof(TResponse).Name);

        try
        {
            var settingsByCategory = mapper.ToSettingsDictionary(request.Settings)
                .GroupBy(kvp => mapper.CategoryForKey(kvp.Key));

            foreach (var group in settingsByCategory)
            {
                await appSettingsService.BulkSetSettingsAsync(
                    group.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    group.Key,
                    options.BulkUpdateDescription);
            }

            var updated = await appSettingsService.GetAllSettingsAsync();
            var dict = updated
                .Where(s => mapper.IsStructuredSettingKey(s.Key))
                .GroupBy(s => s.Key)
                .ToDictionary(g => g.Key, g => g.Last().Value);

            var response = mapper.ToStructuredResponse(dict);
            logger.LogInformation("Successfully upserted bulk structured settings for {ResponseType}", typeof(TResponse).Name);
            return Result.WithSuccess(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error processing UpsertStructuredAppSettingsBulk for {ResponseType}", typeof(TResponse).Name);
            return Result.WithFailure<TResponse>(
                new Error("UpsertStructuredAppSettingsBulk.Error", "An error occurred."));
        }
    }
}
