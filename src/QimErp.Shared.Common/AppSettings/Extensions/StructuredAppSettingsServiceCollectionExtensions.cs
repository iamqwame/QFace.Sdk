using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using QimErp.Shared.Common.AppSettings.Contracts;
using QimErp.Shared.Common.AppSettings.Features;
using QimErp.Shared.Common.AppSettings.Options;

namespace QimErp.Shared.Common.AppSettings.Extensions;

public interface IStructuredAppSettingsApiDescriptor
{
    void MapRoutes(IEndpointRouteBuilder app);
}

internal sealed class StructuredAppSettingsApiDescriptor<TResponse> : IStructuredAppSettingsApiDescriptor
{
    private readonly StructuredAppSettingsApiOptions<TResponse> _options;

    public StructuredAppSettingsApiDescriptor(StructuredAppSettingsApiOptions<TResponse> options) =>
        _options = options;

    public void MapRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(_options.StructuredGetRoute,
                [Authorize] async (ISender sender) =>
                {
                    var result = await sender.Send(new GetStructuredAppSettingsQuery<TResponse>());
                    return result.ToIResult();
                })
            .WithTags(_options.ApiTag)
            .WithName(_options.GetStructuredOperationName)
            .WithSummary($"Get structured {_options.ModuleName} settings")
            .WithDescription($"Gets all {_options.ModuleName} settings organised into a structured response.");

        app.MapPatch(_options.BulkPatchRoute,
                [Authorize] async ([FromBody] TResponse body, ISender sender) =>
                {
                    var result = await sender.Send(new UpsertStructuredAppSettingsBulkCommand<TResponse>(body));
                    return result.ToIResult();
                })
            .WithTags(_options.ApiTag)
            .WithName(_options.UpsertBulkOperationName)
            .WithSummary($"Upsert bulk {_options.ModuleName} settings")
            .WithDescription($"Persists all {_options.ModuleName} settings from a structured request body.");

        if (_options.EnablePageEndpoint)
        {
            app.MapPost(_options.ResolvedPageRoute,
                    [Authorize] async ([FromBody] GetAppSettingsPageCommand command, ISender sender) =>
                    {
                        var result = await sender.Send(command);
                        return result.ToIResult();
                    })
                .WithTags(_options.ApiTag)
                .WithName(_options.GetPageOperationName)
                .WithSummary($"Get {_options.ModuleName} settings page")
                .WithDescription($"Gets a paginated list of {_options.ModuleName} settings.");
        }

        if (_options.EnableCrudEndpoints)
        {
            app.MapPost(_options.ResolvedCreateRoute,
                    [Authorize] async ([FromBody] CreateAppSettingCommand command, ISender sender) =>
                    {
                        var result = await sender.Send(command);
                        return result.ToIResult();
                    })
                .WithTags(_options.ApiTag)
                .WithName(_options.CreateOperationName)
                .WithSummary($"Create {_options.ModuleName} setting")
                .WithDescription($"Creates a new {_options.ModuleName} setting.");

            app.MapPut(_options.ResolvedUpdateRouteTemplate,
                    [Authorize] async (
                        string settingKey,
                        [FromBody] UpdateAppSettingCommand command,
                        ISender sender) =>
                    {
                        command.SettingKey = settingKey;
                        var result = await sender.Send(command);
                        return result.ToIResult();
                    })
                .WithTags(_options.ApiTag)
                .WithName(_options.UpdateOperationName)
                .WithSummary($"Update {_options.ModuleName} setting")
                .WithDescription($"Updates an existing {_options.ModuleName} setting.");

            app.MapDelete(_options.ResolvedDeleteRouteTemplate,
                    [Authorize] async (Guid id, ISender sender) =>
                    {
                        var result = await sender.Send(new DeleteAppSettingCommand { Id = id });
                        return result.ToIResult();
                    })
                .WithTags(_options.ApiTag)
                .WithName(_options.DeleteOperationName)
                .WithSummary($"Delete {_options.ModuleName} setting")
                .WithDescription($"Deletes a {_options.ModuleName} setting.");
        }
    }
}

public static class StructuredAppSettingsServiceCollectionExtensions
{
    public static IServiceCollection AddStructuredAppSettingsApi<TResponse>(
        this IServiceCollection services,
        Action<StructuredAppSettingsApiOptions<TResponse>> configure)
        where TResponse : class
    {
        var options = new StructuredAppSettingsApiOptions<TResponse>
        {
            ModuleName = typeof(TResponse).Name,
        };
        configure(options);

        if (string.IsNullOrWhiteSpace(options.RoutePrefix))
            throw new InvalidOperationException($"{nameof(StructuredAppSettingsApiOptions<TResponse>.RoutePrefix)} is required.");
        if (string.IsNullOrWhiteSpace(options.ApiTag))
            throw new InvalidOperationException($"{nameof(StructuredAppSettingsApiOptions<TResponse>.ApiTag)} is required.");
        if (string.IsNullOrWhiteSpace(options.ModuleName))
            throw new InvalidOperationException($"{nameof(StructuredAppSettingsApiOptions<TResponse>.ModuleName)} is required.");

        services.AddSingleton(options);
        services.AddSingleton<IStructuredAppSettingsApiDescriptor, StructuredAppSettingsApiDescriptor<TResponse>>();

        services.AddTransient<
            IRequestHandler<GetStructuredAppSettingsQuery<TResponse>, Result<TResponse>>,
            GetStructuredAppSettingsHandler<TResponse>>();
        services.AddTransient<
            IRequestHandler<UpsertStructuredAppSettingsBulkCommand<TResponse>, Result<TResponse>>,
            UpsertStructuredAppSettingsBulkHandler<TResponse>>();

        if (options.EnablePageEndpoint)
        {
            services.AddTransient<
                IRequestHandler<GetAppSettingsPageCommand, Result<PaginatedList<AppSettingResponse>>>,
                GetAppSettingsPageHandler<TResponse>>();
        }

        if (options.EnableCrudEndpoints)
        {
            services.AddValidatorsFromAssemblyContaining<CreateAppSettingCommandValidator>();
            services.AddTransient<
                IRequestHandler<CreateAppSettingCommand, Result<AppSettingResponse>>,
                CreateAppSettingHandler<TResponse>>();
            services.AddTransient<
                IRequestHandler<UpdateAppSettingCommand, Result<AppSettingResponse>>,
                UpdateAppSettingHandler<TResponse>>();
            services.AddTransient<
                IRequestHandler<DeleteAppSettingCommand, Result<bool>>,
                DeleteAppSettingHandler<TResponse>>();
        }

        return services;
    }

    public static IEndpointRouteBuilder MapRegisteredStructuredAppSettingsApis(this IEndpointRouteBuilder app)
    {
        var descriptors = app.ServiceProvider.GetServices<IStructuredAppSettingsApiDescriptor>();
        foreach (var descriptor in descriptors)
            descriptor.MapRoutes(app);

        return app;
    }
}
