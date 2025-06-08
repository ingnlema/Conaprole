using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Application.IntegrationTests.Distributors;
using Conaprole.Orders.Application.IntegrationTests.Products;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class GetOrderTest : BaseIntegrationTest
    {
        public GetOrderTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrderQuery_Returns_Seeded_Order()
        {
            // 1) Sembrar la orden y obtener su ID
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetOrderQuery(orderId));
            Assert.False(queryResult.IsFailure);

            // 3) Verificar que el resultado coincide con nuestros datos de prueba
            var order = queryResult.Value;
            Assert.NotNull(order);
            Assert.Equal(orderId, order.Id);
            Assert.Equal(PointOfSaleData.PhoneNumber, order.PointOfSalePhoneNumber);
            Assert.Equal(DistributorData.PhoneNumber, order.DistributorPhoneNumber);
            Assert.Equal(PointOfSaleData.City, order.DeliveryAddressCity);
            Assert.Equal(PointOfSaleData.Street, order.DeliveryAddressStreet);
            Assert.Equal(PointOfSaleData.ZipCode, order.DeliveryAddressZipCode);
            Assert.Equal(OrderData.CurrencyCode, order.PriceCurrency);

            // 4) Verificar que tiene las líneas de orden esperadas
            Assert.NotEmpty(order.OrderLines);
            var orderLine = order.OrderLines.First();
            Assert.Equal(OrderData.OrderLineQuantity, orderLine.Quantity);
            Assert.Equal(ProductData.ExternalProductId, orderLine.Product.ExternalProductId);
            Assert.Equal(ProductData.Name, orderLine.Product.Name);
        }
    }
}