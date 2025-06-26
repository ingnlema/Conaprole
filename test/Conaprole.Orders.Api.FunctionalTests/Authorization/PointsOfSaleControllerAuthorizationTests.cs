using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using Dapper;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Authorization;

[Collection("ApiCollection")]
public class PointsOfSaleControllerAuthorizationTests : BaseFunctionalTest
{
    public PointsOfSaleControllerAuthorizationTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    #region pointsofsale:read permission tests

    [Fact]
    public async Task GetPointsOfSale_WithPointsOfSaleReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read");

        // Act
        var response = await HttpClient.GetAsync("/api/pos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPointsOfSale_WithoutPointsOfSaleReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithoutPermissionAndSetAuthAsync("pointsofsale:read");

        // Act
        var response = await HttpClient.GetAsync("/api/pos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPointOfSaleByPhoneNumber_WithPointsOfSaleReadPermission_ShouldReturn200OrNotFound()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read");

        // Act
        var response = await HttpClient.GetAsync("/api/pos/by-phone/+59891234567");

        // Assert - Can be NotFound if POS doesn't exist, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPointOfSaleByPhoneNumber_WithoutPointsOfSaleReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithoutPermissionAndSetAuthAsync("pointsofsale:read");

        // Act
        var response = await HttpClient.GetAsync("/api/pos/by-phone/+59891234567");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDistributorsByPOS_WithPointsOfSaleReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read");
        await CreatePointOfSaleAsync("+59891234567");

        // Act
        var response = await HttpClient.GetAsync("/api/pos/+59891234567/distributors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDistributorsByPOS_WithoutPointsOfSaleReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithoutPermissionAndSetAuthAsync("pointsofsale:read");
        await CreatePointOfSaleAsync("+59891234567");

        // Act
        var response = await HttpClient.GetAsync("/api/pos/+59891234567/distributors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region pointsofsale:write permission tests

    [Fact]
    public async Task CreatePointOfSale_WithPointsOfSaleWritePermission_ShouldReturn201()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:write");
        var request = new CreatePointOfSaleRequest(
            "Test POS",
            "+59899999999",
            "Montevideo",
            "Test Street 123",
            "11000");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/pos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePointOfSale_WithoutPointsOfSaleWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read"); // Different permission
        var request = new CreatePointOfSaleRequest(
            "Test POS",
            "+59899999999",
            "Montevideo",
            "Test Street 123",
            "11000");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/pos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DisablePointOfSale_WithPointsOfSaleWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:write");
        await CreatePointOfSaleAsync("+59891234567");

        // Act
        var response = await HttpClient.PatchAsync("/api/pos/+59891234567", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DisablePointOfSale_WithoutPointsOfSaleWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read"); // Different permission
        await CreatePointOfSaleAsync("+59891234567");

        // Act
        var response = await HttpClient.PatchAsync("/api/pos/+59891234567", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task EnablePointOfSale_WithPointsOfSaleWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:write");
        await CreateInactivePointOfSaleAsync("+59891234567");

        // Act
        var response = await HttpClient.PatchAsync("/api/pos/+59891234567/enable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task EnablePointOfSale_WithoutPointsOfSaleWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read"); // Different permission
        await CreateInactivePointOfSaleAsync("+59891234567");

        // Act
        var response = await HttpClient.PatchAsync("/api/pos/+59891234567/enable", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignDistributor_WithPointsOfSaleWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:write");
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");
        var request = new AssignDistributorToPointOfSaleRequest("+59899887766", "LACTEOS");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/pos/+59891234567/distributors", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AssignDistributor_WithoutPointsOfSaleWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read"); // Different permission
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");
        var request = new AssignDistributorToPointOfSaleRequest("+59899887766", "LACTEOS");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/pos/+59891234567/distributors", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnassignDistributor_WithPointsOfSaleWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:write");
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.DeleteAsync("/api/pos/+59891234567/distributors/+59899887766/categories/LACTEOS");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnassignDistributor_WithoutPointsOfSaleWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("pointsofsale:read"); // Different permission
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.DeleteAsync("/api/pos/+59891234567/distributors/+59899887766/categories/LACTEOS");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper methods

    private async Task CreateUserWithPermissionAndSetAuthAsync(string permission)
    {
        await CreateUserWithRoleAndSetAuthAsync(GetRoleIdWithPermission(permission));
    }
    
    private async Task CreateUserWithoutPermissionAndSetAuthAsync(string permission)
    {
        // For read permissions where all major roles have them, create a minimal test role
        if (permission.EndsWith(":read"))
        {
            var emptyRoleId = await CreateEmptyTestRoleAsync();
            await CreateUserWithRoleAndSetAuthAsync(emptyRoleId);
        }
        else
        {
            await CreateUserWithRoleAndSetAuthAsync(GetRoleIdWithoutPermission(permission));
        }
    }

    private async Task CreateUserWithRoleAndSetAuthAsync(int roleId)
    {
        // Create user with specific role through proper registration then update roles
        var email = $"authuser+{Guid.NewGuid():N}@test.com";
        var password = "TestPassword123";

        // Register user normally first
        var registerRequest = new RegisterUserRequest(email, "Auth", "User", password);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        using var connection = SqlConnectionFactory.CreateConnection();

        // Get the created user ID
        var userId = await connection.QuerySingleAsync<Guid>(@"
            SELECT id FROM users WHERE email = @Email", 
            new { Email = email });

        // Remove default roles
        await connection.ExecuteAsync(@"
            DELETE FROM role_user WHERE users_id = @UserId", 
            new { UserId = userId });

        // Assign the specific role to user
        await connection.ExecuteAsync(@"
            INSERT INTO role_user (users_id, roles_id)
            VALUES (@UserId, @RoleId)",
            new
            {
                UserId = userId,
                RoleId = roleId
            });

        // Login to get access token
        var loginRequest = new LogInUserRequest(email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private int GetRoleIdWithPermission(string permission)
    {
        // Return role ID that HAS the specified permission
        return permission switch
        {
            "users:read" => 1, // Registered role has users:read
            "users:write" => 3, // Administrator role has users:write  
            "admin:access" => 3, // Administrator role has admin:access
            "distributors:read" => 1, // Registered role has distributors:read
            "distributors:write" => 4, // Distributor role has distributors:write
            "pointsofsale:read" => 1, // Registered role has pointsofsale:read
            "pointsofsale:write" => 4, // Distributor role has pointsofsale:write
            "products:read" => 1, // Registered role has products:read
            "products:write" => 4, // Distributor role has products:write
            "orders:read" => 1, // Registered role has orders:read
            "orders:write" => 4, // Distributor role has orders:write
            _ => 3 // Default to Administrator role
        };
    }
    
    private int GetRoleIdWithoutPermission(string permission)
    {
        // Return role ID that does NOT have the specified permission
        return permission switch
        {
            "users:write" => 1, // Registered role doesn't have users:write
            "admin:access" => 1, // Registered role doesn't have admin:access
            "distributors:write" => 1, // Registered role doesn't have distributors:write
            "pointsofsale:write" => 1, // Registered role doesn't have pointsofsale:write
            "products:write" => 1, // Registered role doesn't have products:write
            "orders:write" => 1, // Registered role doesn't have orders:write
            _ => 1 // Default to Registered role (has fewer permissions)
        };
    }

    private async Task<int> CreateEmptyTestRoleAsync()
    {
        using var connection = SqlConnectionFactory.CreateConnection();
        
        // Create a test role with no permissions
        var roleId = await connection.QuerySingleAsync<int>(@"
            INSERT INTO roles (name) 
            VALUES (@Name) 
            RETURNING id",
            new { Name = $"TestRole_{Guid.NewGuid():N}" });
            
        return roleId;
    }

    private async Task<int> GetRoleIdForPermissionAsync(string permission)
    {
        // Map permissions to appropriate roles based on actual role-permission assignments
        // from migrations: Registered(1) has read perms, Administrator(3) has all, Distributor(4) has most except users:write/admin:access
        return permission switch
        {
            // For positive tests - use roles that HAVE the permission
            "users:read" => 1, // Registered role has users:read
            "users:write" => 3, // Administrator role has users:write  
            "admin:access" => 3, // Administrator role has admin:access
            "distributors:read" => 1, // Registered role has distributors:read
            "distributors:write" => 4, // Distributor role has distributors:write
            "pointsofsale:read" => 1, // Registered role has pointsofsale:read
            "pointsofsale:write" => 4, // Distributor role has pointsofsale:write
            "products:read" => 1, // Registered role has products:read
            "products:write" => 4, // Distributor role has products:write
            "orders:read" => 1, // Registered role has orders:read
            "orders:write" => 4, // Distributor role has orders:write
            _ => 3 // Default to Administrator role
        };
    }

    #endregion
}