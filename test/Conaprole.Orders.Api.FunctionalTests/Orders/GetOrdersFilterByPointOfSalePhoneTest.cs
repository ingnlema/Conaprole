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
    public class GetOrdersFilterByPointOfSalePhoneTest : BaseFunctionalTest
    {
        public GetOrdersFilterByPointOfSalePhoneTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task GetOrders_FilterByPointOfSalePhone_ShouldReturnOnlyMatching()
        {
            var distributorPhone = "+59891112222";
            var pointOfSalePhone = "+59891111111";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            // 1) Sembrar el producto global y crear la línea
            await ProductData.CreateAsync(HttpClient);
            var line = ProductData.OrderLine(1);

            // 2) Preparar datos
            // var phone = "+59891111111";

            // 3) Crear la orden usando el producto global
            var createResp = await HttpClient.PostAsJsonAsync(
                "api/orders",
                new CreateOrderRequest(
                    pointOfSalePhone,
                    distributorPhone,
                    "City",
                    "Street",
                    "11111",
                    "UYU",
                    new List<OrderLineRequest> { line }
                )
            );
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResp.Content.ReadFromJsonAsync<Guid>();

            // 4) Filtrar por número de punto de venta
            var response = await HttpClient.GetAsync(
                $"api/orders?PointOfSalePhoneNumber={Uri.EscapeDataString(pointOfSalePhone)}"
            );
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // 5) Verificar contenido
            var list = await response.Content.ReadFromJsonAsync<List<OrderSummaryResponse>>();
            list.Should().NotBeNull();
            list.Should().Contain(o => o.Id == orderId, "la lista debe incluir la orden recién creada");
            list.Should().OnlyContain(o => o.PointOfSalePhoneNumber == pointOfSalePhone);
        }
    }
}
