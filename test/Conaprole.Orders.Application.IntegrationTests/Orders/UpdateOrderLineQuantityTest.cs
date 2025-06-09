using Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class UpdateOrderLineQuantityTest : BaseIntegrationTest
    {
        public UpdateOrderLineQuantityTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task UpdateOrderLineQuantity_ShouldUpdateQuantitySuccessfully()
        {
            // 1) Seed order and obtain its ID
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Get the order to obtain the order line ID
            var getOrderQuery = new GetOrderQuery(orderId);
            var getOrderResult = await Sender.Send(getOrderQuery);

            Assert.False(getOrderResult.IsFailure);
            var order = getOrderResult.Value;
            Assert.NotEmpty(order.OrderLines);

            var orderLine = order.OrderLines.First();
            var originalQuantity = orderLine.Quantity;
            var newQuantity = originalQuantity + 3; // Change quantity from original

            // 3) Execute the command to update the order line quantity
            var updateCommand = new UpdateOrderLineQuantityCommand(orderId, orderLine.Id, newQuantity);
            var updateResult = await Sender.Send(updateCommand);

            // 4) Verify that the command was successful
            Assert.False(updateResult.IsFailure);
            Assert.Equal(orderLine.Id, updateResult.Value); // Command returns the OrderLineId

            // 5) Verify that the quantity was actually updated by getting the order again
            var getUpdatedOrderResult = await Sender.Send(getOrderQuery);

            Assert.False(getUpdatedOrderResult.IsFailure);
            var updatedOrder = getUpdatedOrderResult.Value;
            var updatedOrderLine = updatedOrder.OrderLines.First(ol => ol.Id == orderLine.Id);
            
            Assert.Equal(newQuantity, updatedOrderLine.Quantity);
            Assert.NotEqual(originalQuantity, updatedOrderLine.Quantity);
        }
    }
}