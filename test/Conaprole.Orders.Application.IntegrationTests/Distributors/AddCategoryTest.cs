using Conaprole.Orders.Application.Distributors.AddCategory;
using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class AddCategoryTest : BaseIntegrationTest
    {
        public AddCategoryTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task AddDistributorCategoryCommand_WithValidData_ShouldSucceed()
        {
            // Arrange
            var uniquePhoneNumber = "+59899887701"; // Unique phone number for this test
            var createDistributorCommand = new CreateDistributorCommand(
                DistributorData.Name,
                uniquePhoneNumber,
                DistributorData.Address,
                DistributorData.DefaultCategories
            );
            var createResult = await Sender.Send(createDistributorCommand);
            Assert.False(createResult.IsFailure);

            var command = new AddDistributorCategoryCommand(
                uniquePhoneNumber,
                Category.CONGELADOS
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task AddDistributorCategoryCommand_WithAlreadyAssignedCategory_ShouldFail()
        {
            // Arrange
            var uniquePhoneNumber = "+59899887702"; // Unique phone number for this test
            var createDistributorCommand = new CreateDistributorCommand(
                DistributorData.Name,
                uniquePhoneNumber,
                DistributorData.Address,
                DistributorData.DefaultCategories
            );
            var createResult = await Sender.Send(createDistributorCommand);
            Assert.False(createResult.IsFailure);

            var command = new AddDistributorCategoryCommand(
                uniquePhoneNumber,
                Category.LACTEOS // This category is already assigned by default in DistributorData
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(DistributorErrors.CategoryAlreadyAssigned.Code, result.Error.Code);
        }

        [Fact]
        public async Task AddDistributorCategoryCommand_WithNonExistentDistributor_ShouldFail()
        {
            // Arrange
            var command = new AddDistributorCategoryCommand(
                "+59999999999", // Non-existent phone number
                Category.CONGELADOS
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(DistributorErrors.NotFound.Code, result.Error.Code);
        }
    }
}