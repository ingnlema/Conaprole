using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Orders.Domain.Distributors;
using Conaprole.Orders.Domain.Shared;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class CreateDistributorTest : BaseIntegrationTest
    {
        public CreateDistributorTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task CreateDistributorCommand_WithValidData_ShouldCreateDistributorSuccessfully()
        {
            // Arrange
            var command = new CreateDistributorCommand(
                "Test Distributor",
                "+59899123456",
                "Test Address 123",
                new List<Category> { Category.LACTEOS }
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            Assert.NotEqual(Guid.Empty, result.Value);

            // Verify the distributor was actually created in the database
            var distributors = await DbContext.Distributors.FindAsync(result.Value);
            Assert.NotNull(distributors);
            Assert.Equal("Test Distributor", distributors.Name);
            Assert.Equal("+59899123456", distributors.PhoneNumber);
            Assert.Equal("Test Address 123", distributors.Address);
            Assert.Contains(Category.LACTEOS, distributors.SupportedCategories);
        }

        [Fact]
        public async Task CreateDistributorCommand_WithDuplicatePhoneNumber_ShouldReturnFailure()
        {
            // Arrange - Create first distributor
            var firstCommand = new CreateDistributorCommand(
                "First Distributor",
                "+59899123456",
                "First Address 123",
                new List<Category> { Category.LACTEOS }
            );
            var firstResult = await Sender.Send(firstCommand);
            Assert.False(firstResult.IsFailure);

            // Arrange - Try to create second distributor with same phone number
            var secondCommand = new CreateDistributorCommand(
                "Second Distributor",
                "+59899123456", // Same phone number
                "Second Address 456",
                new List<Category> { Category.CONGELADOS }
            );

            // Act
            var secondResult = await Sender.Send(secondCommand);

            // Assert
            Assert.True(secondResult.IsFailure);
            Assert.Equal(DistributorErrors.AlreadyExists.Code, secondResult.Error.Code);
            Assert.Equal(DistributorErrors.AlreadyExists.Name, secondResult.Error.Name);
        }

        [Fact]
        public async Task CreateDistributorCommand_WithDifferentPhoneNumbers_ShouldCreateBothSuccessfully()
        {
            // Arrange
            var firstCommand = new CreateDistributorCommand(
                "First Distributor",
                "+59899111111",
                "First Address 123",
                new List<Category> { Category.LACTEOS }
            );

            var secondCommand = new CreateDistributorCommand(
                "Second Distributor",
                "+59899222222", // Different phone number
                "Second Address 456",
                new List<Category> { Category.CONGELADOS }
            );

            // Act
            var firstResult = await Sender.Send(firstCommand);
            var secondResult = await Sender.Send(secondCommand);

            // Assert
            Assert.False(firstResult.IsFailure);
            Assert.False(secondResult.IsFailure);
            Assert.NotEqual(firstResult.Value, secondResult.Value);

            // Verify both distributors exist in database
            var firstDistributor = await DbContext.Distributors.FindAsync(firstResult.Value);
            var secondDistributor = await DbContext.Distributors.FindAsync(secondResult.Value);
            
            Assert.NotNull(firstDistributor);
            Assert.NotNull(secondDistributor);
            Assert.Equal("+59899111111", firstDistributor.PhoneNumber);
            Assert.Equal("+59899222222", secondDistributor.PhoneNumber);
        }
    }
}