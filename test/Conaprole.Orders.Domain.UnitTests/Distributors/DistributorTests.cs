using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Shared;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Distributors;

public class DistributorTests : BaseTest
{
    [Fact]
    public void Constructor_Should_SetPropertyValues()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var distributor = new Distributor(
            id,
            DistributorData.Name,
            DistributorData.PhoneNumber,
            DistributorData.Address,
            DistributorData.CreatedAt,
            DistributorData.SupportedCategories);

        // Assert
        distributor.Id.Should().Be(id);
        distributor.Name.Should().Be(DistributorData.Name);
        distributor.PhoneNumber.Should().Be(DistributorData.PhoneNumber);
        distributor.Address.Should().Be(DistributorData.Address);
        distributor.CreatedAt.Should().Be(DistributorData.CreatedAt);
        distributor.SupportedCategories.Should().BeEquivalentTo(DistributorData.SupportedCategories);
        distributor.PointSales.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_Should_CreateEmptyCategories_WhenNoneProvided()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var distributor = new Distributor(
            id,
            DistributorData.Name,
            DistributorData.PhoneNumber,
            DistributorData.Address,
            DistributorData.CreatedAt,
            Enumerable.Empty<Category>());

        // Assert
        distributor.SupportedCategories.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AddCategory_Should_AddNewCategory_WhenNotExists()
    {
        // Arrange
        var distributor = new Distributor(
            Guid.NewGuid(),
            DistributorData.Name,
            DistributorData.PhoneNumber,
            DistributorData.Address,
            DistributorData.CreatedAt,
            new[] { Category.LACTEOS });

        // Act
        var result = distributor.AddCategory(Category.CONGELADOS);

        // Assert
        result.Should().BeTrue();
        distributor.SupportedCategories.Should().Contain(Category.CONGELADOS);
        distributor.SupportedCategories.Should().HaveCount(2);
    }

    [Fact]
    public void AddCategory_Should_ReturnFalse_WhenCategoryAlreadyExists()
    {
        // Arrange
        var distributor = new Distributor(
            Guid.NewGuid(),
            DistributorData.Name,
            DistributorData.PhoneNumber,
            DistributorData.Address,
            DistributorData.CreatedAt,
            new[] { Category.LACTEOS });

        // Act
        var result = distributor.AddCategory(Category.LACTEOS);

        // Assert
        result.Should().BeFalse();
        distributor.SupportedCategories.Should().HaveCount(1);
        distributor.SupportedCategories.Should().Contain(Category.LACTEOS);
    }

    [Fact]
    public void RemoveCategory_Should_RemoveCategory_WhenExists()
    {
        // Arrange
        var distributor = new Distributor(
            Guid.NewGuid(),
            DistributorData.Name,
            DistributorData.PhoneNumber,
            DistributorData.Address,
            DistributorData.CreatedAt,
            new[] { Category.LACTEOS, Category.CONGELADOS });

        // Act
        var result = distributor.RemoveCategory(Category.LACTEOS);

        // Assert
        result.Should().BeTrue();
        distributor.SupportedCategories.Should().NotContain(Category.LACTEOS);
        distributor.SupportedCategories.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveCategory_Should_ReturnFalse_WhenCategoryDoesNotExist()
    {
        // Arrange
        var distributor = new Distributor(
            Guid.NewGuid(),
            DistributorData.Name,
            DistributorData.PhoneNumber,
            DistributorData.Address,
            DistributorData.CreatedAt,
            new[] { Category.LACTEOS });

        // Act
        var result = distributor.RemoveCategory(Category.CONGELADOS);

        // Assert
        result.Should().BeFalse();
        distributor.SupportedCategories.Should().HaveCount(1);
        distributor.SupportedCategories.Should().Contain(Category.LACTEOS);
    }
}