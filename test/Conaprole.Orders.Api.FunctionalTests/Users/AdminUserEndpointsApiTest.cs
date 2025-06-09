using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.GetAllUsers;
using Conaprole.Orders.Application.Users.GetUserRoles;
using Conaprole.Orders.Application.Users.GetUserPermissions;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class AdminUserEndpointsApiTest : BaseFunctionalTest
{
    public AdminUserEndpointsApiTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnUsers_WhenCalled()
    {
        // Arrange - Create a test user first
        var registerRequest = new RegisterUserRequest("admintest@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await HttpClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserSummaryResponse>>();
        
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
        
        // Should contain our test user
        users.Should().Contain(u => u.Email == "admintest@test.com");
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnFilteredUsers_WhenRoleFilterProvided()
    {
        // Arrange - Create a test user first
        var registerRequest = new RegisterUserRequest("rolefilter@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Filter by Registered role
        var response = await HttpClient.GetAsync("/api/users?roleFilter=Registered");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserSummaryResponse>>();
        
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
        
        // All users should have the Registered role
        users.Should().AllSatisfy(u => u.Roles.Should().Contain("Registered"));
    }

    [Fact]
    public async Task GetUserRoles_ShouldReturnUserRoles_WhenValidUserId()
    {
        // Arrange - Create a test user first
        var registerRequest = new RegisterUserRequest("userroles@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{registerResult}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await response.Content.ReadFromJsonAsync<List<Conaprole.Orders.Application.Users.GetUserRoles.RoleResponse>>();
        
        roles.Should().NotBeNull();
        roles.Should().NotBeEmpty();
        roles.Should().Contain(r => r.Name == "Registered");
    }

    [Fact]
    public async Task GetUserPermissions_ShouldReturnUserPermissions_WhenValidUserId()
    {
        // Arrange - Create a test user first
        var registerRequest = new RegisterUserRequest("userpermissions@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await HttpClient.GetAsync($"/api/users/{registerResult}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var permissions = await response.Content.ReadFromJsonAsync<List<Conaprole.Orders.Application.Users.GetUserPermissions.PermissionResponse>>();
        
        permissions.Should().NotBeNull();
        // Note: May be empty if Registered role has no permissions assigned
    }

    [Fact]
    public async Task GetUserRoles_ShouldReturnBadRequest_WhenInvalidUserId()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/users/{Guid.NewGuid()}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserPermissions_ShouldReturnBadRequest_WhenInvalidUserId()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/users/{Guid.NewGuid()}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}