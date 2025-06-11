using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.LoginUser;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class RefreshTokenTest : BaseFunctionalTest
{
    private const string Email = "refresh@test.com";
    private const string Password = "12345";
    
    public RefreshTokenTest(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewToken_WhenRefreshTokenIsValid()
    {
        // Arrange - Register and login to get a valid refresh token
        var registerRequest = new RegisterUserRequest(Email, "first", "last", Password);
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginRequest = new LogInUserRequest(Email, Password);
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<AccessTokenResponse>(loginContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        loginResult.Should().NotBeNull();
        loginResult!.RefreshToken.Should().NotBeNullOrEmpty();

        var refreshRequest = new RefreshTokenRequest(loginResult.RefreshToken);

        // Act
        var refreshResponse = await HttpClient.PostAsJsonAsync("/api/users/refresh", refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();
        var refreshResult = JsonSerializer.Deserialize<AccessTokenResponse>(refreshContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        refreshResult.Should().NotBeNull();
        refreshResult!.AccessToken.Should().NotBeNullOrEmpty();
        refreshResult.RefreshToken.Should().NotBeNullOrEmpty();
        
        // The new access token should be different from the original
        refreshResult.AccessToken.Should().NotBe(loginResult.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var request = new RefreshTokenRequest("invalid-refresh-token");
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users/refresh", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenIsEmpty()
    {
        // Arrange
        var request = new RefreshTokenRequest("");
        
        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/users/refresh", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}