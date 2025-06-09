using Conaprole.Orders.Application.Orders.UpdateOrderStatus;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Domain.Orders;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class UpdateOrderStatusTest : BaseIntegrationTest
    {
        public UpdateOrderStatusTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task UpdateOrderStatus_ShouldUpdateStatusToConfirmed()
        {
            // 1) Sembrar orden y obtener su ID
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Ejecutar el comando para actualizar el estado
            var updateCommand = new UpdateOrderStatusCommand(orderId, Status.Confirmed);
            var updateResult = await Sender.Send(updateCommand);

            // 3) Verificar que el comando fue exitoso
            Assert.False(updateResult.IsFailure);

            // 4) Verificar que el estado fue actualizado usando GetOrder
            var getOrderQuery = new GetOrderQuery(orderId);
            var getOrderResult = await Sender.Send(getOrderQuery);

            Assert.False(getOrderResult.IsFailure);
            var order = getOrderResult.Value;
            Assert.Equal((int)Status.Confirmed, order.Status);
            Assert.NotNull(order.ConfirmedOnUtc);
            Assert.True(order.ConfirmedOnUtc > DateTime.MinValue);
        }
    }
}