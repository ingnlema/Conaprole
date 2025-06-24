// File: Conaprole.Orders.Api.FunctionalTests/Orders/UpdateOrderLineQuantityTest.cs
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Linq;
using FluentAssertions;
using Xunit;

using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class UpdateOrderLineQuantityTest : BaseFunctionalTest
    {
        public UpdateOrderLineQuantityTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task UpdateLineQuantity_ShouldReturnNoContent_And_ReflectChange()
        {
            var distributorPhone = "+59897776666";
            var pointOfSalePhone = "+59898887777";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var sku = $"SKU-{Guid.NewGuid():N}";
            await ProductData.CreateAsync(HttpClient, sku);

            var line = new OrderLineRequest(sku, 1);
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/Orders",
                new CreateOrderRequest(
                    pointOfSalePhone,
                    distributorPhone,
                    "City",
                    "Street",
                    "66666",
                    "UYU",
                    new List<OrderLineRequest> { line }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResp.Content.ReadFromJsonAsync<Guid>();

            var getResp1 = await HttpClient.GetAsync($"api/Orders/{orderId}");
            getResp1.StatusCode.Should().Be(HttpStatusCode.OK);
            var order1 = await getResp1.Content.ReadFromJsonAsync<OrderResponse>();
            order1!.OrderLines.Should().HaveCount(1);
            var lineId = order1.OrderLines.Single().Id;
            order1.OrderLines.Single().Quantity.Should().Be(1);

            var updResp = await HttpClient.PutAsJsonAsync(
                $"api/Orders/{orderId}/lines/{lineId}",
                new UpdateOrderLineQuantityRequest { NewQuantity = 4 }
            );
            updResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResp2 = await HttpClient.GetAsync($"api/Orders/{orderId}");
            getResp2.StatusCode.Should().Be(HttpStatusCode.OK);
            var order2 = await getResp2.Content.ReadFromJsonAsync<OrderResponse>();
            order2!.OrderLines.Should().HaveCount(1);
            order2.OrderLines.Single().Quantity.Should().Be(4);
        }
    }
}
