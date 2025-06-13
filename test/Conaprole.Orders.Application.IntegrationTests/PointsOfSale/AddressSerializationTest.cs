using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class AddressSerializationTest : BaseIntegrationTest
    {
        public AddressSerializationTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetActivePointsOfSaleQuery_Should_Return_Address_As_Structured_Object()
        {
            // Arrange - Create a point of sale with known address data
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // Act - Execute the query
            var queryResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            
            // Assert - Verify the response structure
            Assert.False(queryResult.IsFailure);
            Assert.NotNull(queryResult.Value);
            
            var pointOfSale = queryResult.Value.First(pos => pos.Id == pointOfSaleId);
            
            // Verify the address is now a structured object, not a string
            Assert.NotNull(pointOfSale.Address);
            Assert.IsType<Address>(pointOfSale.Address);
            
            // Verify individual address components are accessible
            Assert.Equal(PointOfSaleData.City, pointOfSale.Address.City);
            Assert.Equal(PointOfSaleData.Street, pointOfSale.Address.Street);
            Assert.Equal(PointOfSaleData.ZipCode, pointOfSale.Address.ZipCode);
            
            // Verify the address components are not empty
            Assert.NotEmpty(pointOfSale.Address.City);
            Assert.NotEmpty(pointOfSale.Address.Street);
            Assert.NotEmpty(pointOfSale.Address.ZipCode);
        }
    }
}