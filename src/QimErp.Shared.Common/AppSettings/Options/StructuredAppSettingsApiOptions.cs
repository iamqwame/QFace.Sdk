namespace QimErp.Shared.Common.AppSettings.Options;

public sealed class StructuredAppSettingsApiOptions<TResponse>
{
    public string RoutePrefix { get; set; } = string.Empty;
    public string ApiTag { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;

    public string GetStructuredOperationName { get; set; } = "GetStructuredSettings";
    public string UpsertBulkOperationName { get; set; } = "UpsertStructuredSettingsBulk";
    public string GetPageOperationName { get; set; } = "GetAppSettingsPage";
    public string CreateOperationName { get; set; } = "CreateAppSetting";
    public string UpdateOperationName { get; set; } = "UpdateAppSetting";
    public string DeleteOperationName { get; set; } = "DeleteAppSetting";

    public string BulkUpdateDescription { get; set; } = "Structured settings";
    public string DefaultSettingDescription { get; set; } = "Application setting";

    public bool EnablePageEndpoint { get; set; }
    public bool EnableCrudEndpoints { get; set; }

    public string? PageRoute { get; set; }
    public string? CreateRoute { get; set; }
    public string? UpdateRouteTemplate { get; set; }
    public string? DeleteRouteTemplate { get; set; }

    public string StructuredGetRoute => $"{RoutePrefix}/structured";
    public string BulkPatchRoute => $"{RoutePrefix}/bulk";
    public string ResolvedPageRoute => PageRoute ?? $"{RoutePrefix}/page";
    public string ResolvedCreateRoute => CreateRoute ?? RoutePrefix;
    public string ResolvedUpdateRouteTemplate => UpdateRouteTemplate ?? $"{RoutePrefix}/{{settingKey}}";
    public string ResolvedDeleteRouteTemplate => DeleteRouteTemplate ?? $"{RoutePrefix}/{{id:guid}}";
}
