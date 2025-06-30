// File: Conaprole.Orders.Api.FunctionalTests/Orders/RemoveOrderLineTest.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

using Conaprole.Orders.Api.Controllers.Orders;      
using Conaprole.Orders.Application.Orders.GetOrder;      
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class RemoveOrderLineTest : BaseFunctionalTest
    {
        public RemoveOrderLineTest(FunctionalTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task RemoveLine_ShouldReturnNoContent_And_OrderHasOneLineLeft()
        {
            var distributorPhone = "+59897777777";
            var pointOfSalePhone = "+59898888888";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            var sku1 = $"SKU-{Guid.NewGuid():N}";
            await ProductData.CreateAsync(HttpClient, sku1);
            var sku2 = $"SKU-{Guid.NewGuid():N}";
            await ProductData.CreateAsync(HttpClient, sku2);

            var line1 = new OrderLineRequest(sku1, 1);
            var line2 = new OrderLineRequest(sku2, 2);
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/Orders",
                new CreateOrderRequest(
                    pointOfSalePhone,
                    distributorPhone,
                    "City",
                    "Street",
                    "77777",
                    "UYU",
                    new List<OrderLineRequest> { line1, line2 }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResp.Content.ReadFromJsonAsync<Guid>();

            var getResp = await HttpClient.GetAsync($"api/Orders/{orderId}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getResp.Content.ReadFromJsonAsync<OrderResponse>();
            order!.OrderLines.Should().HaveCount(2);

            var ids = order.OrderLines.Select(l => l.Id).ToList();
            var removeId = ids.First();
            var keepId = ids.Last();

            var remResp = await HttpClient.DeleteAsync(
                $"api/Orders/{orderId}/lines/{removeId}"
            );
            remResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getAfterResp = await HttpClient.GetAsync($"api/Orders/{orderId}");
            getAfterResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var orderAfter = await getAfterResp.Content.ReadFromJsonAsync<OrderResponse>();
            orderAfter!.OrderLines.Should().HaveCount(1);
            orderAfter.OrderLines.Single().Id.Should().Be(keepId);
        }

        [Fact]
        public async Task RemoveLastLine_ShouldReturnBadRequest_And_OrderStillHasOneLine()
        {
            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();
            
            var sku = $"SKU-{Guid.NewGuid():N}";
            await ProductData.CreateAsync(HttpClient, sku);

            var distributorPhone = "+59897777777";
            var pointOfSalePhone = "+59898888888";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            var line = new OrderLineRequest(sku, 1);
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/Orders",
                new CreateOrderRequest(
                    pointOfSalePhone,
                    distributorPhone,
                    "City",
                    "Street",
                    "77777",
                    "UYU",
                    new List<OrderLineRequest> { line }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResp.Content.ReadFromJsonAsync<Guid>();

            var getResp = await HttpClient.GetAsync($"api/Orders/{orderId}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getResp.Content.ReadFromJsonAsync<OrderResponse>();
            order!.OrderLines.Should().HaveCount(1);

            var lineId = order.OrderLines.Single().Id;
            var remResp = await HttpClient.DeleteAsync(
                $"api/Orders/{orderId}/lines/{lineId}"
            );
            remResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var getAfter = await HttpClient.GetAsync($"api/Orders/{orderId}");
            var orderAfter = await getAfter.Content.ReadFromJsonAsync<OrderResponse>();
            orderAfter!.OrderLines.Should().HaveCount(1);
            orderAfter.OrderLines.Single().Id.Should().Be(lineId);
        }
    }
}
