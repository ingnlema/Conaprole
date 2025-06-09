using Conaprole.Orders.Application.Distributors.GetPointOfSaleDetails;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class GetPointOfSaleDetailsTest : BaseIntegrationTest
    {
        public GetPointOfSaleDetailsTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetPointOfSaleDetailsQuery_Returns_Assigned_PointOfSale()
        {
            // Use unique phone numbers for this test to avoid conflicts
            var distributorPhone = "+59899887701";
            var pointOfSalePhone = "+59891234501";

            // 1) Sembrar el distribuidor con un teléfono único
            var distributorCommand = new Application.Distributors.CreateDistributor.CreateDistributorCommand(
                "Distribuidor Test 1",
                distributorPhone,
                "Calle Falsa 123",
                new() { Domain.Shared.Category.LACTEOS }
            );
            var distributorResult = await Sender.Send(distributorCommand);
            Assert.False(distributorResult.IsFailure);

            // 2) Sembrar el punto de venta con un teléfono único
            var pointOfSaleCommand = new Application.PointsOfSale.CreatePointOfSale.CreatePointOfSaleCommand(
                "POS Test 1",
                pointOfSalePhone,
                "Montevideo",
                "Av. 18 de Julio 1234",
                "11000"
            );
            var pointOfSaleResult = await Sender.Send(pointOfSaleCommand);
            Assert.False(pointOfSaleResult.IsFailure);

            // 3) Asignar el distribuidor al punto de venta
            var assignCommand = new Application.PointsOfSale.AssignDistributor.AssignDistributorToPointOfSaleCommand(
                pointOfSalePhone,
                distributorPhone,
                Domain.Shared.Category.LACTEOS
            );
            var assignResult = await Sender.Send(assignCommand);
            Assert.False(assignResult.IsFailure);

            // 4) Ejecutar el query
            var query = new GetPointOfSaleDetailsQuery(distributorPhone, pointOfSalePhone);
            var queryResult = await Sender.Send(query);

            // 5) Verificar que el resultado sea exitoso
            Assert.False(queryResult.IsFailure);

            // 6) Verificar que los datos retornados coincidan con los sembrados
            var pointOfSaleDetails = queryResult.Value;
            Assert.NotNull(pointOfSaleDetails);
            Assert.Equal("POS Test 1", pointOfSaleDetails.Name);
            Assert.Equal(pointOfSalePhone, pointOfSaleDetails.PhoneNumber);
            Assert.True(pointOfSaleDetails.IsActive);
            Assert.True(pointOfSaleDetails.CreatedAt > DateTime.MinValue);
        }

        [Fact]
        public async Task GetPointOfSaleDetailsQuery_Returns_Failure_When_NotAssigned()
        {
            // Use unique phone numbers for this test to avoid conflicts
            var distributorPhone = "+59899887702";
            var pointOfSalePhone = "+59891234502";

            // 1) Sembrar el distribuidor con un teléfono único
            var distributorCommand = new Application.Distributors.CreateDistributor.CreateDistributorCommand(
                "Distribuidor Test 2",
                distributorPhone,
                "Calle Falsa 456",
                new() { Domain.Shared.Category.LACTEOS }
            );
            var distributorResult = await Sender.Send(distributorCommand);
            Assert.False(distributorResult.IsFailure);

            // 2) Sembrar el punto de venta pero NO asignarlo al distribuidor
            var pointOfSaleCommand = new Application.PointsOfSale.CreatePointOfSale.CreatePointOfSaleCommand(
                "POS Test 2",
                pointOfSalePhone,
                "Montevideo",
                "Av. 8 de Octubre 2345",
                "11200"
            );
            var pointOfSaleResult = await Sender.Send(pointOfSaleCommand);
            Assert.False(pointOfSaleResult.IsFailure);

            // 3) Ejecutar el query (sin asignar el distribuidor)
            var query = new GetPointOfSaleDetailsQuery(distributorPhone, pointOfSalePhone);
            var queryResult = await Sender.Send(query);

            // 4) Verificar que el resultado sea un fallo
            Assert.True(queryResult.IsFailure);
        }
    }
}