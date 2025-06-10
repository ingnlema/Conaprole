using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<IReadOnlyList<RoleResponse>>;