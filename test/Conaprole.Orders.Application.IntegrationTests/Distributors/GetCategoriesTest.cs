using Conaprole.Orders.Application.Distributors.GetCategories;
using Conaprole.Orders.Application.IntegrationTests.Infrastructure;

namespace Conaprole.Orders.Application.IntegrationTests.Distributors
{
    [Collection("IntegrationCollection")]
    public class GetCategoriesTest : BaseIntegrationTest
    {
        public GetCategoriesTest(IntegrationTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetDistributorCategoriesQuery_Returns_Seeded_Categories()
        {
            // 1) Sembrar el distribuidor y obtener su ID
            var distributorId = await DistributorData.SeedAsync(Sender);

            // 2) Ejecutar el query
            var queryResult = await Sender.Send(new GetDistributorCategoriesQuery(DistributorData.PhoneNumber));
            Assert.False(queryResult.IsFailure);

            // 3) Verificar que las categorías retornadas coincidan con las sembradas
            var categories = queryResult.Value;
            Assert.NotNull(categories);
            Assert.Single(categories);
            Assert.Contains("LACTEOS", categories);
        }
    }
}