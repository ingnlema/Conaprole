using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using User = Conaprole.Orders.Domain.Users.User;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class LoginUserTest : BaseIntegrationTest, IAsyncLifetime
{
    public LoginUserTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    public new async Task InitializeAsync()
    {
        // Call base InitializeAsync to clean database
        await base.InitializeAsync();
        
        // No need to clean up specific users since we're using unique emails
        // Database cleanup is handled by base.InitializeAsync()
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task LoginUserCommand_Should_ReturnAccessToken_WhenCredentialsAreValid()
    {
        // Arrange - Create a user first
        var (email, userId) = await UserData.SeedWithKnownEmailAsync(Sender);
        
        var command = new LogInUserCommand(
            email,
            UserData.Password);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.AccessToken);
    }

    [Fact]
    public async Task LoginUserCommand_Should_Fail_WhenCredentialsAreInvalid()
    {
        // Arrange - Create a user first
        var (email, userId) = await UserData.SeedWithKnownEmailAsync(Sender);
        
        var command = new LogInUserCommand(
            email,
            "WrongPassword123!");

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task LoginUserCommand_Should_Fail_WhenUserDoesNotExist()
    {
        // Arrange - Don't create any user
        var nonExistentEmail = UserData.GenerateUniqueEmail();
        var command = new LogInUserCommand(
            nonExistentEmail,
            UserData.Password);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
    }
}