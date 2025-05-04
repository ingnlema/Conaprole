using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.RemoveOrderLine;

public record RemoveOrderLineFromOrderCommand(
    Guid OrderId,
    Guid OrderLineId
) : ICommand<Guid>;