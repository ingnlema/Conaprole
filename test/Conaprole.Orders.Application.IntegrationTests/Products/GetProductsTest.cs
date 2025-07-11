
using Conaprole.Orders.Application.Products.GetProducts;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;


namespace Conaprole.Orders.Application.IntegrationTests.Products
{
    [Collection("IntegrationCollection")]
    public class GetProductsTest : BaseIntegrationTest
    {
        public GetProductsTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetProductsQuery_Returns_Seeded_Product()
        {
            // 1) Sembrar el producto y obtener su ID
            var productId = await ProductData.SeedAsync(Sender);

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetProductsQuery());
            Assert.False(queryResult.IsFailure);

            // 3) Buscar el producto que acabamos de semillar
            var products = queryResult.Value;
            var fetched  = products.First(p => p.Id == productId);

            // 4) Verificar que coincide con nuestros datos de prueba
            Assert.NotNull(fetched);
            Assert.Equal(ProductData.ExternalProductId, fetched.ExternalProductId);
            Assert.Equal(ProductData.Name,              fetched.Name);
            Assert.Equal(ProductData.UnitPrice,         fetched.UnitPrice);
            Assert.Equal(ProductData.Description,       fetched.Description);
        }

        [Fact]
        public async Task GetProductsQuery_Returns_Empty_List_When_No_Products()
        {
            // 1) Ejecutar el query sin sembrar productos
            var queryResult = await Sender.Send(new GetProductsQuery());
            
            // 2) Verificar que el resultado es exitoso pero vacío
            Assert.False(queryResult.IsFailure);
            Assert.NotNull(queryResult.Value);
            Assert.Empty(queryResult.Value);
        }
    }
}