namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Request model for an order line item
/// </summary>
/// <param name="ExternalProductId">External identifier for the product (SKU)</param>
/// <param name="Quantity">Quantity of the product to order</param>
public record OrderLineRequest(
    string ExternalProductId,
    int Quantity);