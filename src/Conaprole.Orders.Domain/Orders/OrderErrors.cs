using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound = new(
        "Order.Found",
        "The order with the specified identifier was not found");
    
}
