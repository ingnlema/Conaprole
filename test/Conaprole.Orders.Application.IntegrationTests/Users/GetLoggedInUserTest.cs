using Conaprole.Orders.Application.Users.GetLoggedInUser;
using Conaprole.Orders.Application.Abstractions.Data;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Conaprole.Orders.Application.IntegrationTests.Users;

[Collection("IntegrationCollection")]
public class GetLoggedInUserTest : BaseIntegrationTest
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetLoggedInUserTest(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _sqlConnectionFactory = factory.Services.GetRequiredService<ISqlConnectionFactory>();
    }

    [Fact]
    public async Task GetLoggedInUserQuery_Returns_Seeded_User_With_Roles()
    {
        // 1) Sembrar el usuario y obtener su ID
        var userId = await UserData.SeedAsync(_sqlConnectionFactory);

        // 2) Configurar el contexto del usuario para la prueba
        Factory.TestUserContext.UserId = userId;
        Factory.TestUserContext.IdentityId = UserData.TestIdentityId;

        // 3) Ejecutar el query
        var queryResult = await Sender.Send(new GetLoggedInUserQuery());
        Assert.False(queryResult.IsFailure);

        // 4) Verificar que coincide con nuestros datos de prueba
        var user = queryResult.Value;
        Assert.NotNull(user);
        Assert.Equal(UserData.TestEmail, user.Email);
        Assert.Equal(UserData.TestFirstName, user.FirstName);
        Assert.Equal(UserData.TestLastName, user.LastName);
        Assert.Contains("Registered", user.Roles);
        Assert.Null(user.DistributorId);
        Assert.Null(user.DistributorPhoneNumber);
    }
}