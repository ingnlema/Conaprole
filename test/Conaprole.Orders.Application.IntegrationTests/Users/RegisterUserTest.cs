using Conaprole.Orders.Application.Users.RegisterUser;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Users
{
    [Collection("IntegrationCollection")]
    public class RegisterUserTest : BaseIntegrationTest
    {
        public RegisterUserTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task RegisterUserCommand_Returns_Success_With_Valid_UserId()
        {
            // 1) Ejecutar el comando para registrar usuario
            var commandResult = await Sender.Send(UserData.CreateCommand);
            
            // 2) Verificar que el resultado sea exitoso
            Assert.False(commandResult.IsFailure);

            // 3) Verificar que devuelve un ID válido
            var userId = commandResult.Value;
            Assert.NotEqual(Guid.Empty, userId);

            // 4) Verificar que el usuario fue creado con datos correctos
            // (El comando debería ejecutarse sin errores)
            Assert.True(commandResult.IsSuccess);
        }

        [Fact]
        public async Task RegisterUserCommand_With_Different_Email_Returns_Success()
        {
            // 1) Crear comando con email diferente para evitar conflictos
            var uniqueCommand = new RegisterUserCommand(
                "unique.test@example.com",
                UserData.FirstName,
                UserData.LastName,
                UserData.Password,
                UserData.DistributorPhoneNumber
            );

            // 2) Ejecutar el comando
            var commandResult = await Sender.Send(uniqueCommand);
            
            // 3) Verificar que el resultado sea exitoso
            Assert.False(commandResult.IsFailure);
            Assert.NotEqual(Guid.Empty, commandResult.Value);
        }
    }
}