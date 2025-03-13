using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Users.Events;

public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;