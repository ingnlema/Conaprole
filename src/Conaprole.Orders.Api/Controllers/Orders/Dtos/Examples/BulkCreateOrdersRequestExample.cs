using Swashbuckle.AspNetCore.Filters;

namespace Conaprole.Orders.Api.Controllers.Orders.Examples;

public class BulkCreateOrdersRequestExample : IExamplesProvider<BulkCreateOrdersRequest>
{
    public BulkCreateOrdersRequest GetExamples()
    {
        return new BulkCreateOrdersRequest
        {
            Orders = new List<CreateOrderRequest>
            {
                new CreateOrderRequest(
                    "+59891234567",
                    "+59899887766", 
                    "Montevideo",
                    "Calle Test 123",
                    "11000",
                    "UYU",
                    new List<OrderLineRequest>
                    {
                        new OrderLineRequest("GLOBAL_SKU", 2)
                    }
                ),
                new CreateOrderRequest(
                    "+59891234568",
                    "+59899887767",
                    "Punta del Este", 
                    "Avenida Principal 456",
                    "20100",
                    "USD",
                    new List<OrderLineRequest>
                    {
                        new OrderLineRequest("GLOBAL_SKU", 1),
                        new OrderLineRequest("GLOBAL_SKU_2", 3)
                    }
                )
            }
        };
    }
}