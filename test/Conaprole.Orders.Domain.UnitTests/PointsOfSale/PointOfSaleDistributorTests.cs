using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.PointsOfSale;

public class PointOfSaleDistributorTests : BaseTest
{
    [Fact]
    public void Constructor_Should_SetPropertyValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pointOfSaleId = Guid.NewGuid();
        var distributorId = Guid.NewGuid();
        var category = Category.LACTEOS;

        // Act
        var pointOfSaleDistributor = new PointOfSaleDistributor(
            id,
            pointOfSaleId,
            distributorId,
            category);

        // Assert
        pointOfSaleDistributor.Id.Should().Be(id);
        pointOfSaleDistributor.PointOfSaleId.Should().Be(pointOfSaleId);
        pointOfSaleDistributor.DistributorId.Should().Be(distributorId);
        pointOfSaleDistributor.Category.Should().Be(category);
        pointOfSaleDistributor.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_Should_GenerateIdAndSetProperties()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var distributorId = Guid.NewGuid();
        var category = Category.CONGELADOS;

        // Act
        var pointOfSaleDistributor = PointOfSaleDistributor.Create(
            pointOfSaleId,
            distributorId,
            category);

        // Assert
        pointOfSaleDistributor.Id.Should().NotBe(Guid.Empty);
        pointOfSaleDistributor.PointOfSaleId.Should().Be(pointOfSaleId);
        pointOfSaleDistributor.DistributorId.Should().Be(distributorId);
        pointOfSaleDistributor.Category.Should().Be(category);
        pointOfSaleDistributor.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(Category.LACTEOS)]
    [InlineData(Category.CONGELADOS)]
    [InlineData(Category.SUBPRODUCTOS)]
    public void Create_Should_Work_WithAllCategories(Category category)
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var distributorId = Guid.NewGuid();

        // Act
        var pointOfSaleDistributor = PointOfSaleDistributor.Create(
            pointOfSaleId,
            distributorId,
            category);

        // Assert
        pointOfSaleDistributor.Category.Should().Be(category);
        pointOfSaleDistributor.PointOfSaleId.Should().Be(pointOfSaleId);
        pointOfSaleDistributor.DistributorId.Should().Be(distributorId);
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var distributorId = Guid.NewGuid();
        var category = Category.LACTEOS;

        // Act
        var distributor1 = PointOfSaleDistributor.Create(pointOfSaleId, distributorId, category);
        var distributor2 = PointOfSaleDistributor.Create(pointOfSaleId, distributorId, category);

        // Assert
        distributor1.Id.Should().NotBe(distributor2.Id);
    }
}