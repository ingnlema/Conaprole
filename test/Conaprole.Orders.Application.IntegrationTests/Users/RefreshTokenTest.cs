using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Orders.Application.Users.RefreshToken;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class RefreshTokenTest : BaseIntegrationTest, IAsyncLifetime
{
    public RefreshTokenTest(IntegrationTestWebAppFactory factory)
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
    public async Task RefreshTokenCommand_Should_ReturnNewAccessToken_WhenRefreshTokenIsValid()
    {
        // Arrange - Create a user and get a valid refresh token
        var (email, userId) = await UserData.SeedWithKnownEmailAsync(Sender);
        
        var loginCommand = new LogInUserCommand(email, UserData.Password);
        var loginResult = await Sender.Send(loginCommand);
        
        Assert.False(loginResult.IsFailure);
        Assert.NotNull(loginResult.Value);
        Assert.NotEmpty(loginResult.Value.RefreshToken);

        var refreshCommand = new RefreshTokenCommand(loginResult.Value.RefreshToken);

        // Act
        var refreshResult = await Sender.Send(refreshCommand);

        // Assert
        Assert.False(refreshResult.IsFailure);
        Assert.NotNull(refreshResult.Value);
        Assert.NotEmpty(refreshResult.Value.AccessToken);
        Assert.NotEmpty(refreshResult.Value.RefreshToken);
        
        // The new access token should be different from the original
        Assert.NotEqual(loginResult.Value.AccessToken, refreshResult.Value.AccessToken);
    }

    [Fact]
    public async Task RefreshTokenCommand_Should_Fail_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var invalidRefreshToken = "invalid-refresh-token";
        var command = new RefreshTokenCommand(invalidRefreshToken);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RefreshTokenCommand_Should_Fail_WhenRefreshTokenIsEmpty()
    {
        // Arrange
        var command = new RefreshTokenCommand(string.Empty);

        // Act
        var result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsFailure);
    }
}