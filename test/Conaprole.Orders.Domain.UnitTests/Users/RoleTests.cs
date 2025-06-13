using Conaprole.Orders.Domain.UnitTests.Infrastructure;
using Conaprole.Orders.Domain.Users;
using FluentAssertions;

namespace Conaprole.Orders.Domain.UnitTests.Users;

public class RoleTests : BaseTest
{
    [Fact]
    public void Registered_Should_HaveCorrectProperties()
    {
        // Act
        var role = Role.Registered;

        // Assert
        role.Id.Should().Be(1);
        role.Name.Should().Be("Registered");
        role.Users.Should().NotBeNull();
        role.Permissions.Should().NotBeNull();
    }

    [Fact]
    public void API_Should_HaveCorrectProperties()
    {
        // Act
        var role = Role.API;

        // Assert
        role.Id.Should().Be(2);
        role.Name.Should().Be("API");
        role.Users.Should().NotBeNull();
        role.Permissions.Should().NotBeNull();
    }

    [Fact]
    public void Administrator_Should_HaveCorrectProperties()
    {
        // Act
        var role = Role.Administrator;

        // Assert
        role.Id.Should().Be(3);
        role.Name.Should().Be("Administrator");
        role.Users.Should().NotBeNull();
        role.Permissions.Should().NotBeNull();
    }

    [Fact]
    public void Distributor_Should_HaveCorrectProperties()
    {
        // Act
        var role = Role.Distributor;

        // Assert
        role.Id.Should().Be(4);
        role.Name.Should().Be("Distributor");
        role.Users.Should().NotBeNull();
        role.Permissions.Should().NotBeNull();
    }

    [Fact]
    public void AllRoles_Should_HaveUniqueIds()
    {
        // Arrange
        var roles = new[] { Role.Registered, Role.API, Role.Administrator, Role.Distributor };

        // Act & Assert
        var ids = roles.Select(r => r.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllRoles_Should_HaveUniqueNames()
    {
        // Arrange
        var roles = new[] { Role.Registered, Role.API, Role.Administrator, Role.Distributor };

        // Act & Assert
        var names = roles.Select(r => r.Name).ToList();
        names.Should().OnlyHaveUniqueItems();
    }
}