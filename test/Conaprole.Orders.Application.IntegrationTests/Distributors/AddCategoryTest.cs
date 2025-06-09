using Conaprole.Orders.Application.Distributors.AddCategory;
using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class AddCategoryTest : BaseIntegrationTest
    {
        public AddCategoryTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task AddDistributorCategoryCommand_WithValidData_ShouldReturnSuccess()
        {
            // Arrange - Create a distributor with default categories (LACTEOS)
            var distributorId = await DistributorData.SeedAsync(Sender);
            var command = new AddDistributorCategoryCommand(
                DistributorData.PhoneNumber,
                Category.CONGELADOS // Adding a different category than the default LACTEOS
            );

            // Act - Send the command to add category
            var result = await Sender.Send(command);

            // Assert - Verify the result is successful
            Assert.False(result.IsFailure);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task AddDistributorCategoryCommand_WithAlreadyAssignedCategory_ShouldReturnFailure()
        {
            // Arrange - Create a distributor with default categories (LACTEOS) using a different phone number
            var phoneNumber = "+59811112222"; // Different phone number for this test
            var command = new AddDistributorCategoryCommand(
                phoneNumber,
                Category.LACTEOS
            );
            
            // Create distributor with LACTEOS category
            var createDistributorCommand = new CreateDistributorCommand(
                DistributorData.Name,
                phoneNumber,
                DistributorData.Address,
                new List<Category> { Category.LACTEOS }
            );
            var distributorResult = await Sender.Send(createDistributorCommand);
            Assert.False(distributorResult.IsFailure);

            // Act - Try to add the same category that's already assigned
            var result = await Sender.Send(command);

            // Assert - Verify the result is a failure
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task AddDistributorCategoryCommand_WithNonExistentDistributor_ShouldReturnFailure()
        {
            // Arrange - Use a phone number that doesn't exist
            var command = new AddDistributorCategoryCommand(
                "+59899999999", // Non-existent phone number
                Category.CONGELADOS
            );

            // Act - Send the command to add category
            var result = await Sender.Send(command);

            // Assert - Verify the result is a failure
            Assert.True(result.IsFailure);
        }
    }
}