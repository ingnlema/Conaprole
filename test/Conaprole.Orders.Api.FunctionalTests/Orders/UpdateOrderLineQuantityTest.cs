using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.Controllers.Orders.Examples;
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
        public async Task UpdateLineQuantity_ShouldReturnNoContent()
        {
            // 1) Crear el producto global y obtener su GUID
            var productId = await ProductData.CreateAsync(HttpClient);

            // 2) Crear la orden con una línea inicial de ese producto
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/Orders",
                new CreateOrderRequest(
                    "+59896666666",
                    "LineDist",
                    "City",
                    "Street",
                    "66666",
                    "UYU",
                    new List<OrderLineRequest>
                    {
                        ProductData.OrderLine(quantity: 1)
                    }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResp.Content.ReadFromJsonAsync<Guid>();

            // 3) Actualizar la cantidad de esa misma línea
            var updResp = await HttpClient.PutAsJsonAsync(
                $"api/Orders/{orderId}/lines/{productId}",
                new UpdateOrderLineQuantityRequest
                {
                    NewQuantity = 4
                }
            );

            // 4) Verificar 204 No Content
            updResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
