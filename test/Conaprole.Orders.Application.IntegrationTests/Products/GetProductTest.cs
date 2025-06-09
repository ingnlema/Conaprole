using Conaprole.Orders.Application.Products.GetProduct;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Products
{
    [Collection("IntegrationCollection")]
    public class GetProductTest : BaseIntegrationTest
    {
        public GetProductTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetProductQuery_Returns_Seeded_Product()
        {
            // 1) Sembrar el producto y obtener su ID
            var productId = await ProductData.SeedAsync(Sender);

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetProductQuery(productId));
            Assert.False(queryResult.IsFailure);

            // 3) Verificar que el producto retornado coincide con los datos sembrados
            var product = queryResult.Value;
            Assert.NotNull(product);
            Assert.Equal(productId, product.Id);
            Assert.Equal(ProductData.ExternalProductId, product.ExternalProductId);
            Assert.Equal(ProductData.Name, product.Name);
            Assert.Equal(ProductData.UnitPrice, product.UnitPriceAmount);
            Assert.Equal(ProductData.CurrencyCode, product.UnitPriceCurrency);
            Assert.Equal(ProductData.Description, product.Description);
            Assert.Equal(((int)ProductData.DefaultCategory).ToString(), product.Category);
        }

        [Fact]
        public async Task GetProductQuery_Returns_Failure_For_NonExistent_Product()
        {
            // 1) Usar un ID que no existe
            var nonExistentId = Guid.NewGuid();

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetProductQuery(nonExistentId));

            // 3) Verificar que falla
            Assert.True(queryResult.IsFailure);
        }
    }
}