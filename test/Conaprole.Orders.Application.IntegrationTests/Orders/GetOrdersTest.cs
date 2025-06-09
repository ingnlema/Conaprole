using Conaprole.Orders.Application.Orders.GetOrders;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Orders
{
    [Collection("IntegrationCollection")]
    public class GetOrdersTest : BaseIntegrationTest
    {
        public GetOrdersTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrdersQuery_Returns_Empty_List_When_No_Orders()
        {
            // 1) Ejecutar el query sin sembrar órdenes
            var queryResult = await Sender.Send(new GetOrdersQuery(null, null, null, null, null, null));
            
            // 2) Verificar que el resultado es exitoso pero vacío
            Assert.False(queryResult.IsFailure);
            Assert.NotNull(queryResult.Value);
            Assert.Empty(queryResult.Value);
        }

        [Fact]
        public async Task GetOrdersQuery_Returns_Seeded_Order()
        {
            // 1) Sembrar una orden y obtener su ID
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetOrdersQuery(null, null, null, null, null, null));
            Assert.False(queryResult.IsFailure);

            // 3) Buscar la orden que acabamos de sembrar
            var orders = queryResult.Value;
            var fetched = orders.First(o => o.Id == orderId);

            // 4) Verificar que coincide con nuestros datos de prueba
            Assert.NotNull(fetched);
            Assert.Equal(OrderData.DistributorPhone, fetched.DistributorPhoneNumber);
            Assert.Equal(OrderData.PointOfSalePhone, fetched.PointOfSalePhoneNumber);
            Assert.Equal(OrderData.DeliveryCity, fetched.City);
            Assert.Equal(OrderData.DeliveryStreet, fetched.Street);
            Assert.Equal(OrderData.DeliveryZipCode, fetched.ZipCode);
            Assert.Equal(OrderData.CurrencyCode, fetched.PriceCurrency);
            
            // 5) Verificar precio total (ProductData.UnitPrice * OrderData.OrderLineQuantity)
            var expectedPrice = 100m * OrderData.OrderLineQuantity; // 100 * 2 = 200
            Assert.Equal(expectedPrice, fetched.PriceAmount);
            
            // 6) Verificar que tiene fecha de creación
            Assert.True(fetched.CreatedOnUtc > DateTime.MinValue);
        }
    }
}