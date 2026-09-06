using QimErp.Shared.Common.AppSettings.Contracts;
using QimErp.Shared.Common.AppSettings.Options;
using QimErp.Shared.Common.AppSettings.Mappings;

namespace QimErp.Shared.Common.AppSettings.Features;

public sealed class CreateAppSettingCommand : IRequest<Result<AppSettingResponse>>
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class CreateAppSettingCommandValidator : AbstractValidator<CreateAppSettingCommand>
{
    public CreateAppSettingCommandValidator()
    {
        RuleFor(x => x.SettingKey).NotEmpty();
        RuleFor(x => x.SettingValue).NotEmpty();
    }
}

public sealed class CreateAppSettingHandler<TResponse>(
    ILogger<CreateAppSettingHandler<TResponse>> logger,
    IAppSettingsService appSettingsService,
    IStructuredSettingsMapper<TResponse> mapper,
    StructuredAppSettingsApiOptions<TResponse> options,
    IValidator<CreateAppSettingCommand> validator)
    : IRequestHandler<CreateAppSettingCommand, Result<AppSettingResponse>>
{
    public async Task<Result<AppSettingResponse>> Handle(
        CreateAppSettingCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing CreateAppSetting: {Request}", JsonSerializer.Serialize(request));

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.WithFailure<AppSettingResponse>(
                new Error("CreateAppSetting.ValidationFailed", validationResult.ToString()));
        }

        try
        {
            if (await appSettingsService.SettingExistsForCurrentCompanyAsync(request.SettingKey))
            {
                return Result.WithFailure<AppSettingResponse>(
                    new Error("CreateAppSetting.AlreadyExists", "Setting with this key already exists."));
            }

            var category = mapper.CategoryForKey(request.SettingKey);
            await appSettingsService.SetStringSettingAsync(
                request.SettingKey,
                request.SettingValue,
                category,
                request.Description ?? options.DefaultSettingDescription);

            var created = await appSettingsService.GetSettingEntityAsync(request.SettingKey);
            if (created is null)
            {
                return Result.WithFailure<AppSettingResponse>(
                    new Error("CreateAppSetting.SaveFailed", "Failed to save setting."));
            }

            return Result.WithSuccess(created.ToResponse());
        }
        catch (AppSettingScopeViolationException ex)
        {
            logger.LogDebug("CreateAppSetting rejected: {Message}", ex.Message);
            return Result.WithFailure<AppSettingResponse>(new Error("CreateAppSetting.ScopeViolation", ex.Message));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error processing CreateAppSetting: {Request}", JsonSerializer.Serialize(request));
            return Result.WithFailure<AppSettingResponse>(
                new Error("CreateAppSetting.Error", "An error occurred."));
        }
    }
}
