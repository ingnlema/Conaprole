namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Request model for creating multiple orders in a single atomic transaction
/// </summary>
public record BulkCreateOrdersRequest
{
    /// <summary>
    /// List of orders to create. All orders will be created atomically or none will be created if any fails.
    /// </summary>
    public List<CreateOrderRequest> Orders { get; set; } = new();
}