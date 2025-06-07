using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Application.PointsOfSale.AssignDistributor;
using Conaprole.Orders.Application.PointsOfSale.GetDistributorsByPointOfSale;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class GetDistributorsByPointOfSaleTest : BaseIntegrationTest
    {
        public GetDistributorsByPointOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetDistributorsByPointOfSaleQuery_WithNoDistributors_Returns_EmptyList()
        {
            // Arrange - Create a point of sale without any distributors
            var (pointOfSaleId, pointOfSalePhoneNumber) = await PointOfSaleData.SeedAsync(Sender);

            // Act
            var queryResult = await Sender.Send(new GetDistributorsByPointOfSaleQuery(pointOfSalePhoneNumber));
            
            // Assert
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Empty(distributors);
        }

        [Fact]
        public async Task GetDistributorsByPointOfSaleQuery_WithSingleDistributor_Returns_DistributorWithCategories()
        {
            // Arrange - Create point of sale and distributor, then assign them
            var (pointOfSaleId, pointOfSalePhoneNumber) = await PointOfSaleData.SeedAsync(Sender);
            var (distributorId, distributorPhoneNumber) = await DistributorData.SeedAsync(Sender);
            
            // Assign distributor to point of sale
            var assignResult = await Sender.Send(new AssignDistributorToPointOfSaleCommand(
                pointOfSalePhoneNumber,
                distributorPhoneNumber, 
                Category.LACTEOS));
            Assert.False(assignResult.IsFailure);
            Assert.True(assignResult.Value);

            // Act
            var queryResult = await Sender.Send(new GetDistributorsByPointOfSaleQuery(pointOfSalePhoneNumber));
            
            // Assert
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Single(distributors);

            var distributor = distributors.First();
            Assert.Equal(distributorPhoneNumber, distributor.PhoneNumber);
            Assert.Equal(DistributorData.Name, distributor.Name);
            Assert.Single(distributor.Categories);
            Assert.Contains(Category.LACTEOS.ToString(), distributor.Categories);
        }

        [Fact]
        public async Task GetDistributorsByPointOfSaleQuery_WithDistributorMultipleCategories_Returns_DistributorWithAllCategories()
        {
            // Arrange - Create point of sale and distributor that supports multiple categories
            var (pointOfSaleId, pointOfSalePhoneNumber) = await PointOfSaleData.SeedAsync(Sender);
            
            // Create a distributor that supports multiple categories
            var uniquePhoneNumber = $"+59899{DateTime.UtcNow.Ticks % 1000000:D6}";
            var multiCategoryCommand = new CreateDistributorCommand(
                DistributorData.Name,
                uniquePhoneNumber,
                DistributorData.Address,
                new List<Category> { Category.LACTEOS, Category.CONGELADOS }
            );
            
            var distributorResult = await Sender.Send(multiCategoryCommand);
            Assert.False(distributorResult.IsFailure);
            var distributorId = distributorResult.Value;
            
            // Assign distributor to point of sale with multiple categories
            var assignResult1 = await Sender.Send(new AssignDistributorToPointOfSaleCommand(
                pointOfSalePhoneNumber,
                uniquePhoneNumber, 
                Category.LACTEOS));
            Assert.False(assignResult1.IsFailure);
            Assert.True(assignResult1.Value);

            var assignResult2 = await Sender.Send(new AssignDistributorToPointOfSaleCommand(
                pointOfSalePhoneNumber,
                uniquePhoneNumber, 
                Category.CONGELADOS));
            Assert.False(assignResult2.IsFailure);
            Assert.True(assignResult2.Value);

            // Act
            var queryResult = await Sender.Send(new GetDistributorsByPointOfSaleQuery(pointOfSalePhoneNumber));
            
            // Assert
            Assert.False(queryResult.IsFailure);
            var distributors = queryResult.Value;
            Assert.NotNull(distributors);
            Assert.Single(distributors);

            var distributor = distributors.First();
            Assert.Equal(uniquePhoneNumber, distributor.PhoneNumber);
            Assert.Equal(DistributorData.Name, distributor.Name);
            Assert.Equal(2, distributor.Categories.Count);
            Assert.Contains(Category.LACTEOS.ToString(), distributor.Categories);
            Assert.Contains(Category.CONGELADOS.ToString(), distributor.Categories);
        }
    }
}