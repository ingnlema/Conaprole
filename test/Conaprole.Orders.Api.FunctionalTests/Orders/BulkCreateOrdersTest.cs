using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;
using Conaprole.Orders.Application.Orders.GetOrder;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class BulkCreateOrdersTest : BaseFunctionalTest
    {
        public BulkCreateOrdersTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task BulkCreateOrders_ShouldCreateMultipleOrdersSuccessfully()
        {
            var distributorPhone = "+59892222229";
            var pointOfSalePhone = "+59895555559";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            await ProductData.CreateAsync(HttpClient);
            
            var line = ProductData.OrderLine(2);

            var bulkRequest = new BulkCreateOrdersRequest
            {
                Orders = new List<CreateOrderRequest>
                {
                    new CreateOrderRequest(
                        pointOfSalePhone,
                        distributorPhone,
                        "City1",
                        "Street1",
                        "12345",
                        "UYU",
                        new List<OrderLineRequest> { line }
                    ),
                    new CreateOrderRequest(
                        pointOfSalePhone,
                        distributorPhone,
                        "City2",
                        "Street2",
                        "67890",
                        "UYU",
                        new List<OrderLineRequest> { line }
                    )
                }
            };

            var response = await HttpClient.PostAsJsonAsync("api/Orders/bulk", bulkRequest);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var orderIds = await response.Content.ReadFromJsonAsync<List<Guid>>();
            orderIds.Should().NotBeNull();
            orderIds.Should().HaveCount(2);

            // Verify orders were created by fetching them
            foreach (var orderId in orderIds!)
            {
                var getResponse = await HttpClient.GetAsync($"api/Orders/{orderId}");
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
                order.Should().NotBeNull();
                order!.Id.Should().Be(orderId);
            }
        }

        [Fact]
        public async Task BulkCreateOrders_WithInvalidProduct_ShouldReturnBadRequest()
        {
            var distributorPhone = "+59892222230";
            var pointOfSalePhone = "+59895555560";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            await ProductData.CreateAsync(HttpClient);
            
            var validLine = ProductData.OrderLine(1);
            var invalidLine = new OrderLineRequest("INVALID_PRODUCT", 1);

            var bulkRequest = new BulkCreateOrdersRequest
            {
                Orders = new List<CreateOrderRequest>
                {
                    new CreateOrderRequest(
                        pointOfSalePhone,
                        distributorPhone,
                        "City1",
                        "Street1",
                        "12345",
                        "UYU",
                        new List<OrderLineRequest> { validLine }
                    ),
                    new CreateOrderRequest(
                        pointOfSalePhone,
                        distributorPhone,
                        "City2",
                        "Street2",
                        "67890",
                        "UYU",
                        new List<OrderLineRequest> { invalidLine }
                    )
                }
            };

            var response = await HttpClient.PostAsJsonAsync("api/Orders/bulk", bulkRequest);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task BulkCreateOrders_WithEmptyOrdersList_ShouldReturnBadRequest()
        {
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var bulkRequest = new BulkCreateOrdersRequest
            {
                Orders = new List<CreateOrderRequest>()
            };

            var response = await HttpClient.PostAsJsonAsync("api/Orders/bulk", bulkRequest);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task BulkCreateOrders_WithInvalidPointOfSale_ShouldReturnBadRequest()
        {
            var distributorPhone = "+59892222231";

            await CreateDistributorAsync(distributorPhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            await ProductData.CreateAsync(HttpClient);
            
            var line = ProductData.OrderLine(1);

            var bulkRequest = new BulkCreateOrdersRequest
            {
                Orders = new List<CreateOrderRequest>
                {
                    new CreateOrderRequest(
                        "+59899999999", // Invalid POS phone
                        distributorPhone,
                        "City1",
                        "Street1",
                        "12345",
                        "UYU",
                        new List<OrderLineRequest> { line }
                    )
                }
            };

            var response = await HttpClient.PostAsJsonAsync("api/Orders/bulk", bulkRequest);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task BulkCreateOrders_WithInvalidDistributor_ShouldReturnBadRequest()
        {
            var pointOfSalePhone = "+59895555561";

            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            await ProductData.CreateAsync(HttpClient);
            
            var line = ProductData.OrderLine(1);

            var bulkRequest = new BulkCreateOrdersRequest
            {
                Orders = new List<CreateOrderRequest>
                {
                    new CreateOrderRequest(
                        pointOfSalePhone,
                        "+59899999998", // Invalid distributor phone
                        "City1",
                        "Street1",
                        "12345",
                        "UYU",
                        new List<OrderLineRequest> { line }
                    )
                }
            };

            var response = await HttpClient.PostAsJsonAsync("api/Orders/bulk", bulkRequest);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}