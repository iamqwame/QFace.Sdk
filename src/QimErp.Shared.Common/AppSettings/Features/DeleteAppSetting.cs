namespace QimErp.Shared.Common.AppSettings.Features;

public sealed class DeleteAppSettingCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

public sealed class DeleteAppSettingCommandValidator : AbstractValidator<DeleteAppSettingCommand>
{
    public DeleteAppSettingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class DeleteAppSettingHandler<TResponse>(
    ILogger<DeleteAppSettingHandler<TResponse>> logger,
    IAppSettingsService appSettingsService,
    IValidator<DeleteAppSettingCommand> validator)
    : IRequestHandler<DeleteAppSettingCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        DeleteAppSettingCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing DeleteAppSetting: {Request}", JsonSerializer.Serialize(request));

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.WithFailure<bool>(
                new Error("DeleteAppSetting.ValidationFailed", validationResult.ToString()));
        }

        try
        {
            var allSettings = await appSettingsService.GetAllSettingsAsync();
            var setting = allSettings.FirstOrDefault(s => s.Id == request.Id);
            if (setting is null)
            {
                return Result.WithNotFound<bool>(new Error("404", "Setting not found."));
            }

            await appSettingsService.DeleteSettingAsync(setting.Key);
            return Result.WithSuccess(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error processing DeleteAppSetting: {Request}", JsonSerializer.Serialize(request));
            return Result.WithFailure<bool>(
                new Error("DeleteAppSetting.Error", "An error occurred."));
        }
    }
}
