using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Orders.Events;

public record OrderLineRemovedEvent(Guid OrderId, Guid LineId, Guid ProductId) : IDomainEvent;