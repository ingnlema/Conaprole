using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.GetAllRoles;

public sealed record GetAllRolesQuery : IQuery<IReadOnlyList<RoleResponse>>;