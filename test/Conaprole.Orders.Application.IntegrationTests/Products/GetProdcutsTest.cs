
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Conaprole.Orders.Application.Products.GetProducts;
using Conaprole.Ordes.Application.IntegrationTests.Infrastructure;
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Domain.Shared;

namespace Conaprole.Ordes.Application.IntegrationTests.Products
{
    public class GetProductsTest : BaseIntegrationTest
    {
        public GetProductsTest(IntegrationTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task GetProductsQuery_Returns_Seeded_Product()
        {
            var productId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            
            var product = new Product(
                productId,
                new ExternalProductId("SKU_TEST"),
                new Name("Integration Test Product"),
                new Money(200m, Currency.Uyu),
                new Description("This is a test product"),
                now
            );
            
            product.Categories.Add(new Category("TestCategory"));
            
            DbContext.Products.Add(product);
            await DbContext.SaveChangesAsync();
            
            var queryResult = await Sender.Send(new GetProductsQuery());
            Assert.False(queryResult.IsFailure, "El query retornó un fallo.");
            
            var products = queryResult.Value;
            Assert.NotEmpty(products);

            var fetchedProduct = products.FirstOrDefault(p => p.Id == productId);
            Assert.NotNull(fetchedProduct);
            Assert.Equal("SKU_TEST", fetchedProduct.ExternalProductId);
            Assert.Equal("Integration Test Product", fetchedProduct.Name);
            Assert.Equal("This is a test product", fetchedProduct.Description);
            Assert.Equal(200m, fetchedProduct.UnitPrice);

            Assert.Equal(now, fetchedProduct.LastUpdated, TimeSpan.FromSeconds(1));
            Assert.Contains("TestCategory", fetchedProduct.Categories);
        }
    }
}
