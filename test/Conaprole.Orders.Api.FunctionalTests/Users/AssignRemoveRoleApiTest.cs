using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Users;
using Conaprole.Orders.Api.Controllers.Users.Dtos;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Application.Users.GetUserRoles;
using FluentAssertions;

namespace Conaprole.Orders.Api.FunctionalTests.Users;

[Collection("ApiCollection")]
public class AssignRemoveRoleApiTest : BaseFunctionalTest
{
    public AssignRemoveRoleApiTest(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AssignRole_ShouldAssignRoleToUser_WhenValidRequest()
    {
        // Arrange - Create a test user first
        var registerRequest = new RegisterUserRequest("roleassign@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
        var assignRoleRequest = new AssignRoleRequest("Administrator");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{userId}/assign-role", assignRoleRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the role was assigned by checking user roles
        var rolesResponse = await HttpClient.GetAsync($"/api/users/{userId}/roles");
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await rolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();
        
        roles.Should().NotBeNull();
        roles.Should().Contain(r => r.Name == "Administrator");
        roles.Should().Contain(r => r.Name == "Registered"); // Should still have the default role
    }

    [Fact]
    public async Task AssignRole_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var assignRoleRequest = new AssignRoleRequest("Administrator");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{Guid.NewGuid()}/assign-role", assignRoleRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignRole_ShouldReturnBadRequest_WhenRoleNotFound()
    {
        // Arrange - Create a test user first
        var registerRequest = new RegisterUserRequest("invalidrole@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
        var assignRoleRequest = new AssignRoleRequest("InvalidRole");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{userId}/assign-role", assignRoleRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveRole_ShouldRemoveRoleFromUser_WhenValidRequest()
    {
        // Arrange - Create a test user and assign a role first
        var registerRequest = new RegisterUserRequest("roleremove@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
        
        // Assign a role first
        var assignRoleRequest = new AssignRoleRequest("Administrator");
        var assignResponse = await HttpClient.PostAsJsonAsync($"/api/users/{userId}/assign-role", assignRoleRequest);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the role was assigned
        var rolesResponse = await HttpClient.GetAsync($"/api/users/{userId}/roles");
        var roles = await rolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();
        roles.Should().Contain(r => r.Name == "Administrator");

        // Act - Remove the role
        var removeRoleRequest = new RemoveRoleRequest("Administrator");
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{userId}/remove-role", removeRoleRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the role was removed by checking user roles
        var updatedRolesResponse = await HttpClient.GetAsync($"/api/users/{userId}/roles");
        updatedRolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedRoles = await updatedRolesResponse.Content.ReadFromJsonAsync<List<RoleResponse>>();
        
        updatedRoles.Should().NotBeNull();
        updatedRoles.Should().NotContain(r => r.Name == "Administrator");
        updatedRoles.Should().Contain(r => r.Name == "Registered"); // Should still have the default role
    }

    [Fact]
    public async Task RemoveRole_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var removeRoleRequest = new RemoveRoleRequest("Administrator");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{Guid.NewGuid()}/remove-role", removeRoleRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveRole_ShouldReturnBadRequest_WhenRoleNotFound()
    {
        // Arrange - Create a test user first
        var registerRequest = new RegisterUserRequest("removeinvalidrole@test.com", "Test", "User", "12345");
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/users/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
        var removeRoleRequest = new RemoveRoleRequest("InvalidRole");

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/users/{userId}/remove-role", removeRoleRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}