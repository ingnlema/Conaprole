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
        
        [Fact]
        public async Task GetOrdersQuery_With_Ids_Returns_Only_Specified_Orders()
        {
            // 1) Sembrar múltiples órdenes
            var orderId1 = await OrderData.SeedAsync(Sender);
            var orderId2 = await OrderData.SeedAsync(Sender);
            var orderId3 = await OrderData.SeedAsync(Sender);

            // 2) Ejecutar el query pidiendo solo 2 órdenes específicas
            var requestedIds = new List<Guid> { orderId1, orderId3 };
            var queryResult = await Sender.Send(new GetOrdersQuery(null, null, null, null, null, requestedIds));
            
            Assert.False(queryResult.IsFailure);
            Assert.NotNull(queryResult.Value);
            
            // 3) Verificar que devuelve exactamente 2 órdenes
            var orders = queryResult.Value;
            Assert.Equal(2, orders.Count);
            
            // 4) Verificar que devuelve las órdenes correctas
            var returnedIds = orders.Select(o => o.Id).ToHashSet();
            Assert.Contains(orderId1, returnedIds);
            Assert.Contains(orderId3, returnedIds);
            Assert.DoesNotContain(orderId2, returnedIds);
        }

        [Fact]
        public async Task GetOrdersQuery_With_Empty_Ids_Returns_All_Orders()
        {
            // 1) Sembrar una orden
            var orderId = await OrderData.SeedAsync(Sender);

            // 2) Ejecutar el query con lista de IDs vacía
            var queryResult = await Sender.Send(new GetOrdersQuery(null, null, null, null, null, new List<Guid>()));
            
            Assert.False(queryResult.IsFailure);
            Assert.NotNull(queryResult.Value);
            
            // 3) Verificar que devuelve la orden (comportamiento similar a no pasar IDs)
            var orders = queryResult.Value;
            Assert.Single(orders);
            Assert.Equal(orderId, orders.First().Id);
        }
    }
}