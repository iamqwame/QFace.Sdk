using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace QFace.Sdk.RedisCache.Services.Providers;

/// <summary>
/// Newtonsoft settings for cache round-trips. Domain entities use private setters and
/// non-public constructors; with default settings those properties silently deserialize
/// to their defaults ("" / 0 / false) on every cache hit while collections survive
/// (populated in place through their getters). These settings make the round-trip faithful.
/// </summary>
internal static class RedisJsonSettings
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new PrivateSetterContractResolver(),
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
    };

    private sealed class PrivateSetterContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (!property.Writable && member is PropertyInfo propertyInfo)
            {
                property.Writable = propertyInfo.GetSetMethod(nonPublic: true) != null;
            }
            return property;
        }
    }
}
