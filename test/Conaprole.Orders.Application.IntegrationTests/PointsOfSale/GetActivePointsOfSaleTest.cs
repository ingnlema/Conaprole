using Conaprole.Orders.Application.PointsOfSale.GetActivePointsOfSale;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;
using Xunit;

namespace Conaprole.Orders.Application.IntegrationTests.PointsOfSale
{
    [Collection("IntegrationCollection")]
    public class GetActivePointsOfSaleTest : BaseIntegrationTest
    {
        public GetActivePointsOfSaleTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetActivePointsOfSaleQuery_Returns_Seeded_PointOfSale()
        {
            // 1) Sembrar el punto de venta y obtener su ID
            var pointOfSaleId = await PointOfSaleData.SeedAsync(Sender);

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            Assert.False(queryResult.IsFailure);

            // 3) Buscar el punto de venta que acabamos de sembrar
            var pointsOfSale = queryResult.Value;
            var fetched = pointsOfSale.First(pos => pos.Id == pointOfSaleId);

            // 4) Verificar que coincide con nuestros datos de prueba
            Assert.NotNull(fetched);
            Assert.Equal(PointOfSaleData.Name, fetched.Name);
            Assert.Equal(PointOfSaleData.PhoneNumber, fetched.PhoneNumber);
            Assert.True(fetched.IsActive);
            Assert.True(fetched.CreatedAt > DateTime.MinValue);
            
            // 5) Verificar la dirección contiene los datos correctos
            Assert.Contains(PointOfSaleData.Street, fetched.Address);
            Assert.Contains(PointOfSaleData.City, fetched.Address);
            Assert.Contains(PointOfSaleData.ZipCode, fetched.Address);
        }

        [Fact]
        public async Task GetActivePointsOfSaleQuery_Returns_Empty_List_When_No_PointsOfSale()
        {
            // 1) Ejecutar el query sin sembrar puntos de venta
            var queryResult = await Sender.Send(new GetActivePointsOfSaleQuery());
            
            // 2) Verificar que el resultado es exitoso pero vacío
            Assert.False(queryResult.IsFailure);
            Assert.NotNull(queryResult.Value);
            Assert.Empty(queryResult.Value);
        }
    }
}