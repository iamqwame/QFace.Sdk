using FluentAssertions;
using QimErp.Shared.Common.Entities.ValueObjects;
using Xunit;

namespace QimErp.Shared.Common.Tests.Domain;

public class ProductValueObjectImageFileIdDomainTests
{
    private static ProductValueObject CreateProduct() =>
        ProductValueObject.Create("prd-1", "PRD-001", "Widget");

    [Fact]
    public void WithImageFileId_Should_Set_FileId_And_Clear_ImageUrl()
    {
        var product = CreateProduct();
        var fileId = Guid.NewGuid();

        product.WithImageFileId(fileId);

        product.ImageFileId.Should().Be(fileId);
        product.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void WithImageFileId_Should_Clear_Both_When_Null()
    {
        var product = CreateProduct()
            .WithImageFileId(Guid.NewGuid());

        product.WithImageFileId(null);

        product.ImageFileId.Should().BeNull();
        product.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void WithImageUrl_Should_Not_Persist_Url_Or_Synthesize_FileId_When_Url_Provided()
    {
        var product = CreateProduct();
        var url = "https://cdn.example/inventory/tenant/products/a.png";

        product.WithImageUrl(url);

        product.ImageUrl.Should().BeNull();
        product.ImageFileId.Should().BeNull();
    }

    [Fact]
    public void WithImageUrl_Should_Clear_Both_When_Null_Or_Whitespace()
    {
        var product = CreateProduct()
            .WithImageFileId(Guid.NewGuid());

        product.WithImageUrl(null);

        product.ImageUrl.Should().BeNull();
        product.ImageFileId.Should().BeNull();

        product.WithImageFileId(Guid.NewGuid());
        product.WithImageUrl("   ");

        product.ImageUrl.Should().BeNull();
        product.ImageFileId.Should().BeNull();
    }

    [Fact]
    public void WithImageUrl_NonEmpty_Should_Leave_Existing_FileId_Untouched()
    {
        var fileId = Guid.NewGuid();
        var product = CreateProduct().WithImageFileId(fileId);

        product.WithImageUrl("https://cdn.example/inventory/tenant/products/a.png");

        product.ImageFileId.Should().Be(fileId);
        product.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void Create_With_ImageFileId_Should_Set_FileId_And_Leave_Url_Null()
    {
        var fileId = Guid.NewGuid();

        var product = ProductValueObject.Create(
            "prd-1",
            "PRD-001",
            "Widget",
            imageFileId: fileId);

        product.ImageFileId.Should().Be(fileId);
        product.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void Create_With_Only_ImageUrl_Should_Not_Persist_Url()
    {
        var product = ProductValueObject.Create(
            "prd-1",
            "PRD-001",
            "Widget",
            imageUrl: "https://cdn.example/inventory/tenant/products/a.png");

        product.ImageUrl.Should().BeNull();
        product.ImageFileId.Should().BeNull();
    }

    [Fact]
    public void Create_With_Both_Should_Prefer_ImageFileId()
    {
        var fileId = Guid.NewGuid();

        var product = ProductValueObject.Create(
            "prd-1",
            "PRD-001",
            "Widget",
            imageUrl: "https://cdn.example/inventory/tenant/products/a.png",
            imageFileId: fileId);

        product.ImageFileId.Should().Be(fileId);
        product.ImageUrl.Should().BeNull();
    }
}
