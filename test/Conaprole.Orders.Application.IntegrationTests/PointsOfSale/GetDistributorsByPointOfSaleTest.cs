using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.CreatePointOfSale;
using Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class GetDistributorsByPointOfSaleTest : BaseIntegrationTest
    {
        public GetDistributorsByPointOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetDistributorsByPointOfSaleQuery_WithNoAssignedDistributors_Returns_EmptyList()
        {
            // 1) Create a point of sale but don't assign any distributors
            const string pointOfSalePhone = "+59891234580"; // Unique phone number for this test
            
            var pointOfSaleCommand = new CreatePointOfSaleCommand(
                "Punto de Venta Test Sin Distribuidores",
                pointOfSalePhone,
                "Montevideo",
                "Calle Test 123",
                "11000"
            );
            var pointOfSaleResult = await Sender.Send(pointOfSaleCommand);
            Assert.False(pointOfSaleResult.IsFailure);

            // 2) Execute the query
            var query = new GetDistributorsByPointOfSaleQuery(pointOfSalePhone);
            var queryResult = await Sender.Send(query);

            // 3) Verify result
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Empty(distributors);
        }

        [Fact]
        public async Task GetDistributorsByPointOfSaleQuery_WithAssignedDistributors_Returns_DistributorsWithCategories()
        {
            // 1) Create test data with unique phone numbers
            const string pointOfSalePhone = "+59891234581"; // Unique phone number for this test
            const string distributorPhone1 = "+59899887781"; // Unique phone number for distributor 1
            const string distributorPhone2 = "+59899887782"; // Unique phone number for distributor 2

            // 2) Create point of sale
            var pointOfSaleCommand = new CreatePointOfSaleCommand(
                "Punto de Venta Test Con Distribuidores",
                pointOfSalePhone,
                "Montevideo",
                "Avenida Test 456",
                "11001"
            );
            var pointOfSaleResult = await Sender.Send(pointOfSaleCommand);
            Assert.False(pointOfSaleResult.IsFailure);

            // 3) Create first distributor with LACTEOS category
            var distributor1Command = new CreateDistributorCommand(
                "Distribuidor Test 1",
                distributorPhone1,
                "Calle Falsa 123",
                new List<Category> { Category.LACTEOS }
            );
            var distributor1Result = await Sender.Send(distributor1Command);
            Assert.False(distributor1Result.IsFailure);

            // 4) Create second distributor with CONGELADOS category
            var distributor2Command = new CreateDistributorCommand(
                "Distribuidor Test 2",
                distributorPhone2,
                "Calle Falsa 456",
                new List<Category> { Category.CONGELADOS }
            );
            var distributor2Result = await Sender.Send(distributor2Command);
            Assert.False(distributor2Result.IsFailure);

            // 5) Assign first distributor to point of sale
            var assign1Command = new AssignDistributorToPointOfSaleCommand(
                pointOfSalePhone,
                distributorPhone1,
                Category.LACTEOS
            );
            var assign1Result = await Sender.Send(assign1Command);
            Assert.False(assign1Result.IsFailure);
            Assert.True(assign1Result.Value);

            // 6) Assign second distributor to point of sale
            var assign2Command = new AssignDistributorToPointOfSaleCommand(
                pointOfSalePhone,
                distributorPhone2,
                Category.CONGELADOS
            );
            var assign2Result = await Sender.Send(assign2Command);
            Assert.False(assign2Result.IsFailure);
            Assert.True(assign2Result.Value);

            // 7) Execute the query
            var query = new GetDistributorsByPointOfSaleQuery(pointOfSalePhone);
            var queryResult = await Sender.Send(query);

            // 8) Verify result
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Equal(2, distributors.Count);

            // 9) Verify first distributor
            var distributor1 = distributors.First(d => d.PhoneNumber == distributorPhone1);
            Assert.NotNull(distributor1);
            Assert.Equal("Distribuidor Test 1", distributor1.Name);
            Assert.Equal(distributorPhone1, distributor1.PhoneNumber);
            Assert.Single(distributor1.Categories);
            Assert.Contains("LACTEOS", distributor1.Categories);

            // 10) Verify second distributor
            var distributor2 = distributors.First(d => d.PhoneNumber == distributorPhone2);
            Assert.NotNull(distributor2);
            Assert.Equal("Distribuidor Test 2", distributor2.Name);
            Assert.Equal(distributorPhone2, distributor2.PhoneNumber);
            Assert.Single(distributor2.Categories);
            Assert.Contains("CONGELADOS", distributor2.Categories);
        }

        [Fact]
        public async Task GetDistributorsByPointOfSaleQuery_WithNonExistentPointOfSale_Returns_EmptyList()
        {
            // 1) Query for non-existent point of sale
            const string nonExistentPhone = "+59891111111";
            var query = new GetDistributorsByPointOfSaleQuery(nonExistentPhone);
            var queryResult = await Sender.Send(query);

            // 2) Verify result
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Empty(distributors);
        }
    }
}