using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;
using Conaprole.Orders.Application.Orders.GetOrder;
using Conaprole.Orders.Domain.Orders;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class UpdateOrderStatusTest : BaseFunctionalTest
    {
        public UpdateOrderStatusTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task UpdateStatus_ShouldReturnNoContent()
        {
            var distributorPhone = "+59894444444";
            var pointOfSalePhone = "+59894441111";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // Set authorization header for protected endpoints
            await SetAuthorizationHeaderAsync();

            // 1) Crear el producto global y obtener su GUID interno
            var productId = await ProductData.CreateAsync(HttpClient);

            // 2) Crear la orden usando el producto global en la línea inicial
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/Orders",
                new CreateOrderRequest(
                    pointOfSalePhone,
                    distributorPhone,
                    "City",
                    "Street",
                    "44444",
                    "UYU",
                    new List<OrderLineRequest>
                    {
                        ProductData.OrderLine(quantity: 1)
                    }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResp.Content.ReadFromJsonAsync<Guid>();

            // 3) Actualizar el estado a Confirmed
            var resp = await HttpClient.PutAsJsonAsync(
                $"api/Orders/{orderId}/status",
                new UpdateOrderStatusRequest((int)Status.Confirmed)
            );
            resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 4) Verificar en el GET que el estado fue actualizado
            var getResp = await HttpClient.GetAsync($"api/Orders/{orderId}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getResp.Content.ReadFromJsonAsync<OrderResponse>();
            order!.Status.Should().Be((int)Status.Confirmed);
        }
    }
}
