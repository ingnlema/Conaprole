using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Orders.Events;

public record OrderLineQuantityUpdatedEvent(Guid OrderId, Guid LineId, int NewQuantity) : IDomainEvent;