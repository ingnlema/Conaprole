
namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Request model for updating an order's status
/// </summary>
/// <param name="NewStatus">The new status value for the order</param>
public record UpdateOrderStatusRequest(
    int? NewStatus);