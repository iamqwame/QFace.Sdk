using QimErp.Shared.Common.AppSettings.Contracts;
using QimErp.Shared.Common.AppSettings.Options;
using QimErp.Shared.Common.AppSettings.Mappings;

namespace QimErp.Shared.Common.AppSettings.Features;

public sealed class UpdateAppSettingCommand : IRequest<Result<AppSettingResponse>>
{
    public string SettingKey { get; set; } = string.Empty;
    public string? SettingValue { get; set; }
    public string? Description { get; set; }
}

public sealed class UpdateAppSettingCommandValidator : AbstractValidator<UpdateAppSettingCommand>
{
    public UpdateAppSettingCommandValidator()
    {
        RuleFor(x => x.SettingKey).NotEmpty();
        RuleFor(x => x.SettingValue).NotEmpty()
            .When(x => x.SettingValue != null)
            .WithMessage("Setting value cannot be empty");
    }
}

public sealed class UpdateAppSettingHandler<TResponse>(
    ILogger<UpdateAppSettingHandler<TResponse>> logger,
    IAppSettingsService appSettingsService,
    IStructuredSettingsMapper<TResponse> mapper,
    StructuredAppSettingsApiOptions<TResponse> options,
    IValidator<UpdateAppSettingCommand> validator)
    : IRequestHandler<UpdateAppSettingCommand, Result<AppSettingResponse>>
{
    public async Task<Result<AppSettingResponse>> Handle(
        UpdateAppSettingCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing UpdateAppSetting: {Request}", JsonSerializer.Serialize(request));

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.WithFailure<AppSettingResponse>(
                new Error("UpdateAppSetting.ValidationFailed", validationResult.ToString()));
        }

        try
        {
            var existing = await appSettingsService.GetSettingEntityAsync(request.SettingKey);
            if (existing is null)
            {
                return Result.WithNotFound<AppSettingResponse>(new Error("404", "Setting not found."));
            }

            if (!string.IsNullOrWhiteSpace(request.SettingValue))
            {
                var category = mapper.CategoryForKey(request.SettingKey);
                await appSettingsService.SetStringSettingAsync(
                    request.SettingKey,
                    request.SettingValue,
                    category,
                    request.Description ?? existing.Description ?? options.DefaultSettingDescription);
            }

            var updated = await appSettingsService.GetSettingEntityAsync(request.SettingKey);
            if (updated is null)
            {
                return Result.WithFailure<AppSettingResponse>(
                    new Error("UpdateAppSetting.UpdateFailed", "Failed to update setting."));
            }

            return Result.WithSuccess(updated.ToResponse());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error processing UpdateAppSetting: {Request}", JsonSerializer.Serialize(request));
            return Result.WithFailure<AppSettingResponse>(
                new Error("UpdateAppSetting.Error", "An error occurred."));
        }
    }
}
