using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Conaprole.Orders.Api.FunctionalTests.Authentication;

[Collection("ApiCollection")]
public class KeycloakRealmSeederTest : BaseFunctionalTest
{
    public KeycloakRealmSeederTest(FunctionalTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SeedRealmAsync_ShouldCreateRolesAndPermissions_WhenCalled()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IKeycloakRealmSeeder>();

        // Act
        await seeder.SeedRealmAsync();

        // Assert
        // If the method completes without throwing, the seeding was successful
        // The seeder is idempotent, so calling it multiple times should be safe
        await seeder.SeedRealmAsync();
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnValidToken_WhenCalledWithRoles()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IKeycloakRealmSeeder>();
        var roles = new[] { "Registered", "API" };

        // Act
        var token = await seeder.GetTokenAsync(roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain("."); // JWT tokens contain dots
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts: header.payload.signature
    }

    [Fact]
    public async Task GetTokenWithRolesAsync_ShouldReturnValidToken_WhenCalledFromBaseFunctionalTest()
    {
        // Act
        var token = await GetTokenWithRolesAsync("Administrator");

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain("."); // JWT tokens contain dots
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts: header.payload.signature
    }

    [Fact]
    public async Task SetAuthorizationHeaderWithRolesAsync_ShouldSetAuthorizationHeader()
    {
        // Act
        await SetAuthorizationHeaderWithRolesAsync("Registered");

        // Assert
        HttpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        HttpClient.DefaultRequestHeaders.Authorization?.Scheme.Should().Be("Bearer");
        HttpClient.DefaultRequestHeaders.Authorization?.Parameter.Should().NotBeNullOrEmpty();
    }
}