using Conaprole.Orders.Application.Users.GetAllPermissions;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class GetAllPermissionsTest : BaseIntegrationTest
{
    public GetAllPermissionsTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetAllPermissionsQuery_Should_ReturnAllPermissions()
    {
        // Act
        var result = await Sender.Send(new GetAllPermissionsQuery());

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var permissions = result.Value;
        Assert.NotEmpty(permissions);
        
        // Verify that default permissions exist (from domain configuration)
        Assert.Contains(permissions, p => p.Name == "users:read");
        Assert.Contains(permissions, p => p.Name == "users:write");
        Assert.Contains(permissions, p => p.Name == "admin:access");
        
        // Verify permissions are ordered by ID
        var permissionIds = permissions.Select(p => p.Id).ToList();
        Assert.Equal(permissionIds.OrderBy(id => id), permissionIds);
    }
}