// using FluentAssertions;
// using Microsoft.Extensions.DependencyInjection;
// using MongoDB.Driver;
// using QFace.Sdk.MongoDb.Services;
// using QFace.Sdk.MongoDb.Tests.Models;
// using Xunit;
//
// namespace QFace.Sdk.MongoDb.Tests;
//
// /// <summary>
// /// Tests for multi-tenant MongoDB functionality
// /// </summary>
// public class TenantDatabaseTests : MongoDbIntegrationTest
// {
//     private readonly string _connectionString;
//     
//     public TenantDatabaseTests()
//     {
//         _connectionString = ContainerManager.GetConnectionString();
//     }
//     
//     /// <summary>
//     /// Override to register tenant services
//     /// </summary>
//     protected override void ConfigureServices(IServiceCollection services)
//     {
//         // Register the test document repository
//         RegisterRepository<TestDocument>(services);
//         
//         // Register tenant database resolver
//         services.AddMongoDbConnectionResolver((provider, tenantId) => 
//         {
//             // In a real application, this would look up tenant connection info from configuration or database
//             // For testing, we use the same connection with different database names
//             return (_connectionString, $"{DatabaseName}-{tenantId}");
//         });
//         
//         // Register MongoDB provider
//         services.AddMongoDbProvider();
//     }
//     
//     /// <summary>
//     /// Tests that the tenant database resolver can create separate databases for tenants
//     /// </summary>
//     [Fact]
//     public void TenantDatabaseResolver_ShouldProvideDistinctDatabases()
//     {
//         // Arrange
//         var resolver = GetService<ITenantDatabaseResolver>();
//         
//         // Act
//         var tenant1Db = resolver.GetDatabase("tenant1");
//         var tenant2Db = resolver.GetDatabase("tenant2");
//         
//         // Assert
//         defaultDb.Should().NotBeNull();
//         tenant1Db.Should().NotBeNull();
//         tenant2Db.Should().NotBeNull();
//         
//         // Verify database names
//         defaultDb.DatabaseNamespace.DatabaseName.Should().Be(DatabaseName);
//         tenant1Db.DatabaseNamespace.DatabaseName.Should().Be($"{DatabaseName}-tenant1");
//         tenant2Db.DatabaseNamespace.DatabaseName.Should().Be($"{DatabaseName}-tenant2");
//     }
//     
//     /// <summary>
//     /// Tests that the MongoDB provider can work with multiple databases
//     /// </summary>
//     [Fact]
//     public void MongoDbProvider_ShouldProvideDistinctDatabases()
//     {
//         // Arrange
//         var provider = GetService<IMongoDbProvider>();
//         
//         // Act
//         var defaultDb = provider.GetDatabase();
//         var namedDb = provider.GetDatabase("named-db");
//         var tenant1Db = provider.GetTenantDatabase("tenant1");
//         
//         // Assert
//         defaultDb.Should().NotBeNull();
//         namedDb.Should().NotBeNull();
//         tenant1Db.Should().NotBeNull();
//         
//         // Verify database names
//         defaultDb.DatabaseNamespace.DatabaseName.Should().Be(DatabaseName);
//         namedDb.DatabaseNamespace.DatabaseName.Should().Be("named-db");
//         tenant1Db.DatabaseNamespace.DatabaseName.Should().Be($"{DatabaseName}-tenant1");
//     }
//     
//     /// <summary>
//     /// Tests that documents inserted into tenant databases are properly isolated
//     /// </summary>
//     [Fact]
//     public async Task TenantCollections_ShouldIsolateDocumentsByTenant()
//     {
//         // Arrange
//         var provider = GetService<IMongoDbProvider>();
//         var collectionName = "test_documents";
//         
//         // Get collections for different tenants
//         var tenant1Collection = provider.GetTenantCollection<TestDocument>("tenant1", collectionName);
//         var tenant2Collection = provider.GetTenantCollection<TestDocument>("tenant2", collectionName);
//         
//         // Create test documents
//         var tenant1Doc = new TestDocument
//         {
//             Name = "Tenant 1 Document",
//             Description = "Document in tenant 1 database",
//             TenantId = "tenant1"
//         };
//         
//         var tenant2Doc = new TestDocument
//         {
//             Name = "Tenant 2 Document",
//             Description = "Document in tenant 2 database",
//             TenantId = "tenant2"
//         };
//         
//         // Act
//         await tenant1Collection.InsertOneAsync(tenant1Doc);
//         await tenant2Collection.InsertOneAsync(tenant2Doc);
//         
//         // Query each tenant's collection
//         var tenant1Docs = await tenant1Collection.Find(FilterDefinition<TestDocument>.Empty).ToListAsync();
//         var tenant2Docs = await tenant2Collection.Find(FilterDefinition<TestDocument>.Empty).ToListAsync();
//         
//         // Assert
//         tenant1Docs.Should().HaveCount(1);
//         tenant1Docs[0].Name.Should().Be("Tenant 1 Document");
//         
//         tenant2Docs.Should().HaveCount(1);
//         tenant2Docs[0].Name.Should().Be("Tenant 2 Document");
//     }
// }