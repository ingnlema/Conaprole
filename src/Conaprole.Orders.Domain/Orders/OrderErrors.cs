using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound = new(
        "Order.NotFound",
        "The order with the specified identifier was not found");
    
    public static Error ProductNotFound = new(
        "Order.ProductNotFound",
        "The product with the specified external ID was not found.");

    public static Error DuplicateProductInOrder = new(
        "Order.DuplicateProduct",
        "The product is already part of this order.");
    
    public static readonly Error LineNotFound = new(
        "OrderLine.NotFound", "Order or Line not found.");
    
    public static readonly Error LastOrderLineCannotBeRemoved = new(
        "OrderLine.LastLine",
        "Cannot remove the last order line.");
}
