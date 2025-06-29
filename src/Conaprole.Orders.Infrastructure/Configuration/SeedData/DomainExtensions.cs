using Conaprole.Orders.Domain.Users;

namespace Conaprole.Orders.Infrastructure.Configuration.SeedData;

public static class DomainExtensions
{
    public static string ToRoleName(this ApplicationRole role)
    {
        return role.ToString();
    }

    public static string[] ToRoleNames(this ApplicationRole[] roles)
    {
        return roles.Select(r => r.ToRoleName()).ToArray();
    }

    public static string ToPermissionName(this ApplicationPermission permission)
    {
        return RolePermissionMapping.GetPermissionName(permission);
    }

    public static string[] ToPermissionNames(this ApplicationPermission[] permissions)
    {
        return permissions.Select(p => p.ToPermissionName()).ToArray();
    }
}