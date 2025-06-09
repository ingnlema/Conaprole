using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class GetOrderTest : BaseIntegrationTest
    {
        public GetOrderTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrderQuery_WithValidId_Returns_Success()
        {
            // 1) Sembrar una orden y obtener su ID
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Ejecutar el query con el ID de la orden sembrada
            var query = new GetOrderQuery(orderId);
            var result = await Sender.Send(query);

            // 3) Verificar que el resultado sea exitoso
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            var order = result.Value;

            // 4) Verificar que los datos coincidan con los sembrados
            Assert.Equal(orderId, order.Id);
            Assert.Equal(OrderData.PointOfSalePhone, order.PointOfSalePhoneNumber);
            Assert.Equal(OrderData.DistributorPhone, order.DistributorPhoneNumber);
            Assert.Equal(OrderData.DeliveryCity, order.DeliveryAddressCity);
            Assert.Equal(OrderData.DeliveryStreet, order.DeliveryAddressStreet);
            Assert.Equal(OrderData.DeliveryZipCode, order.DeliveryAddressZipCode);
            Assert.Equal(OrderData.CurrencyCode, order.PriceCurrency);

            // 5) Verificar que tiene fecha de creación
            Assert.True(order.CreatedOnUtc > DateTime.MinValue);

            // 6) Verificar que contiene las líneas de orden esperadas
            Assert.NotEmpty(order.OrderLines);
            Assert.Single(order.OrderLines); // OrderData crea una línea

            var orderLine = order.OrderLines.First();
            Assert.Equal(OrderData.OrderLineQuantity, orderLine.Quantity);
            Assert.NotNull(orderLine.Product);
            Assert.Equal(ProductData.ExternalProductId, orderLine.Product.ExternalProductId); // "GLOBAL_SKU"
        }

        [Fact]
        public async Task GetOrderQuery_WithInvalidId_Returns_Failure()
        {
            // 1) Usar un ID que no existe
            var nonExistentId = Guid.NewGuid();

            // 2) Ejecutar el query con el ID inexistente
            var query = new GetOrderQuery(nonExistentId);
            var result = await Sender.Send(query);

            // 3) Verificar que el resultado sea un fallo
            Assert.True(result.IsFailure);
            Assert.Equal("Order.NotFound", result.Error.Code);
        }
    }
}