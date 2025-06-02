using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.GetLoggedInUser;
using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Domain.Shared;
using Dapper;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class GetLoggedInUserTest : BaseFunctionalTest
{
    public GetLoggedInUserTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLoggedInUser_ShouldReturnUserWithRoles_WhenUserHasNoDistributor()
    {
        // Arrange - Register a new user
        var registerRequest = new RegisterUserRequest("getme@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Login to get access token
        var loginRequest = new LogInUserRequest("getme@test.com", "12345");
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        
        user.Should().NotBeNull();
        user!.Email.Should().Be("getme@test.com");
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.Roles.Should().NotBeEmpty();
        user.Roles.Should().Contain("Registered");
        user.DistributorId.Should().BeNull();
        user.DistributorPhoneNumber.Should().BeNull();
    }

    [Fact]
    public async Task GetLoggedInUser_ShouldReturnUserWithDistributor_WhenUserHasDistributor()
    {
        // Arrange - Create a distributor first
        var distributorId = await CreateDistributorAsync("+59898765432");

        // Register a new user
        var registerRequest = new RegisterUserRequest("distributor@test.com", "Distributor", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        // Associate the user with the distributor
        await AssociateUserWithDistributorAsync(userId, distributorId);

        // Login to get access token
        var loginRequest = new LogInUserRequest("distributor@test.com", "12345");
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        
        HttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act
        var response = await HttpClient.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        
        user.Should().NotBeNull();
        user!.Email.Should().Be("distributor@test.com");
        user.FirstName.Should().Be("Distributor");
        user.LastName.Should().Be("User");
        user.Roles.Should().NotBeEmpty();
        user.Roles.Should().Contain("Registered");
        user.DistributorId.Should().Be(distributorId);
        user.DistributorPhoneNumber.Should().Be("+59898765432");
    }

    private async Task AssociateUserWithDistributorAsync(Guid userId, Guid distributorId)
    {
        const string sql = @"UPDATE users SET distributor_id = @DistributorId WHERE id = @UserId";

        using var connection = SqlConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            DistributorId = distributorId
        });
    }
}