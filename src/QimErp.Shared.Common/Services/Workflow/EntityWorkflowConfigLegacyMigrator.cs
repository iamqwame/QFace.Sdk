using System.Text.Json;
using System.Text.Json.Serialization;
using QimErp.Shared.Common.Workflow.Entities;

namespace QimErp.Shared.Common.Services.Workflow;

/// <summary>
/// Converts legacy single-code update/delete bindings into priority-ordered routes.
/// </summary>
public static class EntityWorkflowConfigLegacyMigrator
{
    public static EntityWorkflowConfig Migrate(EntityWorkflowConfig config, JsonElement? rawJson = null)
    {
        if (rawJson is not { ValueKind: JsonValueKind.Object } json)
            return config;

        if (config.UpdateWorkflowRoutes.Count == 0 &&
            TryGetString(json, "updateWorkflowCode", "UpdateWorkflowCode", out var updateCode) &&
            !string.IsNullOrWhiteSpace(updateCode))
        {
            config.UpdateWorkflowRoutes.Add(new WorkflowOperationRoute
            {
                WorkflowCode = updateCode,
                Name = "Legacy update route",
                Priority = 1,
                SignificantFields = ReadStringList(json, "significantFieldsForUpdate", "SignificantFieldsForUpdate"),
                Conditions = ReadConditions(json, "updateTriggerConditions", "UpdateTriggerConditions")
            });
        }

        if (config.DeleteWorkflowRoutes.Count == 0 &&
            TryGetString(json, "deleteWorkflowCode", "DeleteWorkflowCode", out var deleteCode) &&
            !string.IsNullOrWhiteSpace(deleteCode))
        {
            config.DeleteWorkflowRoutes.Add(new WorkflowOperationRoute
            {
                WorkflowCode = deleteCode,
                Name = "Legacy delete route",
                Priority = 1,
                Conditions = ReadConditions(json, "deleteTriggerConditions", "DeleteTriggerConditions")
            });
        }

        return config;
    }

    private static bool TryGetString(JsonElement json, string camelKey, string pascalKey, out string value)
    {
        value = "";
        if (json.TryGetProperty(camelKey, out var camel) && camel.ValueKind == JsonValueKind.String)
        {
            value = camel.GetString() ?? "";
            return true;
        }

        if (json.TryGetProperty(pascalKey, out var pascal) && pascal.ValueKind == JsonValueKind.String)
        {
            value = pascal.GetString() ?? "";
            return true;
        }

        return false;
    }

    private static List<string> ReadStringList(JsonElement json, string camelKey, string pascalKey)
    {
        if (json.TryGetProperty(camelKey, out var camel) && camel.ValueKind == JsonValueKind.Array)
            return camel.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList();

        if (json.TryGetProperty(pascalKey, out var pascal) && pascal.ValueKind == JsonValueKind.Array)
            return pascal.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => s.Length > 0).ToList();

        return [];
    }

    private static List<WorkflowTriggerCondition> ReadConditions(JsonElement json, string camelKey, string pascalKey)
    {
        if (json.TryGetProperty(camelKey, out var camel) && camel.ValueKind == JsonValueKind.Array)
            return DeserializeConditions(camel);

        if (json.TryGetProperty(pascalKey, out var pascal) && pascal.ValueKind == JsonValueKind.Array)
            return DeserializeConditions(pascal);

        return [];
    }

    private static List<WorkflowTriggerCondition> DeserializeConditions(JsonElement array) =>
        JsonSerializer.Deserialize<List<WorkflowTriggerCondition>>(
            array.GetRawText(),
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }) ?? [];
}
