using Conaprole.Orders.Application.Orders.RemoveOrderLine;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class RemoveOrderLineTest : BaseIntegrationTest
    {
        public RemoveOrderLineTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task RemoveOrderLine_FromOrderWithMultipleLines_ShouldRemoveSuccessfully()
        {
            // 1) Seed order with multiple lines
            var orderId = await OrderData.SeedOrderWithMultipleLinesAsync(Sender);

            // 2) Get the order to obtain order line IDs
            var getOrderQuery = new GetOrderQuery(orderId);
            var getOrderResult = await Sender.Send(getOrderQuery);

            Assert.False(getOrderResult.IsFailure);
            var order = getOrderResult.Value;
            Assert.Equal(2, order.OrderLines.Count); // Should have 2 lines

            // 3) Get the first order line to remove
            var orderLineToRemove = order.OrderLines.First();
            var remainingOrderLine = order.OrderLines.Last();
            var originalTotalPrice = order.PriceAmount;

            // 4) Execute the command to remove the order line
            var removeCommand = new RemoveOrderLineFromOrderCommand(orderId, orderLineToRemove.Id);
            var removeResult = await Sender.Send(removeCommand);

            // 5) Verify that the command was successful
            Assert.False(removeResult.IsFailure);
            Assert.Equal(orderLineToRemove.Id, removeResult.Value); // Command returns the removed OrderLineId

            // 6) Verify that the order line was actually removed by getting the order again
            var getUpdatedOrderResult = await Sender.Send(getOrderQuery);

            Assert.False(getUpdatedOrderResult.IsFailure);
            var updatedOrder = getUpdatedOrderResult.Value;
            
            // 7) Verify the order now has only one line
            Assert.Single(updatedOrder.OrderLines);
            Assert.Equal(remainingOrderLine.Id, updatedOrder.OrderLines.First().Id);
            
            // 8) Verify the order price was updated correctly
            Assert.True(updatedOrder.PriceAmount < originalTotalPrice);
            Assert.Equal(remainingOrderLine.SubTotal, updatedOrder.PriceAmount);
        }

        [Fact]
        public async Task RemoveOrderLine_LastLineFromOrder_ShouldReturnFailure()
        {
            // 1) Seed order with single line
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Get the order to obtain the single order line ID
            var getOrderQuery = new GetOrderQuery(orderId);
            var getOrderResult = await Sender.Send(getOrderQuery);

            Assert.False(getOrderResult.IsFailure);
            var order = getOrderResult.Value;
            Assert.Single(order.OrderLines); // Should have only 1 line

            var singleOrderLine = order.OrderLines.First();

            // 3) Execute the command to remove the last order line
            var removeCommand = new RemoveOrderLineFromOrderCommand(orderId, singleOrderLine.Id);
            var removeResult = await Sender.Send(removeCommand);

            // 4) Verify that the command failed with the expected error
            Assert.True(removeResult.IsFailure);
            Assert.Equal("OrderLine.LastLine", removeResult.Error.Code);
            Assert.Contains("Cannot remove the last order line", removeResult.Error.Name);

            // 5) Verify that the order still has the line
            var getOrderAfterFailedRemoveResult = await Sender.Send(getOrderQuery);
            
            Assert.False(getOrderAfterFailedRemoveResult.IsFailure);
            var orderAfterFailedRemove = getOrderAfterFailedRemoveResult.Value;
            Assert.Single(orderAfterFailedRemove.OrderLines);
            Assert.Equal(singleOrderLine.Id, orderAfterFailedRemove.OrderLines.First().Id);
        }

        [Fact]
        public async Task RemoveOrderLine_NonExistentOrderLine_ShouldReturnFailure()
        {
            // 1) Seed order with multiple lines
            var orderId = await OrderData.SeedOrderWithMultipleLinesAsync(Sender);

            // 2) Create a fake order line ID
            var fakeOrderLineId = Guid.NewGuid();

            // 3) Execute the command to remove the non-existent order line
            var removeCommand = new RemoveOrderLineFromOrderCommand(orderId, fakeOrderLineId);
            var removeResult = await Sender.Send(removeCommand);

            // 4) Verify that the command failed with the expected error
            Assert.True(removeResult.IsFailure);
            Assert.Equal("OrderLine.NotFound", removeResult.Error.Code);
            Assert.Contains("Order or Line not found", removeResult.Error.Name);
        }

        [Fact]
        public async Task RemoveOrderLine_NonExistentOrder_ShouldReturnFailure()
        {
            // 1) Create fake IDs
            var fakeOrderId = Guid.NewGuid();
            var fakeOrderLineId = Guid.NewGuid();

            // 2) Execute the command to remove order line from non-existent order
            var removeCommand = new RemoveOrderLineFromOrderCommand(fakeOrderId, fakeOrderLineId);
            var removeResult = await Sender.Send(removeCommand);

            // 3) Verify that the command failed with the expected error
            Assert.True(removeResult.IsFailure);
            Assert.Equal("OrderLine.NotFound", removeResult.Error.Code);
            Assert.Contains("Order or Line not found", removeResult.Error.Name);
        }
    }
}