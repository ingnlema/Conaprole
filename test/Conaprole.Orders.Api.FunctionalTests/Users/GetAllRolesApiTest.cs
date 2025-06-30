using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.GetAllRoles;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class GetAllRolesApiTest : BaseFunctionalTest
{
    public GetAllRolesApiTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetRoles_ShouldReturnAllRoles_WhenCalled()
    {
        // Arrange
        await SetAuthorizationHeaderAsync();
        
        // Act
        var response = await HttpClient.GetAsync("/api/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await response.Content.ReadFromJsonAsync<List<RoleResponse>>();
        
        roles.Should().NotBeNull();
        roles.Should().NotBeEmpty();
        
        // Verify default roles exist
        roles.Should().Contain(r => r.Name == "Registered");
        roles.Should().Contain(r => r.Name == "API");
        roles.Should().Contain(r => r.Name == "Administrator");
        roles.Should().Contain(r => r.Name == "Distributor");
    }
}