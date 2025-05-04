
using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.UpdateOrderLineQuantity;

public record UpdateOrderLineQuantityCommand(
    Guid OrderId,
    Guid OrderLineId,
    int NewQuantity
) : ICommand<Guid>;