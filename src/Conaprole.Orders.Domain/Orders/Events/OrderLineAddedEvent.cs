using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Orders.Events;

public record OrderLineAddedEvent(Guid OrderId, Guid LineId, Guid ProductId, int Quantity) : IDomainEvent;