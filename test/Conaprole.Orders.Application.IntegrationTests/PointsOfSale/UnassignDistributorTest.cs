using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.UnassignDistributor;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class UnassignDistributorTest : BaseIntegrationTest
    {
        public UnassignDistributorTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task UnassignDistributorFromPointOfSaleCommand_WithValidData_Returns_Success()
        {
            // Use unique phone numbers for this test
            var distributorPhone = "+59811111111";
            var pointOfSalePhone = "+59811111112";

            // 1) Sembrar un distribuidor
            var distributorId = await DistributorData.SeedAsync(Sender, distributorPhone);

            // 2) Sembrar un punto de venta
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender, pointOfSalePhone);

            // 3) Asignar el distribuidor al punto de venta primero
            var assignCommand = new AssignDistributorToPointOfSaleCommand(
                pointOfSalePhone,
                distributorPhone,
                Category.LACTEOS
            );
            
            var assignResult = await Sender.Send(assignCommand);
            Assert.False(assignResult.IsFailure, $"Assignment failed: {assignResult.Error?.Code}");
            Assert.True(assignResult.Value);

            // 4) Ejecutar el comando de desasignación (el test principal)
            var unassignCommand = new UnassignDistributorFromPointOfSaleCommand(
                pointOfSalePhone,
                distributorPhone,
                Category.LACTEOS
            );

            var unassignResult = await Sender.Send(unassignCommand);

            // 5) Verificar que el resultado sea exitoso
            Assert.False(unassignResult.IsFailure, $"Unassignment failed: {unassignResult.Error?.Code}");
            Assert.True(unassignResult.Value);
        }

        [Fact]
        public async Task UnassignDistributorFromPointOfSaleCommand_WithNonExistentPointOfSale_Returns_Failure()
        {
            // Use unique phone numbers for this test
            var distributorPhone = "+59822222221";

            // 1) Sembrar un distribuidor
            var distributorId = await DistributorData.SeedAsync(Sender, distributorPhone);

            // 2) Intentar desasignar de un punto de venta que no existe
            var unassignCommand = new UnassignDistributorFromPointOfSaleCommand(
                "+59899999999", // Número de teléfono que no existe
                distributorPhone,
                Category.LACTEOS
            );

            var unassignResult = await Sender.Send(unassignCommand);

            // 3) Verificar que el resultado sea una falla
            Assert.True(unassignResult.IsFailure);
        }

        [Fact]
        public async Task UnassignDistributorFromPointOfSaleCommand_WithNonExistentDistributor_Returns_Failure()
        {
            // Use unique phone numbers for this test
            var pointOfSalePhone = "+59833333332";

            // 1) Sembrar un punto de venta
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender, pointOfSalePhone);

            // 2) Intentar desasignar un distribuidor que no existe
            var unassignCommand = new UnassignDistributorFromPointOfSaleCommand(
                pointOfSalePhone,
                "+59888888888", // Número de teléfono que no existe
                Category.LACTEOS
            );

            var unassignResult = await Sender.Send(unassignCommand);

            // 3) Verificar que el resultado sea una falla
            Assert.True(unassignResult.IsFailure);
        }
    }
}