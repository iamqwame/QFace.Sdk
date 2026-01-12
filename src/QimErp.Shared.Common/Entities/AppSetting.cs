namespace QimErp.Shared.Common.Entities;

public class AppSetting : GuidAuditableEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string? CategoryDescription { get; private set; }
    public AppSettingDataType DataType { get; private set; }
    public AppSettingValidationRules ValidationRules { get; private set; }

    private AppSetting()
    {
        ValidationRules = AppSettingValidationRules.Create();
    }

    public static AppSetting Create(string key, string? value, string category)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty", nameof(category));

        var setting = new AppSetting
        {
            Id = CreateId(),
            Key = key,
            Value = value ?? string.Empty,
            Category = category,
            DataType = AppSettingDataType.String,
            ValidationRules = AppSettingValidationRules.Create()
        };
       
        setting.AsActive();
        return setting;
    }

    public static AppSetting CreateArray(string key, string[]? values, string category)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty", nameof(category));

        var setting = new AppSetting
        {
            Id = CreateId(),
            Key = key,
            Value = JsonSerializer.Serialize(values ?? []),
            Category = category,
            DataType = AppSettingDataType.Array,
            ValidationRules = AppSettingValidationRules.Create()
        };
        setting.AsActive();
        return setting;
    }

    public static AppSetting CreateObject(string key, object? value, string category)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));
        
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty", nameof(category));

        var setting = new AppSetting
        {
            Id = CreateId(),
            Key = key,
            Value = JsonSerializer.Serialize(value ?? new object()),
            Category = category,
            DataType = AppSettingDataType.Object,
            ValidationRules = AppSettingValidationRules.Create()
        };
        setting.AsActive();
        return setting;
    }

    public void UpdateValue(string value)
    {
        Value = value ?? string.Empty;
    }

    public void UpdateArrayValue(string[] values)
    {
        Value = JsonSerializer.Serialize(values ?? []);
        DataType = AppSettingDataType.Array;
    }

    public void UpdateObjectValue(object value)
    {
        Value = JsonSerializer.Serialize(value ?? new object());
        DataType = AppSettingDataType.Object;
    }

    // Fluent API methods
    public AppSetting WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public AppSetting WithCategoryDescription(string categoryDescription)
    {
        CategoryDescription = categoryDescription;
        return this;
    }

    public AppSetting WithValidationRules(AppSettingValidationRules validationRules)
    {
        ValidationRules = validationRules ?? AppSettingValidationRules.Create();
        return this;
    }

    public T? GetValue<T>()
    {
                    if (Value.IsEmpty())
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(Value);
        }
        catch
        {
            return default;
        }
    }

    public string[]? GetArrayValue()
    {
        return GetValue<string[]>();
    }

    public Dictionary<string, object>? GetObjectValue()
    {
        return GetValue<Dictionary<string, object>>();
    }

    public bool GetBooleanValue()
    {
        return bool.TryParse(Value, out var result) && result;
    }

    public int GetIntValue()
    {
        return int.TryParse(Value, out var result) ? result : 0;
    }

    public decimal GetDecimalValue()
    {
        return decimal.TryParse(Value, out var result) ? result : 0;
    }
} 
