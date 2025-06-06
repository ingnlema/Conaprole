using Conaprole.Orders.Application.Users.LoginUser;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class LoginUserTest : BaseIntegrationTest
{
    public LoginUserTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task LoginUser_ShouldReturnAccessToken_WhenValidCredentials()
    {
        // 1) Sembrar el usuario y obtener su ID y email
        var (userId, userEmail) = await UserData.SeedAsync(Sender);

        // 2) Ejecutar el comando de login
        var loginCommand = new LogInUserCommand(userEmail, UserData.Password);
        var loginResult = await Sender.Send(loginCommand);

        // 3) Verificar que el resultado sea exitoso
        Assert.False(loginResult.IsFailure);

        // 4) Verificar que se retorne un access token válido
        var accessTokenResponse = loginResult.Value;
        Assert.NotNull(accessTokenResponse);
        Assert.NotNull(accessTokenResponse.AccessToken);
        Assert.NotEmpty(accessTokenResponse.AccessToken);
    }

    [Fact]
    public async Task LoginUser_ShouldReturnFailure_WhenInvalidCredentials()
    {
        // 1) Sembrar el usuario
        var (userId, userEmail) = await UserData.SeedAsync(Sender);

        // 2) Intentar login con contraseña incorrecta
        var loginCommand = new LogInUserCommand(userEmail, "wrongpassword");
        var loginResult = await Sender.Send(loginCommand);

        // 3) Verificar que el resultado sea fallido
        Assert.True(loginResult.IsFailure);
        Assert.Equal("User.InvalidCredentials", loginResult.Error.Code);
    }

    [Fact]
    public async Task LoginUser_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // 1) Intentar login con usuario que no existe
        var loginCommand = new LogInUserCommand("nonexistent@test.com", "anypassword");
        var loginResult = await Sender.Send(loginCommand);

        // 2) Verificar que el resultado sea fallido
        Assert.True(loginResult.IsFailure);
        Assert.Equal("User.InvalidCredentials", loginResult.Error.Code);
    }
}