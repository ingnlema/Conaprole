using Swashbuckle.AspNetCore.Filters;

namespace Conaprole.Orders.Api.Controllers.Orders.Examples;

public class UpdateOrderStatusRequestExample : IExamplesProvider<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequest GetExamples()
    {
        // Muestra 1 como valor de ejemplo
        return new UpdateOrderStatusRequest(1);
    }
}