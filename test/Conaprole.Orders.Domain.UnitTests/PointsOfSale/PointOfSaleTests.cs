using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.PointsOfSale;
using Conaprole.Orders.Domain.Shared;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.PointsOfSale;

public class PointOfSaleTests : BaseTest
{
    [Fact]
    public void Constructor_Should_SetPropertyValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var pointOfSale = new PointOfSale(
            id,
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        // Assert
        pointOfSale.Id.Should().Be(id);
        pointOfSale.Name.Should().Be(PointOfSaleData.Name);
        pointOfSale.PhoneNumber.Should().Be(PointOfSaleData.PhoneNumber);
        pointOfSale.Address.Should().Be(PointOfSaleData.Address.ToString());
        pointOfSale.CreatedAt.Should().Be(PointOfSaleData.CreatedAt);
        pointOfSale.IsActive.Should().BeTrue(); // Default should be active
        pointOfSale.Distributors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalse()
    {
        // Arrange
        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        // Act
        pointOfSale.Deactivate();

        // Assert
        pointOfSale.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_Should_SetIsActiveToTrue()
    {
        // Arrange
        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        pointOfSale.Deactivate(); // First deactivate

        // Act
        pointOfSale.Activate();

        // Assert
        pointOfSale.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AssignDistributor_Should_AddDistributor_WhenNotExists()
    {
        // Arrange
        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        var distributorId = Guid.NewGuid();
        var category = Category.LACTEOS;

        // Act
        var result = pointOfSale.AssignDistributor(distributorId, category);

        // Assert
        result.Should().BeTrue();
        pointOfSale.Distributors.Should().HaveCount(1);
        pointOfSale.Distributors.First().DistributorId.Should().Be(distributorId);
        pointOfSale.Distributors.First().Category.Should().Be(category);
    }

    [Fact]
    public void AssignDistributor_Should_ReturnFalse_WhenDistributorAlreadyAssigned()
    {
        // Arrange
        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        var distributorId = Guid.NewGuid();
        var category = Category.LACTEOS;

        pointOfSale.AssignDistributor(distributorId, category); // First assignment

        // Act
        var result = pointOfSale.AssignDistributor(distributorId, category); // Duplicate

        // Assert
        result.Should().BeFalse();
        pointOfSale.Distributors.Should().HaveCount(1); // Should not duplicate
    }

    [Fact]
    public void AssignDistributor_Should_AllowSameDistributor_WithDifferentCategory()
    {
        // Arrange
        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        var distributorId = Guid.NewGuid();

        pointOfSale.AssignDistributor(distributorId, Category.LACTEOS);

        // Act
        var result = pointOfSale.AssignDistributor(distributorId, Category.CONGELADOS);

        // Assert
        result.Should().BeTrue();
        pointOfSale.Distributors.Should().HaveCount(2);
    }

    [Fact]
    public void UnassignDistributor_Should_RemoveDistributor_WhenExists()
    {
        // Arrange
        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        var distributorId = Guid.NewGuid();
        var category = Category.LACTEOS;

        pointOfSale.AssignDistributor(distributorId, category);

        // Act
        var result = pointOfSale.UnassignDistributor(distributorId, category);

        // Assert
        result.Should().BeTrue();
        pointOfSale.Distributors.Should().BeEmpty();
    }

    [Fact]
    public void UnassignDistributor_Should_ReturnFalse_WhenDistributorNotFound()
    {
        // Arrange
        var pointOfSale = new PointOfSale(
            Guid.NewGuid(),
            PointOfSaleData.Name,
            PointOfSaleData.PhoneNumber,
            PointOfSaleData.Address,
            PointOfSaleData.CreatedAt);

        var distributorId = Guid.NewGuid();
        var category = Category.LACTEOS;

        // Act
        var result = pointOfSale.UnassignDistributor(distributorId, category);

        // Assert
        result.Should().BeFalse();
        pointOfSale.Distributors.Should().BeEmpty();
    }
}