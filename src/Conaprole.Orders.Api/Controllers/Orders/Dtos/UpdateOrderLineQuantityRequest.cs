using System.ComponentModel.DataAnnotations;

namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Request model for updating the quantity of an order line
/// </summary>
public record UpdateOrderLineQuantityRequest
{
    /// <summary>
    /// The new quantity for the order line. Must be greater than 0.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int NewQuantity { get; init; }
}