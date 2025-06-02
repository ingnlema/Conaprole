using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;
using Conaprole.Orders.Application.Orders.GetOrders;
using Conaprole.Orders.Domain.Orders;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class GetOrdersFilterByStatusTest : BaseFunctionalTest
    {
        public GetOrdersFilterByStatusTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrders_FilterByStatus_ShouldReturnOnlyConfirmed()
        {
            var distributorPhone = "+59892222222";
            var pointOfSalePhone = "+59895555555";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // 1) Crear el producto global y generar la línea de pedido
            await ProductData.CreateAsync(HttpClient);
            var line = ProductData.OrderLine(1);

            // 2) Crear la orden usando ese producto
            var createReq = new CreateOrderRequest(
                pointOfSalePhone,
                distributorPhone,
                "City",
                "Street",
                "22222",
                "UYU",
                new List<OrderLineRequest> { line }
            );
            var createResp = await HttpClient.PostAsJsonAsync("api/orders", createReq);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await createResp.Content.ReadFromJsonAsync<Guid>();

            // 3) Actualizar estado a Confirmed
            var updateResp = await HttpClient.PutAsJsonAsync(
                $"api/orders/{id}/status",
                new UpdateOrderStatusRequest((int)Status.Confirmed)
            );
            updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 4) Filtrar por estado Confirmed
            var response = await HttpClient.GetAsync($"api/orders?Status={(int)Status.Confirmed}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await response.Content.ReadFromJsonAsync<List<OrderSummaryResponse>>();
            list.Should().NotBeNull();
            list.Should().Contain(o => o.Id == id, "la orden actualizada debe aparecer en el filtro");
            list.Should().OnlyContain(o => o.Status == (int)Status.Confirmed);
        }
    }
}
