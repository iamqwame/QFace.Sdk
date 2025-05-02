namespace QFace.Sdk.MongoDb.MultiTenant.Attributes
{
    /// <summary>
    /// Marks a document class to be included in tenant collection creation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TenantCollectionAttribute : Attribute
    {
        /// <summary>
        /// Optional custom collection name
        /// </summary>
        public string? CollectionName { get; }

        /// <summary>
        /// Creates a new tenant collection attribute
        /// </summary>
        public TenantCollectionAttribute()
        {
        }

        /// <summary>
        /// Creates a new tenant collection attribute with custom collection name
        /// </summary>
        /// <param name="collectionName">Custom collection name</param>
        public TenantCollectionAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}