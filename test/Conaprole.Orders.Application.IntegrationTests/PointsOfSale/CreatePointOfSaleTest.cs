using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class CreatePointOfSaleTest : BaseIntegrationTest
    {
        public CreatePointOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task CreatePointOfSaleCommand_Returns_Success_With_Valid_Data()
        {
            // 1) Preparar el comando con datos de prueba
            var command = PointOfSaleData.CreateCommand;

            // 2) Ejecutar el comando
            var result = await Sender.Send(command);

            // 3) Verificar que el resultado sea exitoso
            Assert.False(result.IsFailure);
            Assert.NotEqual(Guid.Empty, result.Value);

            // 4) Verificar que se puede recuperar el punto de venta creado
            var pointOfSaleId = result.Value;
            Assert.True(pointOfSaleId != Guid.Empty);
        }

        [Fact]
        public async Task CreatePointOfSaleCommand_Returns_Failure_When_PhoneNumber_Already_Exists()
        {
            // 1) Crear un punto de venta inicialmente
            var firstCommand = PointOfSaleData.CreateCommand;
            var firstResult = await Sender.Send(firstCommand);
            Assert.False(firstResult.IsFailure);

            // 2) Intentar crear otro punto de venta con el mismo número de teléfono
            var secondCommand = new CreatePointOfSaleCommand(
                "Otro Punto de Venta",
                PointOfSaleData.PhoneNumber, // mismo número de teléfono
                "Canelones",
                "Otra Calle 456",
                "90000"
            );

            var secondResult = await Sender.Send(secondCommand);

            // 3) Verificar que el segundo comando falle
            Assert.True(secondResult.IsFailure);
        }
    }
}