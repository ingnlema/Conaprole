using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Products;

public class ProductTests : BaseTest
{
    [Fact]
    public void Constructor_Should_SetPropertyValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var product = new Product(
            id,
            ProductData.ExternalProductId,
            ProductData.Name,
            ProductData.UnitPrice,
            ProductData.Category,
            ProductData.Description,
            ProductData.LastUpdated);

        // Assert
        product.Id.Should().Be(id);
        product.ExternalProductId.Should().Be(ProductData.ExternalProductId);
        product.Name.Should().Be(ProductData.Name);
        product.UnitPrice.Should().Be(ProductData.UnitPrice);
        product.Category.Should().Be(ProductData.Category);
        product.Description.Should().Be(ProductData.Description);
        product.LastUpdated.Should().Be(ProductData.LastUpdated);
    }

    [Fact]
    public void Constructor_Should_CreateValidProduct_WithAllCategories()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categories = new[] { Category.CONGELADOS, Category.LACTEOS, Category.SUBPRODUCTOS };

        foreach (var category in categories)
        {
            // Act
            var product = new Product(
                id,
                ProductData.ExternalProductId,
                ProductData.Name,
                ProductData.UnitPrice,
                category,
                ProductData.Description,
                ProductData.LastUpdated);

            // Assert
            product.Category.Should().Be(category);
        }
    }
}