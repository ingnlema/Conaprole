
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Conaprole.Orders.Api.Controllers.Orders
{
    public record UpdateOrderStatusRequest(
        int? NewStatus
    );
}