using Conaprole.Orders.Application.Orders.AddOrderLine;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Domain.Products;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class AddOrderLineTest : BaseIntegrationTest
    {
        public AddOrderLineTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithValidData_Should_AddLineSuccessfully()
        {
            // Arrange
            // 1. Crear una orden vacía
            var orderId = await OrderData.SeedEmptyOrderAsync(Sender);
            
            // 2. Crear un producto para agregar
            var productId = await ProductData.SeedAsync(Sender);
            
            // 3. Preparar el comando
            var command = new AddOrderLineToOrderCommand(
                orderId,
                new ExternalProductId(ProductData.ExternalProductId),
                2
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.False(result.IsFailure);
            Assert.NotEqual(Guid.Empty, result.Value);
        }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithNonExistentOrder_Should_ReturnFailure()
        {
            // Arrange
            var nonExistentOrderId = Guid.NewGuid();
            var productId = await ProductData.SeedAsync(Sender);
            
            var command = new AddOrderLineToOrderCommand(
                nonExistentOrderId,
                new ExternalProductId(ProductData.ExternalProductId),
                1
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithNonExistentProduct_Should_ReturnFailure()
        {
            // Arrange
            var orderId = await OrderData.SeedEmptyOrderAsync(Sender);
            var nonExistentProductId = "NON_EXISTENT_PRODUCT";
            
            var command = new AddOrderLineToOrderCommand(
                orderId,
                new ExternalProductId(nonExistentProductId),
                1
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithDuplicateProduct_Should_ReturnFailure()
        {
            // Arrange
            // 1. Crear una orden con una línea de producto
            var orderId = await OrderData.SeedAsync(Sender);
            
            // 2. Intentar agregar el mismo producto otra vez
            var command = new AddOrderLineToOrderCommand(
                orderId,
                new ExternalProductId(ProductData.ExternalProductId),
                3
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
        }
    }
}