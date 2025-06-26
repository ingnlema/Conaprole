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
    public async Task DeleteUser_WithUsersWritePermission_ShouldReturn204()
    {
        // Arrange - use existing role since DeleteUser requires both permission AND role membership
        await CreateUserWithPermissionAndSetAuthAsync("users:write", useExistingRole: true);
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

    private async Task CreateUserWithPermissionAndSetAuthAsync(string permission, bool useExistingRole = false)
    {
        await AuthorizationTestHelper.CreateUserWithPermissionAndSetAuthAsync(
            HttpClient, SqlConnectionFactory, permission, useExistingRole);
    }

    #endregion
}