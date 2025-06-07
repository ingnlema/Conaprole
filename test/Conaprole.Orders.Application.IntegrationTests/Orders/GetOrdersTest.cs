using Conaprole.Orders.Application.Orders.GetOrders;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.Orders;

[Collection("IntegrationCollection")]
public class GetOrdersTest : BaseIntegrationTest
{
    public GetOrdersTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetOrdersQuery_Returns_Seeded_Order()
    {
        // 1) Sembrar la orden y obtener su ID
        var orderId = await OrderData.SeedAsync(Sender);

        // 2) Ejecutar el query
        var queryResult = await Sender.Send(new GetOrdersQuery(null, null, null, null, null));
        Assert.False(queryResult.IsFailure);

        // 3) Buscar la orden que acabamos de sembrar
        var orders = queryResult.Value;
        var fetchedOrder = orders.First(o => o.Id == orderId);

        // 4) Verificar que coincide con nuestros datos de prueba
        Assert.NotNull(fetchedOrder);
        Assert.Equal(OrderData.City, fetchedOrder.City);
        Assert.Equal(OrderData.Street, fetchedOrder.Street);
        Assert.Equal(OrderData.ZipCode, fetchedOrder.ZipCode);
        Assert.Equal(OrderData.CurrencyCode, fetchedOrder.PriceCurrency);
    }

    [Fact]
    public async Task GetOrdersQuery_WithPointOfSaleFilter_Returns_Filtered_Orders()
    {
        // 1) Sembrar la orden
        var orderId = await OrderData.SeedAsync(Sender);

        // 2) Ejecutar el query con filtro por teléfono del punto de venta
        var queryResult = await Sender.Send(new GetOrdersQuery(
            null, null, null, null, PointOfSaleData.PhoneNumber));
        Assert.False(queryResult.IsFailure);

        // 3) Verificar que se encontró la orden filtrada
        var orders = queryResult.Value;
        var fetchedOrder = orders.First(o => o.Id == orderId);
        
        Assert.NotNull(fetchedOrder);
        Assert.Equal(PointOfSaleData.PhoneNumber, fetchedOrder.PointOfSalePhoneNumber);
    }
}