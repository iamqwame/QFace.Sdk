namespace QimErp.Shared.Common.Entities;

public enum AppSettingDataType
{
    String,
    Array,
    Object,
    Boolean,
    Number,
    Decimal
}

public class AppSettingValidationRules
{
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; } // Regex pattern
    public bool? Required { get; set; }
    public string[]? AllowedValues { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MinItems { get; set; } // For arrays
    public int? MaxItems { get; set; } // For arrays
    public string? CustomValidation { get; set; } // Custom validation logic
    public Dictionary<string, string>? AdditionalRules { get; set; } // For extensibility

    public static AppSettingValidationRules Create()
    {
        return new AppSettingValidationRules
        {
            MinLength = null,
            MaxLength = null,
            Pattern = null,
            Required = null,
            AllowedValues = null,
            MinValue = null,
            MaxValue = null,
            MinItems = null,
            MaxItems = null,
            CustomValidation = null,
            AdditionalRules = null
        };
    }

    public AppSettingValidationRules WithMinLength(int minLength)
    {
        MinLength = minLength;
        return this;
    }

    public AppSettingValidationRules WithMaxLength(int maxLength)
    {
        MaxLength = maxLength;
        return this;
    }

    public AppSettingValidationRules WithPattern(string pattern)
    {
        Pattern = pattern;
        return this;
    }

    public AppSettingValidationRules WithRequired(bool required = true)
    {
        Required = required;
        return this;
    }

    public AppSettingValidationRules WithAllowedValues(params string[] values)
    {
        AllowedValues = values;
        return this;
    }

    public AppSettingValidationRules WithMinValue(decimal minValue)
    {
        MinValue = minValue;
        return this;
    }

    public AppSettingValidationRules WithMaxValue(decimal maxValue)
    {
        MaxValue = maxValue;
        return this;
    }

    public AppSettingValidationRules WithMinItems(int minItems)
    {
        MinItems = minItems;
        return this;
    }

    public AppSettingValidationRules WithMaxItems(int maxItems)
    {
        MaxItems = maxItems;
        return this;
    }

    public AppSettingValidationRules WithCustomValidation(string customValidation)
    {
        CustomValidation = customValidation;
        return this;
    }

    public AppSettingValidationRules WithAdditionalRule(string key, string value)
    {
        AdditionalRules ??= new Dictionary<string, string>();
        AdditionalRules[key] = value;
        return this;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public static AppSettingValidationRules? FromJson(string? json)
    {
        if (json.IsEmpty())
            return null;

        try
        {
            return json.Deserialize<AppSettingValidationRules>();
        }
        catch
        {
            return null;
        }
    }

    public bool ValidateValue(object? value)
    {
        if (value == null)
            return Required != true;

        var stringValue = value.ToString();

        // Check required
        if (Required == true && stringValue.IsEmpty())
            return false;

        // Check min/max length
        if (MinLength.HasValue && stringValue?.Length < MinLength.Value)
            return false;

        if (MaxLength.HasValue && stringValue?.Length > MaxLength.Value)
            return false;

        // Check pattern
        if (!Pattern.IsEmpty() && !stringValue.IsEmpty())
        {
            try
            {
                var regex = new Regex(Pattern);
                if (!regex.IsMatch(stringValue))
                    return false;
            }
            catch
            {
                // Invalid regex pattern
                return false;
            }
        }

        // Check allowed values
        if (AllowedValues != null && AllowedValues.Length > 0)
        {
            if (!AllowedValues.Contains(stringValue))
                return false;
        }

        // Check numeric ranges
        if (MinValue.HasValue || MaxValue.HasValue)
        {
            if (decimal.TryParse(stringValue, out var numericValue))
            {
                if (MinValue.HasValue && numericValue < MinValue.Value)
                    return false;
                if (MaxValue.HasValue && numericValue > MaxValue.Value)
                    return false;
            }
            else
            {
                return false; // Not a valid number
            }
        }

        // Check array constraints
        if (MinItems.HasValue || MaxItems.HasValue)
        {
            if (value is Array array)
            {
                if (MinItems.HasValue && array.Length < MinItems.Value)
                    return false;
                if (MaxItems.HasValue && array.Length > MaxItems.Value)
                    return false;
            }
            else if (value is System.Collections.ICollection collection)
            {
                if (MinItems.HasValue && collection.Count < MinItems.Value)
                    return false;
                if (MaxItems.HasValue && collection.Count > MaxItems.Value)
                    return false;
            }
        }

        return true;
    }
} 
