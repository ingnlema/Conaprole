using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Application.Users.AssignRole;
using Conaprole.Orders.Application.Users.RemoveRole;
using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

public class RemoveRoleTest : BaseIntegrationTest
{
    public RemoveRoleTest(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Handle_Should_RemoveRoleFromUser_WhenValidRequest()
    {
        // Arrange - Create a user via RegisterUserCommand and assign Administrator role
        var userId = await UserData.SeedAsync(Sender);
        
        // First assign the Administrator role
        var assignCommand = new AssignRoleCommand(userId, "Administrator");
        await Sender.Send(assignCommand);

        var command = new RemoveRoleCommand(userId, "Administrator");

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify the role was removed - fetch roles from database to avoid static instances
        var updatedUser = await DbContext.Set<User>()
            .Include(u => u.Roles)
            .FirstAsync(u => u.Id == userId);
        
        Assert.NotNull(updatedUser);
        Assert.DoesNotContain(updatedUser.Roles, r => r.Name == "Administrator");
        Assert.Contains(updatedUser.Roles, r => r.Name == "Registered"); // Should still have the default role
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new RemoveRoleCommand(Guid.NewGuid(), "Administrator");

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenRoleNotFound()
    {
        // Arrange - Create a user via RegisterUserCommand
        var userId = await UserData.SeedAsync(Sender);

        var command = new RemoveRoleCommand(userId, "InvalidRole");

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Role.NotFound", result.Error.Code);
    }
}