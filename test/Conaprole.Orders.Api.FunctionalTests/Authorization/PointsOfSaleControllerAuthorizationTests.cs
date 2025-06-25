using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.PointsOfSale.Dtos;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
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
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission

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
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission

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
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
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
        // Create user with specific permission directly in database
        var userId = Guid.NewGuid();
        var email = $"authuser+{Guid.NewGuid():N}@test.com";
        var identityId = Guid.NewGuid().ToString();

        using var connection = SqlConnectionFactory.CreateConnection();

        // Insert user
        await connection.ExecuteAsync(@"
            INSERT INTO users (id, identity_id, first_name, last_name, email, created_at)
            VALUES (@Id, @IdentityId, @FirstName, @LastName, @Email, now())",
            new
            {
                Id = userId,
                IdentityId = identityId,
                FirstName = "Auth",
                LastName = "User",
                Email = email
            });

        // Find role that has this permission
        var roleId = await GetRoleIdForPermissionAsync(permission);
        
        // Assign role to user
        await connection.ExecuteAsync(@"
            INSERT INTO role_user (user_id, role_id)
            VALUES (@UserId, @RoleId)",
            new
            {
                UserId = userId,
                RoleId = roleId
            });

        // Register user with authentication service and login
        var password = "TestPassword123";
        var registerRequest = new RegisterUserRequest(email, "Auth", "User", password);
        await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);

        var loginRequest = new LogInUserRequest(email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private async Task<int> GetRoleIdForPermissionAsync(string permission)
    {
        // Map permissions to roles based on typical role-permission assignments
        return permission switch
        {
            "users:read" => 1, // Registered role typically has basic read permissions
            "users:write" => 3, // Administrator role has write permissions  
            "admin:access" => 3, // Administrator role
            "distributors:read" => 4, // Distributor role
            "distributors:write" => 3, // Administrator role
            "pointsofsale:read" => 1, // Registered role
            "pointsofsale:write" => 3, // Administrator role
            "products:read" => 1, // Registered role
            "products:write" => 3, // Administrator role
            "orders:read" => 1, // Registered role
            "orders:write" => 3, // Administrator role
            _ => 3 // Default to Administrator role
        };
    }

    #endregion
}