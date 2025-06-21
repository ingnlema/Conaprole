using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Shared;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class DeleteUserApiTest : BaseFunctionalTest
{
    public DeleteUserApiTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUser_WhenUserExistsAndCallerIsAdmin()
    {
        // Arrange - Create an admin user
        await CreateAndAuthenticateAdminUserAsync();
        
        // Create a test user to delete
        var email = $"deleteuser+{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(email, "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the user was deleted by trying to get their roles (should return BadRequest)
        var rolesResponse = await HttpClient.GetAsync($"/api/users/{userId}/roles");
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenUserNotFoundAndCallerIsAdmin()
    {
        // Arrange - Create an admin user
        await CreateAndAuthenticateAdminUserAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnUnauthorized_WhenCallerIsNotAuthenticated()
    {
        // Arrange - Create a test user (but don't authenticate)
        var email = $"deleteuser+{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(email, "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act (no authentication header set)
        var response = await HttpClient.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnForbidden_WhenCallerIsNotAdmin()
    {
        // Arrange - Create and authenticate as a regular user (not admin)
        await CreateAndAuthenticateRegularUserAsync();
        
        // Create a test user to try to delete
        var email = $"deleteuser+{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(email, "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_ShouldDeleteUserWithAssignedRoles_WhenUserHasMultipleRoles()
    {
        // Arrange - Create an admin user
        await CreateAndAuthenticateAdminUserAsync();
        
        // Create a test user and assign additional roles
        var email = $"deleteuserroles+{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(email, "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Assign Administrator role
        var assignRoleRequest = new AssignRoleRequest("Administrator");
        var assignResponse = await HttpClient.PostAsJsonAsync($"/api/users/{userId}/assign-role", assignRoleRequest);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify roles were assigned
        var rolesResponse = await HttpClient.GetAsync($"/api/users/{userId}/roles");
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Delete the user
        var deleteResponse = await HttpClient.DeleteAsync($"/api/users/{userId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the user was completely deleted
        var verifyRolesResponse = await HttpClient.GetAsync($"/api/users/{userId}/roles");
        verifyRolesResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task CreateAndAuthenticateAdminUserAsync()
    {
        // Create an admin user
        var adminEmail = $"admin+{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(adminEmail, "Admin", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var adminUserId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Assign Administrator role
        var assignRoleRequest = new AssignRoleRequest("Administrator");
        var assignResponse = await HttpClient.PostAsJsonAsync($"/api/users/{adminUserId}/assign-role", assignRoleRequest);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Login to get access token
        var loginRequest = new LogInUserRequest(adminEmail, "12345");
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }

    private async Task CreateAndAuthenticateRegularUserAsync()
    {
        // Create a regular user
        var userEmail = $"regular+{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(userEmail, "Regular", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Login to get access token (no special roles assigned)
        var loginRequest = new LogInUserRequest(userEmail, "12345");
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);
    }
}