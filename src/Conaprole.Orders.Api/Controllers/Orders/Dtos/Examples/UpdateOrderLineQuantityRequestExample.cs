using Swashbuckle.AspNetCore.Filters;

namespace Conaprole.Orders.Api.Controllers.Orders.Examples;

/// <summary>
/// Example for UpdateOrderLineQuantityRequest
/// </summary>
public class UpdateOrderLineQuantityRequestExample : IExamplesProvider<UpdateOrderLineQuantityRequest>
{
    public UpdateOrderLineQuantityRequest GetExamples()
    {
        return new UpdateOrderLineQuantityRequest
        {
            NewQuantity = 5
        };
    }
}