using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Users;
using Conaprole.Orders.Application.Users.LoginUser;
using Dapper;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Authorization;

[Collection("ApiCollection")]
public class UsersControllerAuthorizationTests : BaseFunctionalTest
{
    public UsersControllerAuthorizationTests(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    #region users:read permission tests

    [Fact]
    public async Task GetLoggedInUser_WithUsersReadPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read");

        // Act
        var response = await HttpClient.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLoggedInUser_WithoutUsersReadPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("products:read"); // Different permission

        // Act
        var response = await HttpClient.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region users:write permission tests

    [Fact]
    public async Task AssignRole_WithUsersWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:write");
        var targetUser = await CreateTestUserAsync();
        var request = new AssignRoleRequest("Registered");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{targetUser.UserId}/assign-role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AssignRole_WithoutUsersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        var targetUser = await CreateTestUserAsync();
        var request = new AssignRoleRequest("Registered");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{targetUser.UserId}/assign-role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveRole_WithUsersWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:write");
        var targetUser = await CreateTestUserAsync();
        var request = new RemoveRoleRequest("Registered");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{targetUser.UserId}/remove-role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveRole_WithoutUsersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        var targetUser = await CreateTestUserAsync();
        var request = new RemoveRoleRequest("Registered");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{targetUser.UserId}/remove-role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_WithUsersWritePermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:write");
        var targetUser = await CreateTestUserAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/users/{targetUser.UserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_WithoutUsersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        var targetUser = await CreateTestUserAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/users/{targetUser.UserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region admin:access permission tests

    [Fact]
    public async Task GetAllUsers_WithAdminAccessPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("admin:access");

        // Act
        var response = await HttpClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllUsers_WithoutAdminAccessPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission

        // Act
        var response = await HttpClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserRoles_WithAdminAccessPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("admin:access");
        var targetUser = await CreateTestUserAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{targetUser.UserId}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserRoles_WithoutAdminAccessPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        var targetUser = await CreateTestUserAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{targetUser.UserId}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserPermissions_WithAdminAccessPermission_ShouldReturn200()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("admin:access");
        var targetUser = await CreateTestUserAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{targetUser.UserId}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserPermissions_WithoutAdminAccessPermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        var targetUser = await CreateTestUserAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{targetUser.UserId}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper methods

    private async Task<(Guid UserId, string Email)> CreateTestUserAsync()
    {
        var userId = Guid.NewGuid();
        var email = $"testuser+{Guid.NewGuid():N}@test.com";
        
        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO users (id, identity_id, first_name, last_name, email, created_at)
            VALUES (@Id, @IdentityId, @FirstName, @LastName, @Email, now())",
            new
            {
                Id = userId,
                IdentityId = Guid.NewGuid().ToString(),
                FirstName = "Test",
                LastName = "User",
                Email = email
            });

        return (userId, email);
    }

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
        // This is a simplified mapping - in reality you'd query the role_permissions table
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