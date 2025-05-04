
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

using Conaprole.Orders.Api.Controllers.Orders;
using Conaprole.Orders.Api.FunctionalTests.Infrastructure;
using Conaprole.Orders.Api.FunctionalTests.Products;
using Conaprole.Orders.Application.Orders.GetOrder;

namespace Conaprole.Orders.Api.FunctionalTests.Orders
{
    [Collection("ApiCollection")]
    public class CreateOrderTest : BaseFunctionalTest
    {
        public CreateOrderTest(FunctionalTestWebAppFactory factory)
            : base(factory) { }

        [Fact]
        public async Task CreateOrder_And_GetById_ShouldReturnCreatedThenOk()
        {
            // 1) Crear el producto global y obtener su GUID interno
            var productId = await ProductData.CreateAsync(HttpClient);

            // 2) Definir la línea usando el ExternalProductId de ProductData
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
            var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Act – Get by ID
            var getResponse = await HttpClient.GetAsync($"api/Orders/{id}");

            // Assert – OK y contenido
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
            order!.Id.Should().Be(id);
            order.Distributor.Should().Be("TestDistributor");
        }
    }
}
