using Conaprole.Orders.Domain.Abstractions;

namespace Conaprole.Orders.Domain.Users.Events;

public sealed record UserRoleRemovedDomainEvent(Guid UserId, int RoleId) : IDomainEvent;