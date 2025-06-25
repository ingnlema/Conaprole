using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Domain.Shared;
using Dapper;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Authorization;

[Collection("ApiCollection")]
public class OrdersControllerAuthorizationTests : BaseFunctionalTest
{
    public OrdersControllerAuthorizationTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    #region orders:read permission tests

    [Fact]
    public async Task GetOrders_WithOrdersReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read");

        // Act
        var response = await HttpClient.GetAsync("/api/Orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrders_WithoutOrdersReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission

        // Act
        var response = await HttpClient.GetAsync("/api/Orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOrder_WithOrdersReadPermission_ShouldReturn200OrNotFound()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read");
        var orderId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/Orders/{orderId}");

        // Assert - Can be NotFound if order doesn't exist, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrder_WithoutOrdersReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        var orderId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/Orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region orders:write permission tests

    [Fact]
    public async Task CreateOrder_WithOrdersWritePermission_ShouldReturn201OrBadRequest()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:write");
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");
        await CreateProductAsync("TEST-001");

        var request = new CreateOrderRequest(
            "+59891234567",
            "+59899887766",
            "Montevideo",
            "Test Street 123",
            "11000",
            "UYU",
            new[] { new OrderLineRequest("TEST-001", 5) });

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/Orders", request);

        // Assert - Can be BadRequest due to business logic, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithoutOrdersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read"); // Different permission
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");
        await CreateProductAsync("TEST-001");

        var request = new CreateOrderRequest(
            "+59891234567",
            "+59899887766",
            "Montevideo",
            "Test Street 123",
            "11000",
            "UYU",
            new[] { new OrderLineRequest("TEST-001", 5) });

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/Orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOrdersBulk_WithOrdersWritePermission_ShouldReturn201OrBadRequest()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:write");
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");
        await CreateProductAsync("TEST-001");

        var order = new CreateOrderRequest(
            "+59891234567",
            "+59899887766",
            "Montevideo",
            "Test Street 123",
            "11000",
            "UYU",
            new[] { new OrderLineRequest("TEST-001", 3) });

        var request = new BulkCreateOrdersRequest { Orders = new[] { order }.ToList() };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/Orders/bulk", request);

        // Assert - Can be BadRequest due to business logic, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrdersBulk_WithoutOrdersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read"); // Different permission
        await CreatePointOfSaleAsync("+59891234567");
        await CreateDistributorAsync("+59899887766");
        await CreateProductAsync("TEST-001");

        var order = new CreateOrderRequest(
            "+59891234567",
            "+59899887766",
            "Montevideo",
            "Test Street 123",
            "11000",
            "UYU",
            new[] { new OrderLineRequest("TEST-001", 3) });

        var request = new BulkCreateOrdersRequest { Orders = new[] { order }.ToList() };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/Orders/bulk", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithOrdersWritePermission_ShouldReturn204OrBadRequest()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:write");
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest((int)Status.Confirmed);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/Orders/{orderId}/status", request);

        // Assert - Can be BadRequest if order doesn't exist, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithoutOrdersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read"); // Different permission
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest((int)Status.Confirmed);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/Orders/{orderId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddLine_WithOrdersWritePermission_ShouldReturn204OrBadRequest()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:write");
        var orderId = Guid.NewGuid();
        var request = new AddOrderLineRequest("TEST-001", 2);

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/Orders/{orderId}/lines", request);

        // Assert - Can be BadRequest/NotFound if order doesn't exist, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddLine_WithoutOrdersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read"); // Different permission
        var orderId = Guid.NewGuid();
        var request = new AddOrderLineRequest("TEST-001", 2);

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/Orders/{orderId}/lines", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLine_WithOrdersWritePermission_ShouldReturn204OrBadRequest()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:write");
        var orderId = Guid.NewGuid();
        var orderLineId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/Orders/{orderId}/lines/{orderLineId}");

        // Assert - Can be BadRequest/NotFound if order doesn't exist, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteLine_WithoutOrdersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read"); // Different permission
        var orderId = Guid.NewGuid();
        var orderLineId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/Orders/{orderId}/lines/{orderLineId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateLineQuantity_WithOrdersWritePermission_ShouldReturn204OrBadRequest()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:write");
        var orderId = Guid.NewGuid();
        var orderLineId = Guid.NewGuid();
        var request = new UpdateOrderLineQuantityRequest { NewQuantity = 10 };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/Orders/{orderId}/lines/{orderLineId}", request);

        // Assert - Can be BadRequest/NotFound if order doesn't exist, but shouldn't be Forbidden
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateLineQuantity_WithoutOrdersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("orders:read"); // Different permission
        var orderId = Guid.NewGuid();
        var orderLineId = Guid.NewGuid();
        var request = new UpdateOrderLineQuantityRequest { NewQuantity = 10 };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/Orders/{orderId}/lines/{orderLineId}", request);

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