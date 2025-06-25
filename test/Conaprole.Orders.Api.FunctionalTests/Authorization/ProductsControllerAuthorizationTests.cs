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
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission

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
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
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
        await CreateUserWithPermissionAndSetAuthAsync("products:read"); // Different permission
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
            INSERT INTO role_user (users_id, roles_id)
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