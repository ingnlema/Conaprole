using System.Data;
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
    public async Task ChangePassword_WithUsersWritePermission_ShouldReturn204OrBadRequest()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:write");
        var targetUser = await CreateTestUserAsync();
        var request = new ChangePasswordRequest("newpassword123");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{targetUser.UserId}/change-password", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WithoutUsersWritePermission_ShouldReturn403()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("users:read"); // Different permission
        var targetUser = await CreateTestUserAsync();
        var request = new ChangePasswordRequest("newpassword123");

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{targetUser.UserId}/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_WithAdminAccessPermission_ShouldReturn204()
    {
        // Arrange
        await CreateUserWithPermissionAndSetAuthAsync("admin:access");
        var targetUser = await CreateTestUserAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/users/{targetUser.UserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_WithoutAdminAccessPermission_ShouldReturn403()
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
        var email = $"testuser+{Guid.NewGuid():N}@test.com";
        var password = "TestPassword123";

        // Register user properly with Keycloak using the same method as the working tests
        var registerRequest = new RegisterUserRequest(email, "Test", "User", password);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        
        if (!registerResponse.IsSuccessStatusCode)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register test user: {error}");
        }

        using var connection = SqlConnectionFactory.CreateConnection();
        
        // Get the created user ID from the database
        var userId = await connection.QuerySingleAsync<Guid>(@"
            SELECT id FROM users WHERE email = @Email", 
            new { Email = email });

        return (userId, email);
    }

    private async Task CreateUserWithPermissionAndSetAuthAsync(string permission, bool useExistingRole = false)
    {
        await AuthorizationTestHelper.CreateUserWithPermissionAndSetAuthAsync(
            HttpClient, SqlConnectionFactory, permission, useExistingRole);
    }

    #endregion
}