using System.ComponentModel.DataAnnotations;

namespace Conaprole.Orders.Api.Controllers.Orders;

public record UpdateOrderLineQuantityRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int NewQuantity { get; init; }
}