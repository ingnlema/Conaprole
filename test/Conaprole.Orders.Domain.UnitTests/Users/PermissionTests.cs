using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Users;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Users;

public class PermissionTests : BaseTest
{
    [Theory]
    [InlineData(1, "users:read")]
    [InlineData(2, "users:write")]
    [InlineData(3, "distributors:read")]
    [InlineData(4, "distributors:write")]
    [InlineData(5, "pointsofsale:read")]
    [InlineData(6, "pointsofsale:write")]
    [InlineData(7, "products:read")]
    [InlineData(8, "products:write")]
    [InlineData(9, "orders:read")]
    [InlineData(10, "orders:write")]
    [InlineData(11, "admin:access")]
    public void StaticPermissions_Should_HaveCorrectProperties(int expectedId, string expectedName)
    {
        // Arrange
        var permissions = new[]
        {
            Permission.UsersRead, Permission.UsersWrite,
            Permission.DistributorsRead, Permission.DistributorsWrite,
            Permission.PointsOfSaleRead, Permission.PointsOfSaleWrite,
            Permission.ProductsRead, Permission.ProductsWrite,
            Permission.OrdersRead, Permission.OrdersWrite,
            Permission.AdminAccess
        };

        // Act
        var permission = permissions.FirstOrDefault(p => p.Id == expectedId);

        // Assert
        permission.Should().NotBeNull();
        permission!.Id.Should().Be(expectedId);
        permission.Name.Should().Be(expectedName);
    }

    [Fact]
    public void AllPermissions_Should_HaveUniqueIds()
    {
        // Arrange
        var permissions = new[]
        {
            Permission.UsersRead, Permission.UsersWrite,
            Permission.DistributorsRead, Permission.DistributorsWrite,
            Permission.PointsOfSaleRead, Permission.PointsOfSaleWrite,
            Permission.ProductsRead, Permission.ProductsWrite,
            Permission.OrdersRead, Permission.OrdersWrite,
            Permission.AdminAccess
        };

        // Act & Assert
        var ids = permissions.Select(p => p.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllPermissions_Should_HaveUniqueNames()
    {
        // Arrange
        var permissions = new[]
        {
            Permission.UsersRead, Permission.UsersWrite,
            Permission.DistributorsRead, Permission.DistributorsWrite,
            Permission.PointsOfSaleRead, Permission.PointsOfSaleWrite,
            Permission.ProductsRead, Permission.ProductsWrite,
            Permission.OrdersRead, Permission.OrdersWrite,
            Permission.AdminAccess
        };

        // Act & Assert
        var names = permissions.Select(p => p.Name).ToList();
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Permission_Names_Should_Follow_ModuleColonAction_Pattern()
    {
        // Arrange
        var permissions = new[]
        {
            Permission.UsersRead, Permission.UsersWrite,
            Permission.DistributorsRead, Permission.DistributorsWrite,
            Permission.PointsOfSaleRead, Permission.PointsOfSaleWrite,
            Permission.ProductsRead, Permission.ProductsWrite,
            Permission.OrdersRead, Permission.OrdersWrite,
            Permission.AdminAccess
        };

        // Act & Assert
        foreach (var permission in permissions)
        {
            permission.Name.Should().Contain(":");
            permission.Name.Split(':').Should().HaveCount(2);
        }
    }
}