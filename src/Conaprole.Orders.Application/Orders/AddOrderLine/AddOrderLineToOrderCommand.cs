
using Conaprole.Orders.Domain.Products;
using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.AddOrderLine
{
    /// <summary>
    /// Comando para agregar una línea a la orden:
    /// sólo OrderId, ExternalProductId y Quantity.
    /// </summary>
    public record AddOrderLineToOrderCommand(
        Guid OrderId,
        ExternalProductId ExternalProductId,
        int Quantity
    ) : ICommand<Guid>;
}