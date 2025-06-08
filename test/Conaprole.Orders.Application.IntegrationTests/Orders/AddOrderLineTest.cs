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
            // 1. Crear una orden con un producto inicial
            var initialOrderData = await OrderData.SeedAsync(Sender);
            
            // 2. Crear un producto diferente para agregar
            var uniqueExternalId = $"PRODUCT_{Guid.NewGuid():N}";
            var productId = await ProductData.SeedWithExternalIdAsync(Sender, uniqueExternalId);
            
            // 3. Preparar el comando para agregar el segundo producto
            var command = new AddOrderLineToOrderCommand(
                initialOrderData.OrderId,
                new ExternalProductId(uniqueExternalId),
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
            var uniqueExternalId = $"PRODUCT_{Guid.NewGuid():N}";
            var productId = await ProductData.SeedWithExternalIdAsync(Sender, uniqueExternalId);
            
            var command = new AddOrderLineToOrderCommand(
                nonExistentOrderId,
                new ExternalProductId(uniqueExternalId),
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
            // 1. Crear una orden con un producto inicial
            var initialOrderData = await OrderData.SeedAsync(Sender);
            var nonExistentProductId = $"NON_EXISTENT_{Guid.NewGuid():N}";
            
            var command = new AddOrderLineToOrderCommand(
                initialOrderData.OrderId,
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
            var orderData = await OrderData.SeedAsync(Sender);
            
            // 2. Intentar agregar el mismo producto otra vez
            var command = new AddOrderLineToOrderCommand(
                orderData.OrderId,
                new ExternalProductId(orderData.ProductExternalId),
                3
            );

            // Act
            var result = await Sender.Send(command);

            // Assert
            Assert.True(result.IsFailure);
        }
    }
}