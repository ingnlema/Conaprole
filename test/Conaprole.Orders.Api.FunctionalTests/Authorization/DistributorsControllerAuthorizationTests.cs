using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Distributors.Dtos;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Shared;
using Dapper;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Authorization;

[Collection("ApiCollection")]
public class DistributorsControllerAuthorizationTests : BaseFunctionalTest
{
    public DistributorsControllerAuthorizationTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    #region distributors:read permission tests

    [Fact]
    public async Task GetDistributors_WithDistributorsReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDistributors_WithoutDistributorsReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission

        // Act
        var response = await HttpClient.GetAsync("/api/distributors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAssignedPointsOfSale_WithDistributorsReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read");
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/pos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAssignedPointsOfSale_WithoutDistributorsReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/pos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPointOfSaleDetails_WithDistributorsReadPermission_ShouldReturn200OrNotFound()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read");
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/pos/+59891234567");

        // Assert - Can be NotFound if POS doesn't exist, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPointOfSaleDetails_WithoutDistributorsReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/pos/+59891234567");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCategories_WithDistributorsReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read");
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategories_WithoutDistributorsReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region distributors:write permission tests

    [Fact]
    public async Task CreateDistributor_WithDistributorsWritePermission_ShouldReturn201()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:write");
        var request = new CreateDistributorRequest(
            "Test Distributor",
            "+59899999999",
            "Test Address",
            new[] { "LACTEOS" }.ToList());

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/distributors", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateDistributor_WithoutDistributorsWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read"); // Different permission
        var request = new CreateDistributorRequest(
            "Test Distributor",
            "+59899999999",
            "Test Address",
            new[] { "LACTEOS" }.ToList());

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/distributors", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddCategory_WithDistributorsWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:write");
        await CreateDistributorAsync("+59899887766");
        var request = new AddDistributorCategoryRequest("BEBIDAS");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/distributors/+59899887766/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddCategory_WithoutDistributorsWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read"); // Different permission
        await CreateDistributorAsync("+59899887766");
        var request = new AddDistributorCategoryRequest("BEBIDAS");

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/distributors/+59899887766/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveCategory_WithDistributorsWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:write");
        await CreateDistributorAsync("+59899887766");
        var request = new RemoveDistributorCategoryRequest("LACTEOS");

        // Act
        var response = await HttpClient.DeleteAsync("/api/distributors/+59899887766/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveCategory_WithoutDistributorsWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read"); // Different permission
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.DeleteAsync("/api/distributors/+59899887766/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region orders:read permission tests (for GetOrders endpoint)

    [Fact]
    public async Task GetOrders_WithOrdersReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read");
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrders_WithoutOrdersReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read"); // Different permission
        await CreateDistributorAsync("+59899887766");

        // Act
        var response = await HttpClient.GetAsync("/api/distributors/+59899887766/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper methods

    private async Task CreateUserWithPermissionAndSetAuthAsync(string permission)
    {
        // Create user with specific permission through proper registration then update roles
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

        // Find role that has this permission
        var roleId = await GetRoleIdForPermissionAsync(permission);
        
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