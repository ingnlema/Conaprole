using Conaprole.Orders.Application.Orders.RemoveOrderLine;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class RemoveOrderLineTest : BaseIntegrationTest
    {
        public RemoveOrderLineTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task RemoveOrderLine_WithMultipleLines_ShouldSucceed()
        {
            // Arrange - Sembrar una orden con dos líneas
            var orderId = await OrderData.SeedOrderWithTwoLinesAsync(Sender);

            // Obtener la orden para verificar que tiene 2 líneas
            var getOrderQuery = new GetOrderQuery(orderId);
            var getOrderResult = await Sender.Send(getOrderQuery);
            Assert.False(getOrderResult.IsFailure);
            Assert.Equal(2, getOrderResult.Value.OrderLines.Count);

            // Obtener el ID de una línea para remover
            var orderLineToRemove = getOrderResult.Value.OrderLines.First();
            var orderLineToKeep = getOrderResult.Value.OrderLines.Last();

            // Act - Remover la línea
            var removeCommand = new RemoveOrderLineFromOrderCommand(orderId, orderLineToRemove.Id);
            var removeResult = await Sender.Send(removeCommand);

            // Assert - Verificar que la operación fue exitosa
            Assert.False(removeResult.IsFailure);
            Assert.Equal(orderLineToRemove.Id, removeResult.Value);

            // Verificar que la orden ahora tiene solo 1 línea
            var getOrderAfterQuery = new GetOrderQuery(orderId);
            var getOrderAfterResult = await Sender.Send(getOrderAfterQuery);
            Assert.False(getOrderAfterResult.IsFailure);
            Assert.Single(getOrderAfterResult.Value.OrderLines);
            Assert.Equal(orderLineToKeep.Id, getOrderAfterResult.Value.OrderLines.Single().Id);
        }

        [Fact]
        public async Task RemoveOrderLine_LastLine_ShouldFail()
        {
            // Arrange - Sembrar una orden con una sola línea
            var orderId = await OrderData.SeedOrderWithSingleLineAsync(Sender);

            // Obtener la orden para verificar que tiene 1 línea
            var getOrderQuery = new GetOrderQuery(orderId);
            var getOrderResult = await Sender.Send(getOrderQuery);
            Assert.False(getOrderResult.IsFailure);
            Assert.Single(getOrderResult.Value.OrderLines);

            var orderLineId = getOrderResult.Value.OrderLines.Single().Id;

            // Act - Intentar remover la última línea
            var removeCommand = new RemoveOrderLineFromOrderCommand(orderId, orderLineId);
            var removeResult = await Sender.Send(removeCommand);

            // Assert - Verificar que la operación falló con el error esperado
            Assert.True(removeResult.IsFailure);
            Assert.Equal(OrderErrors.LastOrderLineCannotBeRemoved, removeResult.Error);

            // Verificar que la orden aún tiene la línea
            var getOrderAfterQuery = new GetOrderQuery(orderId);
            var getOrderAfterResult = await Sender.Send(getOrderAfterQuery);
            Assert.False(getOrderAfterResult.IsFailure);
            Assert.Single(getOrderAfterResult.Value.OrderLines);
            Assert.Equal(orderLineId, getOrderAfterResult.Value.OrderLines.Single().Id);
        }

        [Fact]
        public async Task RemoveOrderLine_NonExistentLine_ShouldFail()
        {
            // Arrange - Sembrar una orden con dos líneas
            var orderId = await OrderData.SeedOrderWithTwoLinesAsync(Sender);
            var nonExistentLineId = Guid.NewGuid();

            // Act - Intentar remover una línea que no existe
            var removeCommand = new RemoveOrderLineFromOrderCommand(orderId, nonExistentLineId);
            var removeResult = await Sender.Send(removeCommand);

            // Assert - Verificar que la operación falló con el error esperado
            Assert.True(removeResult.IsFailure);
            Assert.Equal(OrderErrors.LineNotFound, removeResult.Error);
        }

        [Fact]
        public async Task RemoveOrderLine_NonExistentOrder_ShouldFail()
        {
            // Arrange
            var nonExistentOrderId = Guid.NewGuid();
            var nonExistentLineId = Guid.NewGuid();

            // Act - Intentar remover una línea de una orden que no existe
            var removeCommand = new RemoveOrderLineFromOrderCommand(nonExistentOrderId, nonExistentLineId);
            var removeResult = await Sender.Send(removeCommand);

            // Assert - Verificar que la operación falló con el error esperado
            Assert.True(removeResult.IsFailure);
            Assert.Equal(OrderErrors.LineNotFound, removeResult.Error);
        }
    }
}