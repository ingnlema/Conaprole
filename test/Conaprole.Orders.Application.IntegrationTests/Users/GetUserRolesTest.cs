using Conaprole.Orders.Application.Users.GetUserRoles;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class GetUserRolesTest : BaseIntegrationTest, IAsyncLifetime
{
    public GetUserRolesTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public new async Task InitializeAsync()
    {
        // Call base InitializeAsync to clean database
        await base.InitializeAsync();
        
        // Clean up any existing test users before each test
        var testEmails = new[] { UserData.Email, UserData.AlternativeEmail };
        var existingUsers = await DbContext.Set<User>()
            .Where(u => testEmails.Contains(u.Email.Value))
            .ToListAsync();
        
        if (existingUsers.Any())
        {
            DbContext.Set<User>().RemoveRange(existingUsers);
            await DbContext.SaveChangesAsync();
        }
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetUserRolesQuery_Should_ReturnUserRoles_WhenUserExists()
    {
        // Arrange - Create a test user
        var userId = await UserData.SeedAsync(Sender);

        // Act
        var result = await Sender.Send(new GetUserRolesQuery(userId));

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var roles = result.Value;
        Assert.NotEmpty(roles);
        
        // All new users should have the Registered role by default
        Assert.Contains(roles, r => r.Name == "Registered");
    }

    [Fact]
    public async Task GetUserRolesQuery_Should_ReturnFailure_WhenUserNotExists()
    {
        // Arrange - Use a non-existent user ID
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await Sender.Send(new GetUserRolesQuery(nonExistentUserId));

        // Assert
        Assert.True(result.IsFailure);
    }
}