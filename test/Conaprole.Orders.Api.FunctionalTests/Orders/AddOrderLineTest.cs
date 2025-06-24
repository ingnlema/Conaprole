// File: Conaprole.Orders.Api.FunctionalTests/Orders/AddOrderLineTest.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;
using Conaprole.Orders.Application.Orders.GetOrder;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class AddOrderLineTest : BaseFunctionalTest
    {
        public AddOrderLineTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task CreateOrder_AddLine_ShouldReturnNoContent_And_OrderContainsBothLines()
        {
            var distributorPhone = "+59899887766";
            var pointOfSalePhone = "+59891234567";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var sku1 = $"SKU-{Guid.NewGuid():N}";
            await ProductData.CreateAsync(HttpClient, sku1);
            var initialLine = new OrderLineRequest(sku1, 2);

            var sku2 = $"SKU-{Guid.NewGuid():N}";
            await ProductData.CreateAsync(HttpClient, sku2);
            var newLine = new OrderLineRequest(sku2, 3);

            var createRequest = new CreateOrderRequest(
                pointOfSalePhone,            
                distributorPhone,          
                "Montevideo",
                "Test Street",
                "11200",
                "UYU",
                new List<OrderLineRequest> { initialLine }
            );
            var createResponse = await HttpClient.PostAsJsonAsync("api/Orders", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var addLineResponse = await HttpClient.PostAsJsonAsync(
                $"api/Orders/{orderId}/lines",
                new AddOrderLineRequest(newLine.ExternalProductId, newLine.Quantity)
            );
            addLineResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await HttpClient.GetAsync($"api/Orders/{orderId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

            order!.OrderLines.Should().HaveCount(2);
            order.OrderLines.Any(l =>
                l.Product.ExternalProductId == sku1 &&
                l.Quantity == 2
            ).Should().BeTrue();
            order.OrderLines.Any(l =>
                l.Product.ExternalProductId == sku2 &&
                l.Quantity == 3
            ).Should().BeTrue();
        }
    }
}
