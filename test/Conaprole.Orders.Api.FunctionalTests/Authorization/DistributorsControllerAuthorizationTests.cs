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
        var request = new AddDistributorCategoryRequest("CONGELADOS"); // Use non-deprecated category

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
        var request = new AddDistributorCategoryRequest("CONGELADOS"); // Use non-deprecated category

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

        // Act - Use new endpoint without body
        var response = await HttpClient.DeleteAsync("/api/distributors/+59899887766/categories/LACTEOS");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveCategory_WithoutDistributorsWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("distributors:read"); // Different permission
        await CreateDistributorAsync("+59899887766");

        // Act - Use new endpoint without body
        var response = await HttpClient.DeleteAsync("/api/distributors/+59899887766/categories/LACTEOS");

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
        await AuthorizationTestHelper.CreateUserWithPermissionAndSetAuthAsync(
            HttpClient, SqlConnectionFactory, permission);
    }

    private new async Task CreateDistributorAsync(string phoneNumber)
    {
        var distributorId = Guid.NewGuid();
        const string sql = @"
            INSERT INTO distributor (id, phone_number, name, address, created_at, supported_categories)
            VALUES (@Id, @PhoneNumber, @Name, @Address, now(), @Categories);";

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = distributorId,
            PhoneNumber = phoneNumber,
            Name = "Test Distributor",
            Address = "Test Address",
            Categories = "LACTEOS" // Only LACTEOS category, so we can add others
        });
    }

    #endregion
}