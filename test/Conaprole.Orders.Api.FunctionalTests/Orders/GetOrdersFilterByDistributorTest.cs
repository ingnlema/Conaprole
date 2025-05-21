using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;
using Conaprole.Orders.Application.Orders.GetOrders;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class GetOrdersFilterByDistributorTest : BaseFunctionalTest
    {
        public GetOrdersFilterByDistributorTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrders_FilterByDistributor_ShouldReturnOnlyMatching()
        {
            var distributorPhone = "+59890000000";
            var pointOfSalePhone = "+59891111111";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // 1) Sembrar el producto global y construir la línea de pedido
            await ProductData.CreateAsync(HttpClient);
            var line = ProductData.OrderLine(1);

            // 2) Crear la orden con distributorPhone
            var request = new CreateOrderRequest(
                pointOfSalePhone,
                distributorPhone,
                "City",
                "Street",
                "00000",
                "UYU",
                new List<OrderLineRequest> { line }
            );
            var createResp = await HttpClient.PostAsJsonAsync("api/Orders", request);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);

            // 3) Filtrar por distributorPhone
            var response = await HttpClient.GetAsync($"api/Orders?Distributor={distributorPhone}");

            // 4) Verificar
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var list = await response.Content.ReadFromJsonAsync<List<OrderSummaryResponse>>();
            list.Should().NotBeNull();
            list.Should().OnlyContain(o => o.DistributorPhoneNumber.Contains(distributorPhone));
        }
    }
}