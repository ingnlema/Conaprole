
using System.Net;
using System.Net.Http.Json;
using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;
using Conaprole.Orders.Application.Orders.GetOrder;
using FluentAssertions;


namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class AddOrderLineTest : BaseFunctionalTest
    {
        public AddOrderLineTest(FunctionalTestWebAppFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task CreateOrder_And_GetById_ShouldReturnCreatedThenOk()
        {
            // 1) Crear el producto global via API y obtener su GUID interno
            var productId = await ProductData.CreateAsync(HttpClient);

            // 2) Armar la línea usando el ExternalProductId de ProductData
            var line = ProductData.OrderLine(quantity: 2);

            // 3) Crear la orden con esa línea
            var request = new CreateOrderRequest(
                "+59891234567",
                "TestDistributor",
                "Montevideo",
                "Test Street",
                "11200",
                "UYU",
                new List<OrderLineRequest> { line }
            );

            // Act – Create
            var createResponse = await HttpClient.PostAsJsonAsync("api/Orders", request);

            // Assert – Created
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Act – Get by ID
            var getResponse = await HttpClient.GetAsync($"api/Orders/{orderId}");

            // Assert – OK y contenido correcto
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
            order!.Id.Should().Be(orderId);
            order.Distributor.Should().Be("TestDistributor");
        }
    }
}
