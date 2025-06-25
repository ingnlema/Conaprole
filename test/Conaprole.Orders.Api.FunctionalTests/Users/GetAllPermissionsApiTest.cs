using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.GetAllPermissions;
using Conaprole.Orders.Application.Users.LoginUser;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class GetAllPermissionsApiTest : BaseFunctionalTest
{
    public GetAllPermissionsApiTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetPermissions_ShouldReturnAllPermissions_WhenCalled()
    {
        // Arrange - Authenticate as admin
        await SetAdminAuthorizationHeaderAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var permissions = await response.Content.ReadFromJsonAsync<List<PermissionResponse>>();
        
        permissions.Should().NotBeNull();
        permissions.Should().NotBeEmpty();
        
        // Verify some key permissions exist
        permissions.Should().Contain(p => p.Name == "users:read");
        permissions.Should().Contain(p => p.Name == "users:write");
        permissions.Should().Contain(p => p.Name == "admin:access");
    }
}
}