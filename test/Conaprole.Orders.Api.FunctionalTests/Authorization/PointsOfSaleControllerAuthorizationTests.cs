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
        await AuthorizationTestHelper.CreateUserWithPermissionAndSetAuthAsync(HttpClient, SqlConnectionFactory, permission);
    }
    
    private async Task CreateUserWithoutPermissionAndSetAuthAsync(string permission)
    {
        // Create user with a different permission (not the required one) 
        var alternativePermission = permission switch
        {
            "pointsofsale:read" => "users:write", // User has users:write but not pointsofsale:read
            "pointsofsale:write" => "users:read", // User has users:read but not pointsofsale:write
            _ => "users:read" // Default fallback
        };
        
        await AuthorizationTestHelper.CreateUserWithPermissionAndSetAuthAsync(
            HttpClient, SqlConnectionFactory, alternativePermission);
    }

    #endregion
}