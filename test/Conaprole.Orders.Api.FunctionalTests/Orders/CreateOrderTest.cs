using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
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
            : base(factory)
        {
        }

        [Fact]
        public async Task CreateOrder_And_GetById_ShouldReturnCreatedThenOk()
        {
            var distributorPhone = "+59899887766";
            var pointOfSalePhone = "+59891234567";

            await CreateDistributorAsync(distributorPhone);
            await CreatePointOfSaleAsync(pointOfSalePhone);

            var productId = await ProductData.CreateAsync(HttpClient);
            var line = ProductData.OrderLine(quantity: 2);
            var request = new CreateOrderRequest(
                pointOfSalePhone,
                distributorPhone,
                "Montevideo",
                "Test Street",
                "11200",
                "UYU",
                new List<OrderLineRequest> { line }
            );
            var createResponse = await HttpClient.PostAsJsonAsync("api/Orders", request);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await createResponse.Content.ReadFromJsonAsync<Guid>();
            var getResponse = await HttpClient.GetAsync($"api/Orders/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var order = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
            order!.Id.Should().Be(id);
            order.DistributorPhoneNumber.Should().Be(distributorPhone);
        }
    }
}