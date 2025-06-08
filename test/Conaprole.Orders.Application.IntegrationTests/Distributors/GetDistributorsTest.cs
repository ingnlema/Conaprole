using Conaprole.Orders.Application.Distributors.GetDistributors;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class GetDistributorsTest : BaseIntegrationTest
    {
        public GetDistributorsTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetDistributorsQuery_WithNoDistributors_Returns_EmptyList()
        {
            // Execute the query
            var queryResult = await Sender.Send(new GetDistributorsQuery());
            
            // Verify result
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Empty(distributors);
        }

        [Fact]
        public async Task GetDistributorsQuery_WithDistributors_Returns_AllDistributors()
        {
            // Create test data
            var distributorId = await DistributorData.SeedAsync(Sender);

            // Execute the query
            var queryResult = await Sender.Send(new GetDistributorsQuery());
            
            // Verify result
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Single(distributors);
            
            var distributor = distributors.First();
            Assert.Equal(distributorId, distributor.Id);
            Assert.Equal(DistributorData.PhoneNumber, distributor.PhoneNumber);
            Assert.Equal(DistributorData.Name, distributor.Name);
            Assert.Equal(DistributorData.Address, distributor.Address);
            Assert.NotNull(distributor.SupportedCategories);
            Assert.Contains("LACTEOS", distributor.SupportedCategories);
            Assert.True(distributor.CreatedAt > DateTime.MinValue);
            Assert.True(distributor.AssignedPointsOfSaleCount >= 0);
        }
    }
}