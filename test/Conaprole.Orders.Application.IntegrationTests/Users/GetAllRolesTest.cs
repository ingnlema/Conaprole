using Conaprole.Orders.Application.Users.GetAllRoles;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class GetAllRolesTest : BaseIntegrationTest
{
    public GetAllRolesTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetAllRolesQuery_Should_ReturnAllRoles()
    {
        // Act
        var result = await Sender.Send(new GetAllRolesQuery());

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var roles = result.Value;
        Assert.NotEmpty(roles);
        
        // Verify that default roles exist (from domain configuration)
        Assert.Contains(roles, r => r.Name == "Registered");
        Assert.Contains(roles, r => r.Name == "API");
        Assert.Contains(roles, r => r.Name == "Administrator");
        Assert.Contains(roles, r => r.Name == "Distributor");
        
        // Verify roles are ordered by ID
        var roleIds = roles.Select(r => r.Id).ToList();
        Assert.Equal(roleIds.OrderBy(id => id), roleIds);
    }
}