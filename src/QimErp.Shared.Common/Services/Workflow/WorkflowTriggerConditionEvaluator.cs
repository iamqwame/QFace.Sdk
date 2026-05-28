namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Entity-agnostic evaluation of workflow trigger conditions via reflection.
/// </summary>
public static class WorkflowTriggerConditionEvaluator
{
    public static bool EvaluateAll(IWorkflowEnabled entity, IEnumerable<WorkflowTriggerCondition> conditions)
    {
        foreach (var condition in conditions)
        {
            if (!Evaluate(entity, condition))
                return false;
        }

        return true;
    }

    public static bool Evaluate(IWorkflowEnabled entity, WorkflowTriggerCondition condition)
    {
        var property = entity.GetType().GetProperty(condition.Field);
        if (property == null)
            return false;

        var actualValue = property.GetValue(entity);
        return EvaluateValue(actualValue, condition.Operator, condition.Value);
    }

    public static bool EvaluateValue(object? actualValue, WorkflowOperators @operator, object expectedValue)
    {
        return @operator switch
        {
            WorkflowOperators.Equals => Equals(actualValue, expectedValue?.ToString()) ||
                                        Equals(actualValue?.ToString(), expectedValue?.ToString()),
            WorkflowOperators.NotEquals => !Equals(actualValue, expectedValue?.ToString()) &&
                                         !Equals(actualValue?.ToString(), expectedValue?.ToString()),
            WorkflowOperators.GreaterThan => CompareValues(actualValue, expectedValue) > 0,
            WorkflowOperators.LessThan => CompareValues(actualValue, expectedValue) < 0,
            WorkflowOperators.GreaterThanOrEqual => CompareValues(actualValue, expectedValue) >= 0,
            WorkflowOperators.LessThanOrEqual => CompareValues(actualValue, expectedValue) <= 0,
            WorkflowOperators.Contains => actualValue?.ToString()?.Contains(expectedValue?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) == true,
            WorkflowOperators.StartsWith => actualValue?.ToString()?.StartsWith(expectedValue?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) == true,
            WorkflowOperators.EndsWith => actualValue?.ToString()?.EndsWith(expectedValue?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) == true,
            _ => false
        };
    }

    private static int CompareValues(object? actualValue, object expectedValue)
    {
        if (actualValue is IComparable comparable && TryConvertComparable(expectedValue, actualValue.GetType(), out var converted))
            return comparable.CompareTo(converted);

        if (decimal.TryParse(actualValue?.ToString(), out var actualDecimal) &&
            decimal.TryParse(expectedValue?.ToString(), out var expectedDecimal))
            return actualDecimal.CompareTo(expectedDecimal);

        return 0;
    }

    private static bool TryConvertComparable(object expectedValue, Type targetType, out object? converted)
    {
        converted = null;
        try
        {
            if (targetType.IsEnum && expectedValue is string enumString)
            {
                converted = Enum.Parse(targetType, enumString, ignoreCase: true);
                return true;
            }

            converted = Convert.ChangeType(expectedValue, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
