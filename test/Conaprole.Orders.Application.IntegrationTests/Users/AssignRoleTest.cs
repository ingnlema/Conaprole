using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Application.Users.AssignRole;
using Conaprole.Orders.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

public class AssignRoleTest : BaseIntegrationTest
{
    public AssignRoleTest(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Handle_Should_AssignRoleToUser_WhenValidRequest()
    {
        // Arrange - Create a user via RegisterUserCommand
        var userId = await UserData.SeedAsync(Sender);

        var command = new AssignRoleCommand(userId, "Administrator");

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify the role was assigned - fetch roles from database to avoid static instances
        var updatedUser = await DbContext.Set<User>()
            .Include(u => u.Roles)
            .FirstAsync(u => u.Id == userId);
        
        Assert.NotNull(updatedUser);
        Assert.Contains(updatedUser.Roles, r => r.Name == "Administrator");
        Assert.Contains(updatedUser.Roles, r => r.Name == "Registered"); // Should still have the default role
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new AssignRoleCommand(Guid.NewGuid(), "Administrator");

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

        var command = new AssignRoleCommand(userId, "InvalidRole");

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Role.NotFound", result.Error.Code);
    }
}