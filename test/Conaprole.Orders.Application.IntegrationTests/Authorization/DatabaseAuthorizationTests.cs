using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Application.Users.GetUserPermissions;
using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Application.IntegrationTests.Authorization;

[Collection("IntegrationCollection")]
public class DatabaseAuthorizationTests : BaseIntegrationTest
{
    public DatabaseAuthorizationTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetUserPermissions_Should_ReturnPermissionsFromDatabase_Only()
    {
        // Arrange - Create a user with specific roles
        var user = User.Create(
            new FirstName("Test"),
            new LastName("User"),
            new Email("test@example.com"),
            DateTime.UtcNow);
        
        user.SetIdentityId("test-identity-id");

        // Get existing roles from database
        var registeredRole = await DbContext.Set<Role>()
            .FirstAsync(r => r.Name == "Registered");
        var administratorRole = await DbContext.Set<Role>()
            .FirstAsync(r => r.Name == "Administrator");

        // Assign roles to user
        user.AssignRole(registeredRole);
        user.AssignRole(administratorRole);

        DbContext.Set<User>().Add(user);
        await DbContext.SaveChangesAsync();

        var query = new GetUserPermissionsQuery(user.Id);

        // Act
        var result = await Sender.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        var permissions = result.Value;
        
        // Should have permissions from both roles
        Assert.Contains(permissions, p => p.Name == "users:read");      // From Registered
        Assert.Contains(permissions, p => p.Name == "admin:access");    // From Administrator
        Assert.Contains(permissions, p => p.Name == "users:write");     // From Administrator
    }

    [Fact]
    public async Task GetUserPermissions_Should_ReturnOnlyAssignedRolePermissions()
    {
        // Arrange - Create a user with only the Registered role
        var user = User.Create(
            new FirstName("Limited"),
            new LastName("User"),
            new Email("limited@example.com"),
            DateTime.UtcNow);
        
        user.SetIdentityId("limited-identity-id");

        // Only assign the Registered role
        var registeredRole = await DbContext.Set<Role>()
            .FirstAsync(r => r.Name == "Registered");

        user.AssignRole(registeredRole);

        DbContext.Set<User>().Add(user);
        await DbContext.SaveChangesAsync();

        var query = new GetUserPermissionsQuery(user.Id);

        // Act
        var result = await Sender.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        var permissions = result.Value;
        
        // Debug: Print all permissions to understand what we're getting
        var permissionNames = permissions.Select(p => p.Name).ToList();
        var permissionNamesStr = string.Join(", ", permissionNames);
        // This will show in test output if the test fails
        
        // Should only have permissions from Registered role (whatever they currently are in the database)
        Assert.Contains(permissions, p => p.Name == "users:read");
        Assert.Contains(permissions, p => p.Name == "users:write");
        Assert.Contains(permissions, p => p.Name == "products:read");
        
        // The important test is that we get permissions from the database, not JWT claims
        // and that we DON'T get admin permissions
        Assert.DoesNotContain(permissions, p => p.Name == "admin:access");
    }

    [Fact]
    public async Task AuthorizationService_Should_QueryDatabaseForPermissions()
    {
        // This test verifies that the AuthorizationService implementation
        // correctly queries the database for user permissions
        
        // Arrange - Create a user with specific permissions
        var user = User.Create(
            new FirstName("Database"),
            new LastName("User"),
            new Email("database@example.com"),
            DateTime.UtcNow);
        
        user.SetIdentityId("database-test-identity");

        var distributorRole = await DbContext.Set<Role>()
            .FirstAsync(r => r.Name == "Distributor");

        user.AssignRole(distributorRole);

        DbContext.Set<User>().Add(user);
        await DbContext.SaveChangesAsync();

        // Act - Use the AuthorizationService directly
        var authorizationService = GetService<Application.Abstractions.Authentication.IAuthorizationService>();
        var permissions = await authorizationService.GetPermissionsForUserAsync("database-test-identity");

        // Assert - Should return permissions from the database
        Assert.Contains("users:read", permissions);
        Assert.Contains("distributors:read", permissions);
        Assert.Contains("distributors:write", permissions);
        Assert.Contains("pointsofsale:read", permissions);
        Assert.Contains("pointsofsale:write", permissions);
        Assert.Contains("products:read", permissions);
        Assert.Contains("orders:read", permissions);
        Assert.Contains("orders:write", permissions);
        
        // Should NOT have admin permissions
        Assert.DoesNotContain("admin:access", permissions);
        Assert.DoesNotContain("users:write", permissions);
    }
}