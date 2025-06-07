using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class AssignDistributorTest : BaseIntegrationTest
    {
        public AssignDistributorTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task AssignDistributorToPointOfSaleCommand_Should_Return_Success_When_Valid_Data()
        {
            // 1) Sembrar el distribuidor y obtener su ID
            var distributorId = await DistributorData.SeedAsync(Sender);

            // 2) Sembrar el punto de venta y obtener su ID
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // 3) Crear el comando para asignar el distribuidor al punto de venta
            var command = new AssignDistributorToPointOfSaleCommand(
                PointOfSaleData.PhoneNumber,
                DistributorData.PhoneNumber,
                Category.LACTEOS
            );

            // 4) Ejecutar el comando
            var result = await Sender.Send(command);

            // 5) Verificar que el resultado sea exitoso
            Assert.False(result.IsFailure);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task AssignDistributorToPointOfSaleCommand_Should_Return_Failure_When_Already_Assigned()
        {
            // 1) Usar datos únicos para evitar conflictos con otros tests
            var (distributorId, distributorPhone) = await DistributorData.SeedUniqueAsync(Sender, "DupTest");
            var (pointOfSaleId, posPhone) = await PointOfSaleData.SeedUniqueAsync(Sender, "DupTest");

            // 2) Crear el comando para asignar el distribuidor al punto de venta
            var command = new AssignDistributorToPointOfSaleCommand(
                posPhone,
                distributorPhone,
                Category.LACTEOS
            );

            // 3) Ejecutar el comando por primera vez (debe ser exitoso)
            var firstResult = await Sender.Send(command);
            Assert.False(firstResult.IsFailure);
            Assert.True(firstResult.Value);

            // 4) Ejecutar el comando por segunda vez (debe fallar porque ya está asignado)
            var secondResult = await Sender.Send(command);
            Assert.True(secondResult.IsFailure);
        }
    }
}