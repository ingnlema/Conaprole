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
    public class GetOrdersByIdsTest : BaseFunctionalTest
    {
        public GetOrdersByIdsTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrders_FilterByIds_ShouldReturnOnlySpecifiedOrders()
        {
            var distributorPhone = "+59892222222";
            var pointOfSalePhone = "+59895555555";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            await ProductData.CreateAsync(HttpClient);
            
            var line = ProductData.OrderLine(1);

            // Create 3 orders
            var order1 = await CreateOrderAsync(distributorPhone, pointOfSalePhone, line, "City1");
            var order2 = await CreateOrderAsync(distributorPhone, pointOfSalePhone, line, "City2");
            var order3 = await CreateOrderAsync(distributorPhone, pointOfSalePhone, line, "City3");

            // Request only 2 specific orders by IDs
            var idsParam = $"{order1},{order3}";
            var response = await HttpClient.GetAsync($"api/Orders?ids={idsParam}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await response.Content.ReadFromJsonAsync<List<OrderSummaryResponse>>();
            list.Should().NotBeNull();
            list.Should().HaveCount(2);
            
            var returnedIds = list!.Select(o => o.Id).ToHashSet();
            returnedIds.Should().Contain(order1);
            returnedIds.Should().Contain(order3);
            returnedIds.Should().NotContain(order2);
        }

        [Fact]
        public async Task GetOrders_FilterByIds_EmptyList_ShouldReturnAllOrders()
        {
            var distributorPhone = "+59892222223";
            var pointOfSalePhone = "+59895555556";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            await ProductData.CreateAsync(HttpClient);
            
            var line = ProductData.OrderLine(1);

            // Create 1 order
            var orderId = await CreateOrderAsync(distributorPhone, pointOfSalePhone, line, "City1");

            // Request with empty ids parameter
            var response = await HttpClient.GetAsync("api/Orders?ids=");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await response.Content.ReadFromJsonAsync<List<OrderSummaryResponse>>();
            list.Should().NotBeNull();
            list.Should().Contain(o => o.Id == orderId);
        }

        [Fact]
        public async Task GetOrders_FilterByIds_InvalidGuid_ShouldReturnBadRequest()
        {
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            // Request with invalid GUID
            var response = await HttpClient.GetAsync("api/Orders?ids=invalid-guid");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateOrderAsync(string distributorPhone, string pointOfSalePhone, OrderLineRequest line, string city)
        {
            var createReq = new CreateOrderRequest(
                pointOfSalePhone,
                distributorPhone,
                city,
                "Street",
                "12345",
                "UYU",
                new List<OrderLineRequest> { line }
            );

            var createResp = await HttpClient.PostAsJsonAsync("api/Orders", createReq);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            return await createResp.Content.ReadFromJsonAsync<Guid>();
        }
    }
}