using Conaprole.Orders.Application.Distributors.CreateDistributor;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class CreateDistributorTest : BaseIntegrationTest, IAsyncLifetime
    {
        public CreateDistributorTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        public async Task InitializeAsync()
        {
            // Clean distributor data to ensure clean test state
            await DbContext.Set<Conaprole.Orders.Domain.Distributors.Distributor>()
                .ExecuteDeleteAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task CreateDistributorCommand_WithValidData_ShouldReturnSuccessAndGuid()
        {
            // Arrange
            var command = new CreateDistributorCommand(
                "Distribuidor Test Nuevo",
                "+59899123456",
                "Calle Nueva 456", 
                new List<Category> { Category.LACTEOS, Category.CONGELADOS }
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            Assert.NotEqual(Guid.Empty, result.Value);
        }

        [Fact]
        public async Task CreateDistributorCommand_WithDuplicatePhoneNumber_ShouldReturnFailure()
        {
            // Arrange - Create first distributor
            var firstCommand = new CreateDistributorCommand(
                "Primer Distribuidor",
                "+59899987654",
                "Calle Primera 123",
                new List<Category> { Category.LACTEOS }
            );
            
            var firstResult = await Sender.Send(firstCommand);
            Assert.False(firstResult.IsFailure);

            // Arrange - Try to create second distributor with same phone number
            var duplicateCommand = new CreateDistributorCommand(
                "Segundo Distribuidor",
                "+59899987654", // Same phone number
                "Calle Segunda 789",
                new List<Category> { Category.CONGELADOS }
            );

            // Act
            var duplicateResult = await Sender.Send(duplicateCommand);

            // Assert
            Assert.True(duplicateResult.IsFailure);
        }

        [Fact]
        public async Task CreateDistributorCommand_WithValidData_ShouldStoreCorrectData()
        {
            // Arrange
            const string expectedName = "Distribuidor Verificacion";
            const string expectedPhone = "+59899555444";
            const string expectedAddress = "Calle Verificacion 999";
            var expectedCategories = new List<Category> { Category.SUBPRODUCTOS };

            var command = new CreateDistributorCommand(
                expectedName,
                expectedPhone,
                expectedAddress,
                expectedCategories
            );

            // Act
            var createResult = await Sender.Send(command);
            Assert.False(createResult.IsFailure);

            // Verify the data was stored correctly by querying it back
            var getCommand = new Conaprole.Orders.Application.Distributors.GetDistributors.GetDistributorsQuery();
            var getResult = await Sender.Send(getCommand);
            
            Assert.False(getResult.IsFailure);
            var distributors = getResult.Value;
            var createdDistributor = distributors.FirstOrDefault(d => d.Id == createResult.Value);

            // Assert
            Assert.NotNull(createdDistributor);
            Assert.Equal(expectedName, createdDistributor.Name);
            Assert.Equal(expectedPhone, createdDistributor.PhoneNumber);
            Assert.Equal(expectedAddress, createdDistributor.Address);
            Assert.Equal(expectedCategories.Count, createdDistributor.SupportedCategories.Count);
            Assert.Contains(Category.SUBPRODUCTOS.ToString(), createdDistributor.SupportedCategories);
        }
    }
}