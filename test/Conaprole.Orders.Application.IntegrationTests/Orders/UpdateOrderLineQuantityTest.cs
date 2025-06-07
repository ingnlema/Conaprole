using Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class UpdateOrderLineQuantityTest : BaseIntegrationTest
    {
        public UpdateOrderLineQuantityTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task UpdateOrderLineQuantity_Should_Update_Line_Quantity_Successfully()
        {
            // Arrange - Seed necessary data

            // 1) Create a product
            var productId = await ProductData.SeedAsync(Sender);

            // 2) Create a distributor
            var distributorId = await DistributorData.SeedAsync(Sender);

            // 3) Create a point of sale
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // 4) Create an order with one order line
            var orderLines = new List<Conaprole.Orders.Application.Orders.CreateOrder.CreateOrderLineCommand>
            {
                OrderData.CreateOrderLine(ProductData.ExternalProductId, 2)
            };

            var orderId = await OrderData.SeedAsync(
                Sender,
                PointOfSaleData.PhoneNumber,
                DistributorData.PhoneNumber,
                orderLines);

            // 5) Get the order to retrieve the order line ID
            var getOrderResult = await Sender.Send(new GetOrderQuery(orderId));
            Assert.False(getOrderResult.IsFailure);
            
            var order = getOrderResult.Value;
            Assert.Single(order.OrderLines);
            
            var orderLine = order.OrderLines.First();
            Assert.Equal(2, orderLine.Quantity); // Initial quantity

            // Act - Update the order line quantity
            var newQuantity = 5;
            var updateCommand = new UpdateOrderLineQuantityCommand(
                orderId,
                orderLine.Id,
                newQuantity);

            var updateResult = await Sender.Send(updateCommand);

            // Assert - Verify the update was successful
            Assert.False(updateResult.IsFailure);
            Assert.Equal(orderLine.Id, updateResult.Value);

            // Verify the quantity was actually updated
            var getUpdatedOrderResult = await Sender.Send(new GetOrderQuery(orderId));
            Assert.False(getUpdatedOrderResult.IsFailure);
            
            var updatedOrder = getUpdatedOrderResult.Value;
            var updatedOrderLine = updatedOrder.OrderLines.First(ol => ol.Id == orderLine.Id);
            Assert.Equal(newQuantity, updatedOrderLine.Quantity);
        }
    }
}