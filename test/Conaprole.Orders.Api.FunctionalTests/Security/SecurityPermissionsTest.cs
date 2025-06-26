using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Users;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Security;

[Collection("ApiCollection")]
public class SecurityPermissionsTest : BaseFunctionalTest
{
    public SecurityPermissionsTest(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PublicEndpoints_ShouldAllowAnonymousAccess()
    {
        // Arrange & Act - Test public endpoints without authentication
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/users/login", new
        {
            Email = "invalid@test.com",
            Password = "invalidpassword"
        });

        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", new
        {
            Email = "newuser@test.com",
            FirstName = "Test",
            LastName = "User",
            Password = "ValidPassword123",
            DistributorPhoneNumber = (string?)null
        });

        // Assert - Public endpoints should be accessible (but may return business logic errors)
        loginResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        registerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProtectedEndpoints_ShouldRequireAuthentication()
    {
        // Arrange & Act - Test protected endpoints without authentication
        var permissionsResponse = await HttpClient.GetAsync("/api/permissions");
        var rolesResponse = await HttpClient.GetAsync("/api/roles");
        var usersResponse = await HttpClient.GetAsync("/api/users");
        var distributorsResponse = await HttpClient.GetAsync("/api/distributors");
        var productsResponse = await HttpClient.GetAsync("/api/products");
        var ordersResponse = await HttpClient.GetAsync("/api/orders");
        var posResponse = await HttpClient.GetAsync("/api/pos");

        // Assert - All protected endpoints should return Unauthorized
        permissionsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        usersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        distributorsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        productsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ordersResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        posResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthenticatedEndpoints_ShouldWorkWithValidToken()
    {
        // Arrange - Use the test user that was already set up with proper permissions in InitializeAsync()
        await SetAuthorizationHeaderAsync();

        // Act - Test with valid authentication
        var userMeResponse = await HttpClient.GetAsync("/api/users/me");

        // Assert - Should work with proper authentication
        userMeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}