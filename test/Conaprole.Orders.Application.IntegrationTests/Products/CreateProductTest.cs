using Conaprole.Orders.Application.Products.CreateProduct;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

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
            var command = ProductData.CreateCommand;

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
            var command = ProductData.CreateCommand;

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            var productId = result.Value;
            Assert.NotEqual(Guid.Empty, productId);

            // Verify the product ID is a valid GUID
            Assert.True(productId.GetType() == typeof(Guid));
        }
    }
}