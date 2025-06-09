using Conaprole.Orders.Application.Abstractions.Messaging;

namespace Conaprole.Orders.Application.Users.GetAllPermissions;

public sealed record GetAllPermissionsQuery : IQuery<IReadOnlyList<PermissionResponse>>;