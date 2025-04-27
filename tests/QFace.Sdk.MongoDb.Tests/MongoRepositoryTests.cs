// using FluentAssertions;
// using Microsoft.Extensions.DependencyInjection;
// using MongoDB.Driver;
// using QFace.Sdk.MongoDb.Tests.Models;
// using Xunit;
//
// namespace QFace.Sdk.MongoDb.Tests;
//
// /// <summary>
// /// Tests for the MongoDB repository implementation
// /// </summary>
// public class MongoRepositoryTests : MongoDbIntegrationTest
// {
//     /// <summary>
//     /// Override to register required services
//     /// </summary>
//     protected override void ConfigureServices(IServiceCollection services)
//     {
//         // Register the test document repository
//         RegisterRepository<TestDocument>(services);
//     }
//
//     /// <summary>
//     /// Tests that the repository can insert a document
//     /// </summary>
//     [Fact]
//     public async Task InsertOneAsync_ShouldInsertDocument()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//         var document = CreateTestDocument();
//
//         // Act
//         await repository.InsertOneAsync(document);
//
//         // Assert
//         var result = await repository.GetByIdAsync(document.Id);
//         result.Should().NotBeNull();
//         result.Id.Should().Be(document.Id);
//         result.Name.Should().Be(document.Name);
//         result.Description.Should().Be(document.Description);
//         result.Score.Should().Be(document.Score);
//         result.Tags.Should().BeEquivalentTo(document.Tags);
//         result.Metadata.Version.Should().Be(document.Metadata.Version);
//         result.Metadata.Properties.Should().BeEquivalentTo(document.Metadata.Properties);
//         result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
//         result.IsActive.Should().BeTrue();
//     }
//
//     /// <summary>
//     /// Tests that the repository can update a document
//     /// </summary>
//     [Fact]
//     public async Task UpdateAsync_ShouldUpdateDocument()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//         var document = CreateTestDocument();
//         await repository.InsertOneAsync(document);
//
//         // Update the document
//         document.Name = "Updated Name";
//         document.Description = "Updated Description";
//         document.Score = 100;
//         document.Tags.Add("updated");
//         document.Metadata.Version = "2.0";
//         document.Metadata.Properties["updated"] = "true";
//
//         // Act
//         var result = await repository.UpdateAsync(document);
//
//         // Assert
//         result.Should().BeTrue();
//
//         // Verify the update
//         var updated = await repository.GetByIdAsync(document.Id);
//         updated.Should().NotBeNull();
//         updated.Name.Should().Be("Updated Name");
//         updated.Description.Should().Be("Updated Description");
//         updated.Score.Should().Be(100);
//         updated.Tags.Should().Contain("updated");
//         updated.Metadata.Version.Should().Be("2.0");
//         updated.Metadata.Properties["updated"].Should().Be("true");
//         updated.LastModifiedDate.Should().BeAfter(updated.CreatedDate);
//     }
//
//     /// <summary>
//     /// Tests that the repository can find documents by a filter expression
//     /// </summary>
//     [Fact]
//     public async Task FindAsync_ShouldReturnMatchingDocuments()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//
//         // Create multiple documents
//         var document1 = CreateTestDocument("Document 1", "First document", 10);
//         var document2 = CreateTestDocument("Document 2", "Second document", 20);
//         var document3 = CreateTestDocument("Document 3", "Third document", 30);
//
//         await repository.InsertManyAsync(new[] { document1, document2, document3 });
//
//         // Act
//         var result = await repository.FindAsync(d => d.Score > 15);
//
//         // Assert
//         result.Should().HaveCount(2);
//         result.Select(d => d.Name).Should().Contain(new[] { "Document 2", "Document 3" });
//     }
//
//     /// <summary>
//     /// Tests that the repository can page results
//     /// </summary>
//     [Fact]
//     public async Task FindWithPagingAsync_ShouldReturnPagedResults()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//
//         // Create multiple documents
//         var documents = Enumerable.Range(1, 25)
//             .Select(i => CreateTestDocument($"Document {i}", $"Description {i}", i))
//             .ToList();
//
//         await repository.InsertManyAsync(documents);
//
//         // Act - Get page 2 with 10 items per page, sorted by score
//         var result = await repository.FindWithPagingAsync(
//             null,
//             d => d.Score,
//             sortDescending: true,
//             page: 2,
//             pageSize: 10);
//
//         // Assert
//         result.Items.Should().HaveCount(10);
//         result.TotalCount.Should().Be(25);
//
//         // Verify we got documents 11-20 sorted by score descending
//         var scores = result.Items.Select(d => d.Score).ToList();
//         scores.Should().BeInDescendingOrder();
//         scores.Max().Should().Be(15); // The 11th highest score (25 - 10)
//         scores.Min().Should().Be(6); // The 20th highest score (25 - 19)
//     }
//
//     /// <summary>
//     /// Tests that the repository can soft delete a document
//     /// </summary>
//     [Fact]
//     public async Task SoftDeleteByIdAsync_ShouldMakeDocumentInactive()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//         var document = CreateTestDocument();
//         await repository.InsertOneAsync(document);
//
//         // Act
//         var result = await repository.SoftDeleteByIdAsync(document.Id);
//
//         // Assert
//         result.Should().BeTrue();
//
//         // Verify that the document is inactive
//         var deleted = await repository.FindOneAsync(d => d.Id == document.Id, includeInactive: true);
//         deleted.Should().NotBeNull();
//         deleted.IsActive.Should().BeFalse();
//
//         // Verify that the document is not returned when not including inactive
//         var notFound = await repository.FindOneAsync(d => d.Id == document.Id);
//         notFound.Should().BeNull();
//     }
//
//     /// <summary>
//     /// Tests that the repository can restore a soft-deleted document
//     /// </summary>
//     [Fact]
//     public async Task RestoreByIdAsync_ShouldMakeDocumentActive()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//         var document = CreateTestDocument();
//         await repository.InsertOneAsync(document);
//         await repository.SoftDeleteByIdAsync(document.Id);
//
//         // Act
//         var result = await repository.RestoreByIdAsync(document.Id);
//
//         // Assert
//         result.Should().BeTrue();
//
//         // Verify that the document is active
//         var restored = await repository.FindOneAsync(d => d.Id == document.Id);
//         restored.Should().NotBeNull();
//         restored.IsActive.Should().BeTrue();
//     }
//
//     /// <summary>
//     /// Tests that the repository can hard delete a document
//     /// </summary>
//     [Fact]
//     public async Task DeleteByIdAsync_ShouldRemoveDocument()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//         var document = CreateTestDocument();
//         await repository.InsertOneAsync(document);
//
//         // Act
//         var result = await repository.DeleteByIdAsync(document.Id);
//
//         // Assert
//         result.Should().BeTrue();
//
//         // Verify that the document is deleted
//         var deleted = await repository.FindOneAsync(d => d.Id == document.Id, includeInactive: true);
//         deleted.Should().BeNull();
//
//         // Verify count
//         var count = await repository.CountAsync();
//         count.Should().Be(0);
//     }
//
//     /// <summary>
//     /// Tests that the repository can filter by tenant ID
//     /// </summary>
//     [Fact]
//     public async Task GetAllAsync_WithTenantId_ShouldFilterByTenant()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//
//         // Create documents with different tenant IDs
//         var document1 = CreateTestDocument("Tenant A Doc", "Document for tenant A", 10);
//         document1.TenantId = "tenant-a";
//
//         var document2 = CreateTestDocument("Tenant B Doc", "Document for tenant B", 20);
//         document2.TenantId = "tenant-b";
//
//         var document3 = CreateTestDocument("Tenant A Doc 2", "Another document for tenant A", 30);
//         document3.TenantId = "tenant-a";
//
//         await repository.InsertManyAsync(new[] { document1, document2, document3 });
//
//         // Act
//         var tenantADocs = await repository.GetAllAsync(tenantId: "tenant-a");
//         var tenantBDocs = await repository.GetAllAsync(tenantId: "tenant-b");
//
//         // Assert
//         tenantADocs.Should().HaveCount(2);
//         tenantADocs.All(d => d.TenantId == "tenant-a").Should().BeTrue();
//
//         tenantBDocs.Should().HaveCount(1);
//         tenantBDocs.All(d => d.TenantId == "tenant-b").Should().BeTrue();
//     }
//
//     /// <summary>
//     /// Tests that bulk write operations work properly
//     /// </summary>
//     [Fact]
//     public async Task BulkWriteAsync_ShouldProcessMultipleOperations()
//     {
//         // Arrange
//         var repository = GetRepository<TestDocument>();
//
//         // Create initial documents
//         var document1 = CreateTestDocument("Doc 1", "First document", 10);
//         var document2 = CreateTestDocument("Doc 2", "Second document", 20);
//         await repository.InsertManyAsync(new[] { document1, document2 });
//
//         // Prepare bulk operations
//         var operations = new List<MongoDB.Driver.WriteModel<TestDocument>>();
//
//         // Update operation
//         var update = MongoDB.Driver.Builders<TestDocument>.Update
//             .Set(d => d.Name, "Updated Doc 1")
//             .Set(d => d.Score, 15);
//
//         var updateFilter = MongoDB.Driver.Builders<TestDocument>.Filter.Eq(d => d.Id, document1.Id);
//         operations.Add(new MongoDB.Driver.UpdateOneModel<TestDocument>(updateFilter, update));
//
//         // Delete operation
//         var deleteFilter = MongoDB.Driver.Builders<TestDocument>.Filter.Eq(d => d.Id, document2.Id);
//         operations.Add(new MongoDB.Driver.DeleteOneModel<TestDocument>(deleteFilter));
//
//         // Insert operation
//         var document3 = CreateTestDocument("Doc 3", "Third document", 30);
//         operations.Add(new MongoDB.Driver.InsertOneModel<TestDocument>(document3));
//
//         // Act
//         var result = await repository.BulkWriteAsync(operations);
//
//         // Assert
//         result.IsAcknowledged.Should().BeTrue();
//         result.InsertedCount.Should().Be(1);
//         result.ModifiedCount.Should().Be(1);
//         result.DeletedCount.Should().Be(1);
//
//         // Verify the results
//         var documents = await repository.GetAllAsync();
//         documents.Should().HaveCount(2); // Doc 1 (updated) and Doc 3 (inserted)
//
//         var updated = documents.FirstOrDefault(d => d.Id == document1.Id);
//         updated.Should().NotBeNull();
//         updated!.Name.Should().Be("Updated Doc 1");
//         updated.Score.Should().Be(15);
//
//         var inserted = documents.FirstOrDefault(d => d.Name == "Doc 3");
//         inserted.Should().NotBeNull();
//     }
//
//     /// <summary>
//     /// Helper method to create a test document
//     /// </summary>
//     private static TestDocument CreateTestDocument(string name = "Test Document",
//         string description = "Test Description", int score = 42)
//     {
//         return new TestDocument
//         {
//             Name = name,
//             Description = description,
//             Score = score,
//             Tags = new List<string> { "test", "document", "integration" },
//             Metadata = new TestMetadata
//             {
//                 Version = "1.0",
//                 Properties = new Dictionary<string, string>
//                 {
//                     { "type", "test" },
//                     { "category", "integration" }
//                 }
//             }
//         };
//     }
// }