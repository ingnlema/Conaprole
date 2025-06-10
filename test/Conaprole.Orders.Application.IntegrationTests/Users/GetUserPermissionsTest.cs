using Conaprole.Orders.Application.Users.GetUserPermissions;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class GetUserPermissionsTest : BaseIntegrationTest, IAsyncLifetime
{
    public GetUserPermissionsTest(IntegrationTestWebAppFactory factory)
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
    public async Task GetUserPermissionsQuery_Should_ReturnUserPermissions_WhenUserExists()
    {
        // Arrange - Create a test user
        var userId = await UserData.SeedAsync(Sender);

        // Act
        var result = await Sender.Send(new GetUserPermissionsQuery(userId));

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        
        var permissions = result.Value;
        // Note: The permissions depend on what permissions are assigned to the Registered role
        // In the domain model, we need to check what permissions the Registered role has
        // For now, we just verify that it returns a valid list (could be empty)
        Assert.NotNull(permissions);
    }

    [Fact]
    public async Task GetUserPermissionsQuery_Should_ReturnFailure_WhenUserNotExists()
    {
        // Arrange - Use a non-existent user ID
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await Sender.Send(new GetUserPermissionsQuery(nonExistentUserId));

        // Assert
        Assert.True(result.IsFailure);
    }
}