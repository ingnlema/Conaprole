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
        // Arrange - first create a product with proper permissions
        await CreateUserWithPermissionAndSetAuthAsync("products:write");
        var productId = await CreateProductAsync();
        
        // Then switch to a user without products:read permission
        await CreateUserWithoutPermissionAndSetAuthAsync("products:read");

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
        await AuthorizationTestHelper.CreateUserWithPermissionAndSetAuthAsync(
            HttpClient, SqlConnectionFactory, permission);
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
        // Create user with a different permission (not the required one) 
        var alternativePermission = permission switch
        {
            "products:read" => "users:write", // User has users:write but not products:read
            "products:write" => "users:read", // User has users:read but not products:write
            _ => "users:read" // Default fallback
        };
        
        await AuthorizationTestHelper.CreateUserWithPermissionAndSetAuthAsync(
            HttpClient, SqlConnectionFactory, alternativePermission);
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