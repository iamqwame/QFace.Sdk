using QimErp.Shared.Common.AppSettings.Contracts;
using QimErp.Shared.Common.AppSettings.Mappings;

namespace QimErp.Shared.Common.AppSettings.Features;

public sealed class GetAppSettingsPageCommand : PaginatedQueryBase, IRequest<Result<PaginatedList<AppSettingResponse>>>
{
    public string? SettingKey { get; set; }
}

public sealed class GetAppSettingsPageHandler<TResponse>(
    ILogger<GetAppSettingsPageHandler<TResponse>> logger,
    IAppSettingsService appSettingsService,
    IStructuredSettingsMapper<TResponse> mapper)
    : IRequestHandler<GetAppSettingsPageCommand, Result<PaginatedList<AppSettingResponse>>>
{
    public async Task<Result<PaginatedList<AppSettingResponse>>> Handle(
        GetAppSettingsPageCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing GetAppSettingsPage: {Request}", JsonSerializer.Serialize(request));

        try
        {
            var allSettings = await appSettingsService.GetAllSettingsAsync();
            var query = allSettings
                .Where(s => mapper.IsStructuredSettingKey(s.Key))
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(request.SettingKey))
            {
                query = query.Where(s => s.Key.Contains(request.SettingKey, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = query.OrderBy(s => s.Key).ToList();
            var totalCount = ordered.Count;
            var items = ordered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => s.ToResponse())
                .ToList();

            var response = new PaginatedList<AppSettingResponse>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);

            logger.LogInformation("Successfully retrieved {Count} app settings", items.Count);
            return Result.WithSuccess(response);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error processing GetAppSettingsPage: {Request}", JsonSerializer.Serialize(request));
            return Result.WithFailure<PaginatedList<AppSettingResponse>>(
                new Error("GetAppSettingsPage.Error", "An error occurred."));
        }
    }
}
