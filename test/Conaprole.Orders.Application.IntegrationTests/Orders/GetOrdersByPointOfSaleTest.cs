using Conaprole.Orders.Application.Orders.GetOrdersByPointOfSale;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Orders;

[Collection("IntegrationCollection")]
public class GetOrdersByPointOfSaleTest : BaseIntegrationTest
{
    public GetOrdersByPointOfSaleTest(IntegrationTestWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetOrdersByPointOfSaleQuery_Returns_Seeded_Orders()
    {
        // Arrange
        const string pointOfSalePhoneNumber = "+59891234567";
        const int expectedOrderCount = 3;

        // 1) Sembrar las órdenes para el punto de venta
        var orderIds = await OrderData.SeedMultipleAsync(Sender, pointOfSalePhoneNumber, expectedOrderCount);

        // 2) Ejecutar el query
        var queryResult = await Sender.Send(new GetOrdersByPointOfSaleQuery(pointOfSalePhoneNumber));
        
        // 3) Verificar que el resultado sea exitoso
        Assert.False(queryResult.IsFailure);
        
        // 4) Verificar que se devolvieron las órdenes esperadas
        var orders = queryResult.Value;
        Assert.NotNull(orders);
        Assert.Equal(expectedOrderCount, orders.Count);
        
        // 5) Verificar que todas las órdenes pertenecen al punto de venta correcto
        foreach (var order in orders)
        {
            Assert.Equal(pointOfSalePhoneNumber, order.PointOfSalePhoneNumber);
            Assert.Contains(order.Id, orderIds);
        }
    }

    [Fact]
    public async Task GetOrdersByPointOfSaleQuery_Returns_Empty_When_No_Orders_Found()
    {
        // Arrange
        const string nonExistentPhoneNumber = "+59999999999";

        // Act
        var queryResult = await Sender.Send(new GetOrdersByPointOfSaleQuery(nonExistentPhoneNumber));
        
        // Assert
        Assert.False(queryResult.IsFailure);
        var orders = queryResult.Value;
        Assert.NotNull(orders);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task GetOrdersByPointOfSaleQuery_Returns_Only_Orders_For_Specified_PointOfSale()
    {
        // Arrange
        const string pointOfSale1Phone = "+59891111111";
        const string pointOfSale2Phone = "+59892222222";

        // 1) Crear órdenes para dos puntos de venta diferentes
        var ordersForPos1 = await OrderData.SeedMultipleAsync(Sender, pointOfSale1Phone, 2);
        var ordersForPos2 = await OrderData.SeedMultipleAsync(Sender, pointOfSale2Phone, 3);

        // 2) Consultar órdenes para el primer punto de venta
        var queryResult1 = await Sender.Send(new GetOrdersByPointOfSaleQuery(pointOfSale1Phone));
        
        // 3) Verificar que solo devuelve órdenes del primer punto de venta
        Assert.False(queryResult1.IsFailure);
        var orders1 = queryResult1.Value;
        Assert.Equal(2, orders1.Count);
        Assert.All(orders1, order => Assert.Equal(pointOfSale1Phone, order.PointOfSalePhoneNumber));
        
        // 4) Consultar órdenes para el segundo punto de venta
        var queryResult2 = await Sender.Send(new GetOrdersByPointOfSaleQuery(pointOfSale2Phone));
        
        // 5) Verificar que solo devuelve órdenes del segundo punto de venta
        Assert.False(queryResult2.IsFailure);
        var orders2 = queryResult2.Value;
        Assert.Equal(3, orders2.Count);
        Assert.All(orders2, order => Assert.Equal(pointOfSale2Phone, order.PointOfSalePhoneNumber));
    }
}