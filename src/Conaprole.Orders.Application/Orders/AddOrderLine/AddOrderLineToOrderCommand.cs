using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.AddOrderLine;

public record AddOrderLineToOrderCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string CurrencyCode
) : ICommand<Guid>;