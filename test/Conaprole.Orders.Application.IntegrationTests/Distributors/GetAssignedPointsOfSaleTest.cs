using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Application.Distributors.GetAssignedPointsOfSale;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class GetAssignedPointsOfSaleTest : BaseIntegrationTest
    {
        public GetAssignedPointsOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetAssignedPointsOfSaleQuery_WithNoAssignments_Returns_EmptyList()
        {
            // Create a distributor but don't assign any points of sale
            const string distributorPhone = "+59899887760"; // Unique phone number for this test
            var distributorCommand = new CreateDistributorCommand(
                "Distribuidor Test 1",
                distributorPhone,
                "Calle Falsa 123",
                new List<Category> { Category.LACTEOS }
            );
            var distributorResult = await Sender.Send(distributorCommand);
            Assert.False(distributorResult.IsFailure);

            // Execute the query
            var queryResult = await Sender.Send(new GetAssignedPointsOfSaleQuery(distributorPhone));

            // Verify result
            Assert.False(queryResult.IsFailure);
            var pointsOfSale = queryResult.Value;
            Assert.NotNull(pointsOfSale);
            Assert.Empty(pointsOfSale);
        }

        [Fact]
        public async Task GetAssignedPointsOfSaleQuery_WithAssignedPointOfSale_Returns_AssignedPointOfSale()
        {
            // 1) Create test data with unique phone numbers
            const string distributorPhone = "+59899887761"; // Unique phone number for this test
            const string pointOfSalePhone = "+59891234561"; // Unique phone number for this test

            var distributorCommand = new CreateDistributorCommand(
                "Distribuidor Test 2",
                distributorPhone,
                "Calle Falsa 456",
                new List<Category> { Category.LACTEOS }
            );
            var distributorResult = await Sender.Send(distributorCommand);
            Assert.False(distributorResult.IsFailure);
            var distributorId = distributorResult.Value;

            var pointOfSaleCommand = new CreatePointOfSaleCommand(
                "Punto de Venta Test 2",
                pointOfSalePhone,
                "Montevideo",
                "Avenida Test 456",
                "11001"
            );
            var pointOfSaleResult = await Sender.Send(pointOfSaleCommand);
            Assert.False(pointOfSaleResult.IsFailure);
            var pointOfSaleId = pointOfSaleResult.Value;

            // 2) Assign the distributor to the point of sale
            var assignCommand = new AssignDistributorToPointOfSaleCommand(
                pointOfSalePhone,
                distributorPhone,
                Category.LACTEOS
            );
            var assignResult = await Sender.Send(assignCommand);
            Assert.False(assignResult.IsFailure);
            Assert.True(assignResult.Value);

            // 3) Execute the query
            var queryResult = await Sender.Send(new GetAssignedPointsOfSaleQuery(distributorPhone));

            // 4) Verify result
            Assert.False(queryResult.IsFailure);
            var pointsOfSale = queryResult.Value;
            Assert.NotNull(pointsOfSale);
            Assert.Single(pointsOfSale);

            var pointOfSale = pointsOfSale.First();
            Assert.Equal(pointOfSaleId, pointOfSale.Id);
            Assert.Equal("Punto de Venta Test 2", pointOfSale.Name);
            Assert.Equal(pointOfSalePhone, pointOfSale.PhoneNumber);
            Assert.NotNull(pointOfSale.Address);
            Assert.True(pointOfSale.IsActive);
            Assert.True(pointOfSale.CreatedAt > DateTime.MinValue);
        }
    }
}