using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(Guid UserId) : IQuery<IReadOnlyList<PermissionResponse>>;