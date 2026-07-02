using QimErp.Shared.Common.AppSettings.Contracts;

namespace QimErp.Shared.Common.AppSettings.Mappings;

public static class AppSettingMappings
{
    public static AppSettingResponse ToResponse(this AppSetting setting) =>
        new()
        {
            Id = setting.Id,
            SettingKey = setting.Key,
            SettingValue = setting.Value,
            Description = setting.Description,
            CreatedDate = setting.Created,
        };
}
