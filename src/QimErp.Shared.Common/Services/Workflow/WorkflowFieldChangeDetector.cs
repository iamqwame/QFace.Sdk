using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Detects which business property names changed on a workflow-enabled entity.
/// </summary>
public static class WorkflowFieldChangeDetector
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
        "DataStatus",
    };

    public static HashSet<string> DetectChangedFields(EntityEntry entry)
    {
        var changed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in entry.Properties)
        {
            if (ShouldSkip(property))
                continue;

            if (entry.State == EntityState.Added)
            {
                if (property.CurrentValue is not null && IsScalarOrOwnedLeaf(property))
                    changed.Add(property.Metadata.Name);
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                changed.Add(property.Metadata.Name);
                continue;
            }

            if (property.IsModified && !ValuesEqual(property.OriginalValue, property.CurrentValue))
                changed.Add(property.Metadata.Name);
        }

        foreach (var reference in entry.References.Where(r => r.TargetEntry != null))
        {
            var target = reference.TargetEntry!;
            if (target.Metadata.IsOwned())
            {
                var nestedChanged = DetectChangedFields(target);
                if (nestedChanged.Count > 0)
                    changed.Add(reference.Metadata.Name);
            }
        }

        return changed;
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
               && typeof(System.Collections.IEnumerable).IsAssignableFrom(property.Metadata.ClrType)
               && property.Metadata.ClrType != typeof(byte[]);
    }

    private static bool IsScalarOrOwnedLeaf(PropertyEntry property) =>
        property.Metadata.ClrType.IsPrimitive
        || property.Metadata.ClrType == typeof(string)
        || property.Metadata.ClrType == typeof(decimal)
        || property.Metadata.ClrType == typeof(DateTime)
        || property.Metadata.ClrType == typeof(DateTimeOffset)
        || property.Metadata.ClrType == typeof(DateOnly)
        || property.Metadata.ClrType == typeof(TimeOnly)
        || property.Metadata.ClrType == typeof(Guid)
        || property.Metadata.ClrType.IsEnum;

    private static bool ValuesEqual(object? left, object? right) =>
        left is null && right is null
        || left is not null && left.Equals(right);
}
