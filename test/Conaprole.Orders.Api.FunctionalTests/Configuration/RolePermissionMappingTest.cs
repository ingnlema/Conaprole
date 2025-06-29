using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Domain.Users;
using Conaprole.Orders.Infrastructure.Configuration.SeedData;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Configuration;

[Collection("ApiCollection")]
public class RolePermissionMappingTest : BaseFunctionalTest
{
    public RolePermissionMappingTest(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public void GetRolePermissionMappings_ShouldContainAllDomainRoles()
    {
        // Arrange
        var allRoles = Enum.GetValues<ApplicationRole>();
        
        // Act
        var mappings = RolePermissionMapping.GetRolePermissionMappings();

        // Assert
        foreach (var role in allRoles)
        {
            mappings.Should().ContainKey(role);
        }
    }

    [Fact]
    public void GetPermissionName_ShouldReturnCorrectFormat_ForAllPermissions()
    {
        // Arrange
        var allPermissions = Enum.GetValues<ApplicationPermission>();
        
        // Act & Assert
        foreach (var permission in allPermissions)
        {
            var permissionName = RolePermissionMapping.GetPermissionName(permission);
            
            permissionName.Should().NotBeNullOrEmpty();
            // Permission names should follow the pattern "resource:action"
            if (permission != ApplicationPermission.AdminAccess)
            {
                permissionName.Should().Contain(":");
            }
        }
    }

    [Fact]
    public void DomainExtensions_ShouldConvertRoleNamesCorrectly()
    {
        // Arrange
        var roles = new[] { ApplicationRole.Administrator, ApplicationRole.Registered };
        
        // Act
        var roleNames = roles.ToRoleNames();

        // Assert
        roleNames.Should().Contain("Administrator");
        roleNames.Should().Contain("Registered");
        roleNames.Should().HaveCount(2);
    }

    [Fact]
    public void DomainExtensions_ShouldConvertPermissionNamesCorrectly()
    {
        // Arrange
        var permissions = new[] { ApplicationPermission.UsersRead, ApplicationPermission.AdminAccess };
        
        // Act
        var permissionNames = permissions.ToPermissionNames();

        // Assert
        permissionNames.Should().Contain("users:read");
        permissionNames.Should().Contain("admin:access");
        permissionNames.Should().HaveCount(2);
    }
}