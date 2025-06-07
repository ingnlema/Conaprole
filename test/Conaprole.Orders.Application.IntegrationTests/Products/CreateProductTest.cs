using Conaprole.Orders.Application.Products.CreateProduct;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.IntegrationTests.Products
{
    [Collection("IntegrationCollection")]
    public class CreateProductTest : BaseIntegrationTest
    {
        public CreateProductTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task CreateProductCommand_Creates_Product_Successfully()
        {
            // Arrange
            var uniqueExternalId = $"TEST_PRODUCT_{Guid.NewGuid()}";
            var command = new CreateProductCommand(
                uniqueExternalId,
                "Test Product",
                100m,
                "UYU",
                "Test product for integration tests",
                Category.LACTEOS
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            Assert.True(result.Value != Guid.Empty);
        }

        [Fact]
        public async Task CreateProductCommand_Returns_Valid_ProductId()
        {
            // Arrange
            var uniqueExternalId = $"TEST_PRODUCT_{Guid.NewGuid()}";
            var command = new CreateProductCommand(
                uniqueExternalId,
                "Test Product 2",
                150m,
                "UYU",
                "Another test product",
                Category.CONGELADOS
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            var productId = result.Value;
            Assert.NotEqual(Guid.Empty, productId);

            // Verify the product ID is a valid GUID
            Assert.True(productId.GetType() == typeof(Guid));
        }

        [Fact]
        public async Task CreateProductCommand_Fails_For_Duplicate_ExternalId()
        {
            // Arrange
            var duplicateExternalId = $"DUPLICATE_TEST_{Guid.NewGuid()}";
            var command1 = new CreateProductCommand(
                duplicateExternalId,
                "First Product",
                100m,
                "UYU",
                "First product",
                Category.LACTEOS
            );
            var command2 = new CreateProductCommand(
                duplicateExternalId,
                "Second Product",
                200m,
                "UYU",
                "Second product with same external ID",
                Category.CONGELADOS
            );

            // Act
            var result1 = await Sender.Send(command1);
            var result2 = await Sender.Send(command2);

            // Assert
            Assert.False(result1.IsFailure);
            Assert.True(result2.IsFailure);
        }
    }
}