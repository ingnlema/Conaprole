using Conaprole.Orders.Application.Distributors.RemoveCategory;
using Conaprole.Orders.Application.Distributors.GetDistributors;
using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class RemoveCategoryTest : BaseIntegrationTest
    {
        public RemoveCategoryTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task RemoveDistributorCategory_ExistingCategoryAndDistributor_ShouldReturnTrue()
        {
            // Arrange - Create a distributor with LACTEOS category using unique phone number
            var phoneNumber = "+59899123456";  // Unique phone number for this test
            var distributorId = await CreateDistributorWithCategoryAsync(phoneNumber, Category.LACTEOS);
            
            // Act - Remove the LACTEOS category that was assigned during creation
            var removeCommand = new RemoveDistributorCategoryCommand(
                phoneNumber,
                Category.LACTEOS
            );
            var result = await Sender.Send(removeCommand);
            
            // Assert
            Assert.False(result.IsFailure);
            Assert.True(result.Value);
            
            // Verify the category was actually removed by checking distributor state
            var getDistributorQuery = new GetDistributorsQuery();
            var distributorResult = await Sender.Send(getDistributorQuery);
            Assert.False(distributorResult.IsFailure);
            
            var distributor = distributorResult.Value.First(d => d.Id == distributorId);
            Assert.DoesNotContain("LACTEOS", distributor.SupportedCategories);
            Assert.Empty(distributor.SupportedCategories); // Should be empty after removing the only category
        }

        [Fact]
        public async Task RemoveDistributorCategory_CategoryNotAssigned_ShouldReturnFailure()
        {
            // Arrange - Create a distributor with only LACTEOS category
            var phoneNumber = "+59899654321";  // Unique phone number for this test
            await CreateDistributorWithCategoryAsync(phoneNumber, Category.LACTEOS);
            
            // Act - Try to remove a category that's not assigned (SUBPRODUCTOS)
            var command = new RemoveDistributorCategoryCommand(
                phoneNumber,
                Category.SUBPRODUCTOS
            );
            var result = await Sender.Send(command);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Distributor.CategoryNotAssigned", result.Error.Code);
        }

        [Fact]
        public async Task RemoveDistributorCategory_DistributorNotFound_ShouldReturnFailure()
        {
            // Act - Try to remove category from non-existent distributor
            var command = new RemoveDistributorCategoryCommand(
                "+59800000000", // Non-existent phone number
                Category.LACTEOS
            );
            var result = await Sender.Send(command);
            
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Distributor.NotFound", result.Error.Code);
        }

        /// <summary>
        /// Helper method to create a distributor with a specific category for testing purposes.
        /// </summary>
        private async Task<Guid> CreateDistributorWithCategoryAsync(string phoneNumber, Category category)
        {
            var command = new CreateDistributorCommand(
                "Test Distributor",
                phoneNumber,
                "Test Address 123",
                new List<Category> { category }
            );
            
            var result = await Sender.Send(command);
            if (result.IsFailure)
                throw new Exception($"Error creating test distributor: {result.Error.Code}");
                
            return result.Value;
        }
    }
}