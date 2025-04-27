using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Orders.RemoveOrderLine;

public record RemoveOrderLineCommand(Guid OrderId, Guid ProductId) : ICommand;