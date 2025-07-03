using Swashbuckle.AspNetCore.Filters;

namespace Conaprole.Orders.Api.Controllers.Orders.Examples;

/// <summary>
/// Example for CreateOrderRequest
/// </summary>
public class CreateOrderRequestExample : IExamplesProvider<CreateOrderRequest>
{
    public CreateOrderRequest GetExamples()
    {
        return new CreateOrderRequest(
            PointOfSalePhoneNumber: "+59891234567",
            DistributorPhoneNumber: "+59899887766",
            City: "Montevideo",
            Street: "18 de Julio 1234",
            ZipCode: "11000",
            CurrencyCode: "UYU",
            OrderLines: new List<OrderLineRequest>
            {
                new("LECHE-ENTERA-1L", 10),
                new("YOGURT-FRUTILLA-200G", 5),
                new("QUESO-MUZZARELLA-500G", 2)
            }
        );
    }
}