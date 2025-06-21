using Conaprole.Orders.Application.Users.DeleteUser;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class DeleteUserTest : BaseIntegrationTest, IAsyncLifetime
{
    public DeleteUserTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public new async Task InitializeAsync()
    {
        // Call base InitializeAsync to clean database
        await base.InitializeAsync();
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task DeleteUserCommand_Should_DeleteUser_Successfully()
    {
        // Arrange - Create a user first
        var userId = await UserData.SeedAsync(Sender);
        
        // Verify user exists
        var userBeforeDelete = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(userBeforeDelete);

        var command = new DeleteUserCommand(userId);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);

        // Verify user was deleted from database
        var userAfterDelete = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.Null(userAfterDelete);
    }

    [Fact]
    public async Task DeleteUserCommand_Should_Fail_WhenUserNotFound()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var command = new DeleteUserCommand(nonExistentUserId);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Found", result.Error.Code);
    }

    [Fact]
    public async Task DeleteUserCommand_Should_DeleteUserWithRoles_Successfully()
    {
        // Arrange - Create a user and assign roles
        var userId = await UserData.SeedAsync(Sender);
        
        // Get the user with roles
        var userWithRoles = await DbContext.Set<User>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(userWithRoles);
        Assert.NotEmpty(userWithRoles.Roles); // Should have at least the Registered role

        var command = new DeleteUserCommand(userId);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);

        // Verify user and their role associations were deleted
        var userAfterDelete = await DbContext.Set<User>()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.Null(userAfterDelete);
    }

    [Fact]
    public async Task DeleteUserCommand_Should_DeleteUserWithDistributor_Successfully()
    {
        // Arrange - First create a distributor then create a user with that distributor
        var distributorId = await DistributorData.SeedAsync(Sender);
        var userId = await UserData.SeedWithDistributorAsync(Sender, DistributorData.PhoneNumber);
        
        // Verify user exists with distributor
        var userWithDistributor = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(userWithDistributor);
        Assert.NotNull(userWithDistributor.DistributorId);

        var command = new DeleteUserCommand(userId);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);

        // Verify user was deleted from database
        var userAfterDelete = await DbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId);
        Assert.Null(userAfterDelete);
    }
}