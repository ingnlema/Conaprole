using Conaprole.Orders.Application.Orders.AddOrderLine;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.Products.CreateProduct;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class AddOrderLineTest : BaseIntegrationTest
    {
        public AddOrderLineTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithValidData_Returns_Success()
        {
            // 1) Seed order with default product
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Create a different product to add to the order
            var secondProductCommand = new CreateProductCommand(
                "SECOND_PRODUCT",
                "Second Product",
                150m,
                "UYU",
                "Second product for testing",
                Category.LACTEOS
            );
            var secondProductResult = await Sender.Send(secondProductCommand);
            Assert.False(secondProductResult.IsFailure);

            // 3) Create and execute the command
            var command = new AddOrderLineToOrderCommand(
                orderId,
                new ExternalProductId("SECOND_PRODUCT"),
                3
            );

            var result = await Sender.Send(command);

            // 4) Verify that the command was successful
            Assert.False(result.IsFailure);
            Assert.NotEqual(Guid.Empty, result.Value); // Should return the new order line ID

            // 5) Verify that the order line was actually added by getting the order
            var getOrderQuery = new GetOrderQuery(orderId);
            var getOrderResult = await Sender.Send(getOrderQuery);

            Assert.False(getOrderResult.IsFailure);
            var order = getOrderResult.Value;
            Assert.Equal(2, order.OrderLines.Count); // Should now have 2 order lines
        }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithDuplicateProduct_Returns_Failure()
        {
            // 1) Seed order with default product
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Try to add the same product that's already in the order
            var command = new AddOrderLineToOrderCommand(
                orderId,
                new ExternalProductId(ProductData.ExternalProductId), // Same as seeded product
                2
            );

            var result = await Sender.Send(command);

            // 3) Verify that the command failed with the correct error
            Assert.True(result.IsFailure);
            Assert.Equal("Order.DuplicateProduct", result.Error.Code);
        }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithInvalidOrderId_Returns_Failure()
        {
            // 1) Use a non-existent order ID
            var nonExistentOrderId = Guid.NewGuid();

            // 2) Create and execute the command
            var command = new AddOrderLineToOrderCommand(
                nonExistentOrderId,
                new ExternalProductId(ProductData.ExternalProductId),
                2
            );

            var result = await Sender.Send(command);

            // 3) Verify that the command failed with the correct error
            Assert.True(result.IsFailure);
            Assert.Equal("Order.NotFound", result.Error.Code);
        }

        [Fact]
        public async Task AddOrderLineToOrderCommand_WithInvalidProduct_Returns_Failure()
        {
            // 1) Seed order
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Create command with non-existent product
            var command = new AddOrderLineToOrderCommand(
                orderId,
                new ExternalProductId("INEXISTENT_PRODUCT"),
                2
            );

            var result = await Sender.Send(command);

            // 3) Verify that the command failed with the correct error
            Assert.True(result.IsFailure);
            Assert.Equal("Order.ProductNotFound", result.Error.Code);
        }
    }
}