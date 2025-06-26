using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Products;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Shared;
using Dapper;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Authorization;

[Collection("ApiCollection")]
public class ProductsControllerAuthorizationTests : BaseFunctionalTest
{
    public ProductsControllerAuthorizationTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    #region products:read permission tests

    [Fact]
    public async Task GetProducts_WithProductsReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("products:read");

        // Act
        var response = await HttpClient.GetAsync("/api/Products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProducts_WithoutProductsReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithoutPermissionAndSetAuthAsync("products:read");

        // Act
        var response = await HttpClient.GetAsync("/api/Products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProduct_WithProductsReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("products:read");
        var productId = await CreateProductAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProduct_WithoutProductsReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithoutPermissionAndSetAuthAsync("products:read");
        var productId = await CreateProductAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region products:write permission tests

    [Fact]
    public async Task CreateProduct_WithProductsWritePermission_ShouldReturn201()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("products:write");
        var request = new CreateProductRequest(
            "TEST-PRODUCT-001",
            "Test Product",
            100.50m,
            "UYU",
            "Test Description",
            Category.LACTEOS);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/Products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateProduct_WithoutProductsWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithoutPermissionAndSetAuthAsync("products:write");
        var request = new CreateProductRequest(
            "TEST-PRODUCT-002",
            "Test Product 2",
            150.75m,
            "UYU",
            "Test Description 2",
            Category.LACTEOS);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/Products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper methods

    private async Task CreateUserWithPermissionAndSetAuthAsync(string permission)
    {
        await CreateUserWithRoleAndSetAuthAsync(GetRoleIdWithPermission(permission));
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

    private async Task<Guid> CreateProductAsync()
    {
        var request = new CreateProductRequest(
            "TEST-PRODUCT-001",
            "Test Product",
            100.00m,
            "UYU",
            "Test Description",
            Category.LACTEOS);

        var response = await HttpClient.PostAsJsonAsync("/api/Products", request);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        if (location != null && Guid.TryParse(location.Split('/').Last(), out var productId))
        {
            return productId;
        }

        throw new InvalidOperationException("Could not extract product ID from response");
    }

    #endregion
}