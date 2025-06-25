using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Users;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;
[Collection("ApiCollection")]
public class LoginUserTest : BaseFunctionalTest
{
    private const string Email = "login@test.com";
    private const string Password = "12345";
    
    public LoginUserTest(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_ShouldReturnUnathorized_WhenUserDoesNotExists()
    {
        var request = new LogInUserRequest(Email, Password);
        
        var response = await HttpClient.PostAsJsonAsync("/api/users/login", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    }
    
    [Fact]
    public async Task Login_ShouldReturnOk_WhenUserExists()
    {
        var registerRequest = new RegisterUserRequest(Email, "first", "last", Password);
    

        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
    

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        Debug.WriteLine("Register Response Content: " + registerContent);

        registerContent.Should().NotBeNullOrEmpty("porque se espera que el registro devuelva información del usuario creado");
    
        var loginRequest = new LogInUserRequest(Email, Password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
    
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        Debug.WriteLine("Login Response Content: " + loginContent);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserIsDeleted()
    {
        // Arrange - Create and register user
        var email = $"deleteduser+{Guid.NewGuid():N}@test.com";
        var password = "12345";
        
        var registerRequest = new RegisterUserRequest(email, "Deleted", "User", password);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Create admin user to delete the original user
        await CreateAndAuthenticateAdminUserAsync();
        
        // Delete the user
        var deleteResponse = await HttpClient.DeleteAsync($"/api/users/{userId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Clear authentication header
        HttpClient.DefaultRequestHeaders.Authorization = null;
        
        // Act - Try to login with deleted user
        var loginRequest = new LogInUserRequest(email, password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        
        // Assert - Should return 401 Unauthorized, not 500 Internal Server Error
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task CreateAndAuthenticateAdminUserAsync()
    {
        // Use the base class method to create admin user
        await SetAdminAuthorizationHeaderAsync();
    }
}