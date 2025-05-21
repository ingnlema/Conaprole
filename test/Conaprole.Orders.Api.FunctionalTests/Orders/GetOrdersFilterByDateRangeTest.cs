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
    public class GetOrdersFilterByDateRangeTest : BaseFunctionalTest
    {
        public GetOrdersFilterByDateRangeTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrders_FilterByDateRange_ShouldFilterByFrom()
        {
            var distributorPhone = "+59899887777";
            var pointOfSalePhone = "+59893333333";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // 1) Seedear el producto global y construir la línea de pedido
            await ProductData.CreateAsync(HttpClient);
            var line = ProductData.OrderLine(1);

            // 2) Marcar el tiempo antes de crear la orden
            var before = DateTime.UtcNow.AddMinutes(-1);

            // 3) Crear la orden con la línea apuntando al producto global
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/Orders",
                new CreateOrderRequest(
                    pointOfSalePhone,
                    distributorPhone,
                    "City",
                    "Street",
                    "33333",
                    "UYU",
                    new List<OrderLineRequest> { line }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await createResp.Content.ReadFromJsonAsync<Guid>();

            // 4) Esperar para que exista una marca de tiempo posterior
            await Task.Delay(1000);
            var after = DateTime.UtcNow;

            // Act & Assert: filtro desde 'before' debería incluir la orden
            var resp1 = await HttpClient.GetAsync(
                $"api/Orders?From={Uri.EscapeDataString(before.ToString("o"))}"
            );
            resp1.StatusCode.Should().Be(HttpStatusCode.OK);
            var list1 = await resp1.Content.ReadFromJsonAsync<List<OrderSummaryResponse>>();
            list1.Should().Contain(o => o.Id == id);

            // Act & Assert: filtro desde 'after' NO debería incluirla
            var resp2 = await HttpClient.GetAsync(
                $"api/Orders?From={Uri.EscapeDataString(after.ToString("o"))}"
            );
            resp2.StatusCode.Should().Be(HttpStatusCode.OK);
            var list2 = await resp2.Content.ReadFromJsonAsync<List<OrderSummaryResponse>>();
            list2.Should().NotContain(o => o.Id == id);
        }
    }
}
