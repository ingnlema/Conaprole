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
    public class RemoveOrderLineTest : BaseFunctionalTest
    {
        public RemoveOrderLineTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task RemoveLine_ShouldReturnNoContent()
        {
            // 1) Crear el producto global y obtener su GUID
            var productId = await ProductData.CreateAsync(HttpClient);

            // 2) Construir la línea inicial
            var line = ProductData.OrderLine(quantity: 1);

            // 3) Crear la orden con esa única línea
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/Orders",
                new CreateOrderRequest(
                    "+59897777777",
                    "LineDist",
                    "City",
                    "Street",
                    "77777",
                    "UYU",
                    new List<OrderLineRequest> { line }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResp.Content.ReadFromJsonAsync<Guid>();

            // 4) Eliminar la línea usando el productId interno
            var remResp = await HttpClient.DeleteAsync(
                $"api/Orders/{orderId}/lines/{productId}"
            );

            // 5) Verificar 204 No Content
            remResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}