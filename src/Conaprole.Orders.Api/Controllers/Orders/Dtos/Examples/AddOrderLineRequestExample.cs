using Swashbuckle.AspNetCore.Filters;

namespace Conaprole.Orders.Api.Controllers.Orders.Examples;

/// <summary>
/// Example for AddOrderLineRequest
/// </summary>
public class AddOrderLineRequestExample : IExamplesProvider<AddOrderLineRequest>
{
    public AddOrderLineRequest GetExamples()
    {
        return new AddOrderLineRequest(
            ExternalProductId: "DULCE-LECHE-500G",
            Quantity: 3
        );
    }
}