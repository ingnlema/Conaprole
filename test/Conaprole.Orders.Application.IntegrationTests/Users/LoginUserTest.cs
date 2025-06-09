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
        
        // Clean up any existing test users before each test
        var testEmails = new[] { UserData.Email, UserData.AlternativeEmail };
        // Use alternative approach to avoid LINQ translation issues with Email.Value
        var existingUsers = new List<User>();
        foreach (var email in testEmails)
        {
            var user = await DbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Email.Value == email);
            if (user != null)
                existingUsers.Add(user);
        }
        
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
    public async Task LoginUserCommand_Should_ReturnAccessToken_WhenCredentialsAreValid()
    {
        // Arrange - Create a user first
        await UserData.SeedAsync(Sender);
        
        var command = new LogInUserCommand(
            UserData.Email,
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
        await UserData.SeedAsync(Sender);
        
        var command = new LogInUserCommand(
            UserData.Email,
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
        var command = new LogInUserCommand(
            "nonexistent@example.com",
            UserData.Password);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
    }
}