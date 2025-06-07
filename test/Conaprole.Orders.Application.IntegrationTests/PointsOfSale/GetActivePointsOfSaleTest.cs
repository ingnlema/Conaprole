using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class GetActivePointsOfSaleTest : BaseIntegrationTest
    {
        public GetActivePointsOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetActivePointsOfSaleQuery_Returns_Only_Active_PointsOfSale()
        {
            // 1) Sembrar un punto de venta activo y uno inactivo
            var activePointOfSaleId = await PointOfSaleData.SeedActiveAsync(Sender);
            var inactivePointOfSaleId = await PointOfSaleData.SeedInactiveAsync(Sender);

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            Assert.False(queryResult.IsFailure);

            // 3) Verificar que solo retorna puntos de venta activos
            var pointsOfSale = queryResult.Value;
            Assert.NotNull(pointsOfSale);

            // 4) Buscar el punto de venta activo que acabamos de sembrar
            var activePoint = pointsOfSale.FirstOrDefault(p => p.Id == activePointOfSaleId);
            Assert.NotNull(activePoint);
            Assert.Equal(PointOfSaleData.Name, activePoint.Name);
            Assert.Equal(PointOfSaleData.PhoneNumber, activePoint.PhoneNumber);
            Assert.True(activePoint.IsActive);

            // 5) Verificar que el punto de venta inactivo NO está incluido
            var inactivePoint = pointsOfSale.FirstOrDefault(p => p.Id == inactivePointOfSaleId);
            Assert.Null(inactivePoint);

            // 6) Verificar que todos los puntos retornados están activos
            Assert.All(pointsOfSale, pos => Assert.True(pos.IsActive));
        }
    }
}