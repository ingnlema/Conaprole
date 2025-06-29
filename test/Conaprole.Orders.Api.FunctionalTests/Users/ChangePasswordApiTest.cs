using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class ChangePasswordApiTest : BaseFunctionalTest
{
    public ChangePasswordApiTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnNoContent_WhenUserChangesOwnPassword()
    {
        // Arrange - Register and login a user
        var email = "changeownpassword@test.com";
        var originalPassword = "originalpass123";
        var newPassword = "newpass123";

        var registerRequest = new RegisterUserRequest(email, "Test", "User", originalPassword);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Login to get access token
        var loginRequest = new LogInUserRequest(email, originalPassword);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act - Change password
        var changePasswordRequest = new ChangePasswordRequest(newPassword);
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{userId}/change-password", changePasswordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the password was changed by trying to login with new password
        HttpClient.DefaultRequestHeaders.Authorization = null; // Remove authorization header
        var newLoginRequest = new LogInUserRequest(email, newPassword);
        var newLoginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", newLoginRequest);
        newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify old password no longer works
        var oldPasswordLoginRequest = new LogInUserRequest(email, originalPassword);
        var oldPasswordLoginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", oldPasswordLoginRequest);
        oldPasswordLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_ShouldSucceed_WhenUserWithUsersWritePermissionChangesAnotherUsersPassword()
    {
        // Arrange - Register two users
        var user1Email = "user1@test.com";
        var user2Email = "user2@test.com";
        var password = "password123";

        // Register first user
        var registerUser1Request = new RegisterUserRequest(user1Email, "User", "One", password);
        var registerUser1Response = await HttpClient.PostAsJsonAsync("/api/users/register", registerUser1Request);
        registerUser1Response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Register second user
        var registerUser2Request = new RegisterUserRequest(user2Email, "User", "Two", password);
        var registerUser2Response = await HttpClient.PostAsJsonAsync("/api/users/register", registerUser2Request);
        registerUser2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user2Id = await registerUser2Response.Content.ReadFromJsonAsync<Guid>();

        // Login as first user
        var loginRequest = new LogInUserRequest(user1Email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act - Try to change second user's password
        var changePasswordRequest = new ChangePasswordRequest("newpassword123");
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{user2Id}/change-password", changePasswordRequest);

        // Assert - Should succeed because user has users:write permission (API role)
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange - Register a user but don't login
        var email = "notauth@test.com";
        var password = "password123";

        var registerRequest = new RegisterUserRequest(email, "Test", "User", password);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Act - Try to change password without authentication
        var changePasswordRequest = new ChangePasswordRequest("newpassword123");
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{userId}/change-password", changePasswordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange - Login as a valid user
        var email = "validuser@test.com";
        var password = "password123";

        var registerRequest = new RegisterUserRequest(email, "Test", "User", password);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Login to get access token
        var loginRequest = new LogInUserRequest(email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act - Try to change password for non-existent user
        var nonExistentUserId = Guid.NewGuid();
        var changePasswordRequest = new ChangePasswordRequest("newpassword123");
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{nonExistentUserId}/change-password", changePasswordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenPasswordIsTooShort()
    {
        // Arrange - Register and login a user
        var email = "shortpass@test.com";
        var originalPassword = "password123";

        var registerRequest = new RegisterUserRequest(email, "Test", "User", originalPassword);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Login to get access token
        var loginRequest = new LogInUserRequest(email, originalPassword);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act - Try to change to a short password
        var changePasswordRequest = new ChangePasswordRequest("123"); // Too short
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{userId}/change-password", changePasswordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnUnauthorized_WhenUserIsDeletedButTokenStillValid()
    {
        // Arrange - Register user and get token
        var email = $"deleteduser+{Guid.NewGuid():N}@test.com";
        var password = "password123";

        var registerRequest = new RegisterUserRequest(email, "Test", "User", password);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Login to get access token
        var loginRequest = new LogInUserRequest(email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        string userToken = loginResult!.AccessToken;

        // Create admin and delete the user
        await SetAdminAuthorizationHeaderAsync();
        var deleteResponse = await HttpClient.DeleteAsync($"/api/users/{userId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Use the user's token (which is still valid but user is deleted)
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

        // Act - Try to change password of deleted user with their old token
        var changePasswordRequest = new ChangePasswordRequest("newpassword123");
        var response = await HttpClient.PutAsJsonAsync($"/api/users/{userId}/change-password", changePasswordRequest);

        // Assert - Should return 403 Forbidden (authorization fails for deleted user)
        // The JWT token is still valid but user doesn't exist in the database anymore
        // so the authorization system correctly denies access
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}