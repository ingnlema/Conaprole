namespace Conaprole.Orders.Api.Controllers.Orders;

/// <summary>
/// Request model for adding a new line to an existing order
/// </summary>
/// <param name="ExternalProductId">External identifier for the product (SKU)</param>
/// <param name="Quantity">Quantity of the product to add to the order</param>
public record AddOrderLineRequest(
    string ExternalProductId,
    int Quantity);