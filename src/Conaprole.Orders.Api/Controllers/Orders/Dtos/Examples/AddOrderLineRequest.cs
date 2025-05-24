// File: Conaprole.Orders.Api/Controllers/Orders/AddOrderLineRequest.cs
namespace Conaprole.Orders.Api.Controllers.Orders
{
    /// <summary>
    /// Request para agregar una línea a la orden:
    /// sólo ExternalProductId (SKU) y Quantity.
    /// </summary>
    public record AddOrderLineRequest(
        string ExternalProductId,
        int Quantity
    );
}