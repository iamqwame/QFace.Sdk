namespace QimErp.Shared.Common.Knowledge;

public static class KnowledgeCollectionKeys
{
    public const string HrResources = "hr-resources";
    public const string PlatformSales = "platform-sales";
}

public static class KnowledgeCache
{
    private const string Prefix = "qface:qimerp:knowledge:";

    public static string AskRateLimit(string tenantId, string userId) => $"{Prefix}ask:{tenantId}:{userId}";

    public const int AskRateLimitWindowSeconds = 60;
    public const int AskRateLimitMaxPerWindow = 20;
}
