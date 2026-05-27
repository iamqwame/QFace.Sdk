using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Captures the scalar fields that changed when a workflow-enabled entity is saved.
/// Keys are camelCase property names; values are display-safe strings. No fixed schema.
/// </summary>
public static class WorkflowEntityChangeSnapshot
{
    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "WorkflowStatus",
        "CurrentWorkflowHistoryId",
        "CurrentWorkflowInstanceId",
        "WorkflowCode",
        "WorkflowComments",
        "WorkflowInitiatedAt",
        "WorkflowInitiatedByEmail",
        "WorkflowInitiatedByEmployeeId",
        "WorkflowInitiatedByName",
        "WorkflowCompletedAt",
        "WorkflowCompletedByEmail",
        "WorkflowCompletedByEmployeeId",
        "WorkflowCompletedByName",
        "WorkflowRejectionReason",
        "WorkflowAutoApproved",
        "TenantId",
        "ReferenceNumber",
        "Created",
        "LastModified",
        "CreatedByUserId",
        "CreatedByEmail",
        "CreatedByName",
        "LastModifiedByUserId",
        "LastModifiedByEmail",
        "LastModifiedByName",
        "PreviousDataStatus",
        "ImageUrl",
        "ProfilePicture",
        "BannerImage",
        "SendInvitationOnActivation",
    };

    public static Dictionary<string, object> Capture(EntityEntry entry)
    {
        var snapshot = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in entry.Properties)
        {
            if (ShouldSkip(property))
                continue;

            var include = entry.State == EntityState.Added
                ? property.CurrentValue is not null
                : property.IsModified && !ValuesEqual(property.OriginalValue, property.CurrentValue);

            if (!include)
                continue;

            if (TryNormalize(property.CurrentValue, out var value))
            {
                snapshot[ToSnapshotKey(property.Metadata.Name)] = value;
                continue;
            }

            var nested = ReadNestedDisplay(property.CurrentValue);
            if (!string.IsNullOrWhiteSpace(nested))
                snapshot[ToSnapshotKey(property.Metadata.Name)] = nested;
        }

        return snapshot;
    }

    public static string? GetDisplayName(object entity)
    {
        foreach (var name in new[] { "FullName", "DisplayName", "Name", "Title", "Subject", "Label", "Code" })
        {
            var value = entity.GetType().GetProperty(name)?.GetValue(entity)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        var first = entity.GetType().GetProperty("FirstName")?.GetValue(entity)?.ToString();
        var last = entity.GetType().GetProperty("LastName")?.GetValue(entity)?.ToString();
        var combined = string.Join(" ", new[] { first, last }.Where(v => !string.IsNullOrWhiteSpace(v)));
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    private static bool ShouldSkip(PropertyEntry property)
    {
        if (ExcludedProperties.Contains(property.Metadata.Name))
            return true;

        if (property.Metadata.IsPrimaryKey())
            return true;

        if (property.Metadata.IsForeignKey())
            return true;

        return property.Metadata.ClrType != typeof(string)
               && typeof(System.Collections.IEnumerable).IsAssignableFrom(property.Metadata.ClrType);
    }

    private static bool ValuesEqual(object? left, object? right) =>
        left is null && right is null
        || left is not null && left.Equals(right);

    private static bool TryNormalize(object? value, out object normalized)
    {
        normalized = null!;
        if (value is null)
            return false;

        switch (value)
        {
            case string s when !string.IsNullOrWhiteSpace(s):
                normalized = s.Trim();
                return true;
            case Enum e:
                normalized = e.ToString();
                return true;
            case DateTime dt:
                normalized = dt.ToString("yyyy-MM-dd");
                return true;
            case DateOnly d:
                normalized = d.ToString("yyyy-MM-dd");
                return true;
            case bool b:
                normalized = b;
                return true;
            case int or long or decimal or double or float:
                normalized = value;
                return true;
            case Guid g when g != Guid.Empty:
                normalized = g.ToString();
                return true;
            default:
                return false;
        }
    }

    private static string? ReadNestedDisplay(object? value)
    {
        if (value is null or string)
            return null;

        foreach (var name in new[] { "Name", "FullName", "Title", "Label", "Code", "Email" })
        {
            var nested = value.GetType().GetProperty(name)?.GetValue(value)?.ToString();
            if (!string.IsNullOrWhiteSpace(nested))
                return nested.Trim();
        }

        return null;
    }

    private static string ToSnapshotKey(string propertyName)
    {
        var key = propertyName.Replace("#", "_", StringComparison.Ordinal);
        if (!key.Contains('_'))
            return ToCamelCase(key);

        var parts = key.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return ToCamelCase(key);

        var combined = parts[0] + string.Concat(parts.Skip(1).Select(Part =>
            Part.Length == 0 ? "" : char.ToUpperInvariant(Part[0]) + Part[1..]));
        return ToCamelCase(combined);
    }

    private static string ToCamelCase(string name) =>
        string.IsNullOrEmpty(name) || char.IsLower(name[0])
            ? name
            : char.ToLowerInvariant(name[0]) + name[1..];
}
